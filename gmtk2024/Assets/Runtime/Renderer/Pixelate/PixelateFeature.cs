using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace gmtk2024.Runtime.Renderer.Pixelate
{
    [DisallowMultipleRendererFeature(nameof(PixelateFeature))]
    [Tooltip("Pixelate Feature")]
    public class PixelateFeature : ScriptableRendererFeature
    {
        private Pixelate? _Camera;
        private UniversalRenderPipelineAsset? _UniversalRenderPipelineAsset;
        private UpscalePass? _UpscalePass;

        public Pixelate Camera => _Camera!;

        public override void Create()
        {
            name = "Pixelate Feature";

            _UniversalRenderPipelineAsset =
                GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

            _UpscalePass = new UpscalePass
            {
                renderPassEvent = RenderPassEvent.AfterRendering - 10
            };
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            _UniversalRenderPipelineAsset!.upscalingFilter = UpscalingFilterSelection.Linear;
            _UniversalRenderPipelineAsset!.renderScale = 1f;

            if (
                !renderingData.cameraData.camera.gameObject.TryGetComponent(out Pixelate pixelate)
                || renderingData.cameraData.cameraType != CameraType.Game
            )
                return;

            _Camera = pixelate;
            _UpscalePass!.InitCamera(_Camera);
            renderer.EnqueuePass(_UpscalePass!);
        }

        private class UpscalePass : ScriptableRenderPass
        {
            private const string k_UpscaledTextureName = "_Pixelate_UpscaledTexture";
            private Pixelate? _Camera;
            private readonly Material _Material;
            private RenderTextureDescriptor _UpscaledTextureDescriptor;

            public UpscalePass()
            {
                _UpscaledTextureDescriptor = new RenderTextureDescriptor(
                    (i32?)_Camera?.DisplayViewport.width ?? Screen.width,
                    (i32?)_Camera?.DisplayViewport.height ?? Screen.height,
                    RenderTextureFormat.Default,
                    0
                );

                const string k_ShaderPath = "Hidden/Pixelate/Upscale Blit";
                if (_Material == null)
                    _Material = CoreUtils.CreateEngineMaterial(k_ShaderPath);

                ConfigureInput(ScriptableRenderPassInput.Color);
            }

            internal void InitCamera(Pixelate camera)
            {
                _Camera = camera;
            }

            private static void ExecutePass(PassData data, UnsafeGraphContext context)
            {
                var cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                cmd.SetRenderTarget(data.Upscaled);
                Blitter.BlitTexture(cmd, data.Source, data.ScaleBias, data.Material, 0);
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData
            )
            {
                assert(_Camera.IsNotNull());

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();

                var source = resourceData.activeColorTexture;

                _UpscaledTextureDescriptor.width = (i32)_Camera!.DisplayViewport.width;
                _UpscaledTextureDescriptor.height = (i32)_Camera!.DisplayViewport.height;
                _UpscaledTextureDescriptor.depthBufferBits = 0;
                _UpscaledTextureDescriptor.msaaSamples = (i32)MSAASamples.None;
                _UpscaledTextureDescriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
                _UpscaledTextureDescriptor.enableRandomWrite = false;

                var upscaled = UniversalRenderer.CreateRenderGraphTexture(
                    renderGraph,
                    _UpscaledTextureDescriptor,
                    k_UpscaledTextureName,
                    true,
                    FilterMode.Trilinear
                );

#if UNITY_WEBGL && !UNITY_EDITOR
                // var scaleBias = new float4(1, -1f, 0f, 1f);
                var scaleBias = new float4(1, 1f, 0f, 0f);
                scaleBias.z += _Camera!.SnapDisplacement.x;
                scaleBias.w += _Camera!.SnapDisplacement.y;
#else
                // var scaleBias = new float4(1, -1f, 0f, 1f);
                var scaleBias = new float4(1, 1f, 0f, 0f);
                scaleBias.z += _Camera!.SnapDisplacement.x;
                scaleBias.w += _Camera!.SnapDisplacement.y;
#endif

                using (
                    var builder = renderGraph.AddUnsafePass<PassData>(
                        "Pixelate Upscale Pass",
                        out var passData,
                        profilingSampler
                    )
                )
                {
                    passData.Material = _Material;
                    passData.Source = source;
                    passData.Upscaled = upscaled;
                    passData.ScaleBias = scaleBias;

                    builder.UseTexture(passData.Source);
                    builder.UseTexture(passData.Upscaled, AccessFlags.Write);

                    builder.AllowPassCulling(false);

                    builder.SetRenderFunc(
                        (PassData data, UnsafeGraphContext context) => ExecutePass(data, context)
                    );
                }

                // #FB_NOTE: 'hacks' to make this work nicely with the URP render pipeline
                // Make sure that the 'source' used for final blit is the upscaled texture.
                resourceData.cameraColor = upscaled;
                // Set the pixel rect to the display viewport when final blit pass is executed.
                // NOTE: This is reset to RenderViewport in the Pixelate camera component.
                cameraData.SetPixelRect(_Camera!.DisplayViewport);
            }

            private class PassData
            {
                public Material? Material;
                public float4 ScaleBias;
                public TextureHandle Source;
                public TextureHandle Upscaled;
            }
        }
    }
}
