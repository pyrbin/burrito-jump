using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace gmtk2024.Runtime.Renderer.Ascii
{
    // inspired by https://www.youtube.com/watch?v=gg40RWiaHRY&list=PLUKV95Q13e_Un6ADYZ9NyWJ3W1R2cbCYv&index=16
    [DisallowMultipleRendererFeature(nameof(AsciiFeature))]
    [Tooltip("Ascii Feature")]
    public class AsciiFeature : ScriptableRendererFeature
    {
        public ComputeShader ComputeShader;
        public Texture2D AsciiLUT;

        internal AsciiComputePass _ComputePass;

        public override void Create()
        {
            name = "Ascii Feature";

            _ComputePass = new AsciiComputePass(ComputeShader, AsciiLUT);
            _ComputePass.renderPassEvent = RenderPassEvent.AfterRendering - 9;
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            if (
                !renderingData.cameraData.camera.gameObject.TryGetComponent(
                    out Pixelate.Pixelate pixelate
                )
                || renderingData.cameraData.cameraType != CameraType.Game
            )
                return;

            renderer.EnqueuePass(_ComputePass);
        }

        protected override void Dispose(bool disposing)
        {
            _ComputePass.Dispose();
        }

        internal class AsciiComputePass : ScriptableRenderPass, IDisposable
        {
            private readonly Texture2D _AsciiTexture;
            private readonly RTHandle _AsciiTextureHandle;
            private readonly ComputeShader _ComputeShader;

            public AsciiComputePass(ComputeShader computeShader, Texture2D asciiTex)
            {
                _ComputeShader = computeShader;
                _AsciiTexture = asciiTex;
                _AsciiTextureHandle = RTHandles.Alloc(_AsciiTexture);
            }

            public void Dispose()
            {
                _AsciiTextureHandle?.Release();
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData
            )
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                var source = resourceData.cameraColor;
                var downscaled = resourceData.activeColorTexture;
                var width = source.GetDescriptor(renderGraph).width;
                var ascii = renderGraph.ImportTexture(_AsciiTextureHandle);

                using var builder = renderGraph.AddComputePass<PassData>(
                    "Ascii Pass",
                    out var passData,
                    profilingSampler
                );

                passData.ComputeShader = _ComputeShader;
                passData.Destination = source;
                passData.AsciiTex = ascii;

                builder.UseTexture(passData.Destination, AccessFlags.ReadWrite);
                builder.UseTexture(passData.AsciiTex);

                builder.AllowPassCulling(false);
                builder.SetRenderFunc(
                    (PassData data, ComputeGraphContext ctx) =>
                    {
                        var kernel = data.ComputeShader.FindKernel("CS_Main");

                        ctx.cmd.SetComputeTextureParam(
                            data.ComputeShader,
                            kernel,
                            "_AsciiTex",
                            data.AsciiTex
                        );

                        ctx.cmd.SetComputeTextureParam(
                            data.ComputeShader,
                            kernel,
                            "_Destination",
                            data.Destination
                        );

                        ctx.cmd.DispatchCompute(
                            data.ComputeShader,
                            kernel,
                            Math.CeilToInt(width / 8),
                            Math.CeilToInt(width / 8),
                            1
                        );
                    }
                );
            }

            private class PassData
            {
                public TextureHandle AsciiTex;
                public ComputeShader ComputeShader;
                public TextureHandle Destination;
            }
        }
    }
}
