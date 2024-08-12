using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Rendering;
using static gmtk2024.Runtime.Renderer.Pixelate.PixelSnap;

namespace gmtk2024.Runtime.Renderer.Pixelate
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class Pixelate : MonoBehaviour
    {
        private const u8 k_MIN_PIXELS_PER_UNIT = 1;
        private const u8 k_MAX_PIXELS_PER_UNIT = 64;

        [Range(k_MIN_PIXELS_PER_UNIT, k_MAX_PIXELS_PER_UNIT)]
        [Tooltip("Density of pixels per world unit")]
        public u8 PixelsPerUnit;

        [Tooltip("Enable snapping of registered transforms")]
        public bool EnableTransformSnapping;

        private Camera _Camera;
        private f32 _PreviousFieldOfView;
        private f32 _PreviousOrthographicSize;
        private u8 _PreviousPixelsPerUnit;
        private float3 _PreviousPosition;

        // Change detection
        private RenderingPath _PreviousRenderingPath;
        private quaternion _PreviousRotation;

        private Rect _RenderViewport,
            _DisplayViewport = new(0, 0, 0, 0);

        private JobHandle _SnapJobHandle;

        // Snapping

        private DynamicTransformAccessArray _Snappables;
        private JobHandle _UnsnapJobHandle;

        // Total displacement of the camera in percentage of the render size when snapped to pixel grid-space.
        // This is updated if sub-pixel camera smoothing is enabled.
        [ShowInInspector]
        public Vector2 SnapDisplacement { get; private set; }

        public f32 OrthographicSize => _Camera.orthographicSize;

        public f32 FieldOfView => _Camera.fieldOfView;

        public bool IsOrthographic => _Camera.orthographic;

        [ShowInInspector]
        public Rect RenderViewport => _RenderViewport;

        [ShowInInspector]
        public Rect DisplayViewport => _DisplayViewport;

        [ShowInInspector]
        public f32 UnitsPerPixel => 1.0f / PixelsPerUnit;

        [ShowInInspector]
        public f32 ScaleFactor { get; private set; } = 1.0f;

        public bool IsDirty { get; internal set; } = true;

        [ShowInInspector]
        private i32 SnappableCount => _Snappables.IsCreated ? _Snappables.Length : 0;

        [ShowInInspector]
        private bool SnappingActive => EnableTransformSnapping && _Snappables.IsCreated;

        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            if (EnableTransformSnapping)
                Snappable_Allocate();
        }

        private void OnEnable()
        {
            _Camera = GetComponent<Camera>();
            RenderPipelineManager.beginCameraRendering += PreRenderCamera;
            RenderPipelineManager.endCameraRendering += PostRenderCamera;
            IsDirty = true;

            if (EnableTransformSnapping)
                Snappable_Allocate();
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= PreRenderCamera;
            RenderPipelineManager.endCameraRendering -= PostRenderCamera;

            IsDirty = true;
            _Camera.rect = new Rect(0, 0, 1, 1);
            SnapDisplacement = float2.zero;

            Snappable_Dispose();
        }

        private void OnDestroy()
        {
            Snappable_Dispose();
        }

        private void PreRenderCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != _Camera || !enabled)
                return;

            DetectDirtyAndUpdate();

            _PreviousPosition = _Camera.transform.position;
            _PreviousRotation = _Camera.transform.rotation;
            _Camera.pixelRect = _RenderViewport;

            Snap(
                _Camera.transform.rotation,
                UnitsPerPixel,
                _Camera.transform,
                out var displacement
            );

            var displacementInPixels = displacement.xy * PixelsPerUnit;

            SnapDisplacement = new float2(
                displacementInPixels.x / _RenderViewport.width,
                displacementInPixels.y / _RenderViewport.height
            );

            if (!SnappingActive)
                return;

            _SnapJobHandle = _Snappables.ScheduleReadWriteTransforms(
                new SnapTransformsJob
                {
                    CameraRotation = _Camera.transform.rotation,
                    UnitsPerPixel = UnitsPerPixel
                },
                _SnapJobHandle
            );

            _Snappables.WaitTillJobsComplete();
        }

        private void PostRenderCamera(ScriptableRenderContext context, Camera camera)
        {
            if (camera != _Camera)
                return;

            _Camera.ResetProjectionMatrix();
            _Camera.pixelRect = _DisplayViewport;
            _Camera.transform.SetPositionAndRotation(_PreviousPosition, _PreviousRotation);

            if (!SnappingActive)
                return;

            _UnsnapJobHandle = _Snappables.ScheduleWriteTransforms(
                new UnsnapTransformsJob { Positions = _Snappables.Positions.AsArray() },
                _UnsnapJobHandle
            );
            _Snappables.WaitTillJobsComplete();
        }

        private void DetectDirtyAndUpdate()
        {
            DetectDirtyChanges();

            if (IsDirty)
            {
                UpdateRenderViewport();
                IsDirty = false;
            }

            if (EnableTransformSnapping && !_Snappables.IsCreated)
                Snappable_Allocate();

            assert(IsDirty == false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DetectDirtyChanges()
        {
            if (_DisplayViewport.width != Screen.width || _DisplayViewport.height != Screen.height)
            {
                _DisplayViewport.width = Screen.width;
                _DisplayViewport.height = Screen.height;
                IsDirty = true;
            }

            IsDirty =
                IsDirty
                || _PreviousOrthographicSize != OrthographicSize
                || _PreviousFieldOfView != FieldOfView
                || _PreviousRenderingPath != _Camera.actualRenderingPath
                || _PreviousPixelsPerUnit != PixelsPerUnit;

            if (!IsDirty)
                return;

            _PreviousOrthographicSize = OrthographicSize;
            _PreviousFieldOfView = FieldOfView;
            _PreviousRenderingPath = _Camera.actualRenderingPath;
            _PreviousPixelsPerUnit = PixelsPerUnit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateRenderViewport()
        {
            UpdateScaleFactor();

            _RenderViewport.width = (i32)(_DisplayViewport.width / ScaleFactor);
            _RenderViewport.height = (i32)(_DisplayViewport.height / ScaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateScaleFactor()
        {
            var pixelScale = IsOrthographic ? PixelScale_Orthographic() : PixelScale_Perspective();

            var scaleFactorX = math.max(1, DisplayViewport.width / pixelScale);
            var scaleFactorY = math.max(1, DisplayViewport.height / pixelScale);
            var scaleFactor = math.min(scaleFactorX, scaleFactorY);

            const i32 k_MIN_SCALE_FACTOR = 1;
            ScaleFactor = math.max(k_MIN_SCALE_FACTOR, scaleFactor);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private f32 PixelScale_Orthographic()
        {
            return 2 * PixelsPerUnit * OrthographicSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private f32 PixelScale_Perspective()
        {
            const f32 k_DEFAULT_DISTANCE = 10.0f;
            var halfVerticalFoV = math.tan(math.radians(FieldOfView * 0.5f));
            var verticalSizeAtDefaultDistance = 2 * k_DEFAULT_DISTANCE * halfVerticalFoV;
            return PixelsPerUnit * verticalSizeAtDefaultDistance;
        }

        private void Snappable_Allocate()
        {
            if (_Snappables.IsCreated)
                return;

            const i32 k_DEFAULT_CAPACITY = 1_000;
            _Snappables = new DynamicTransformAccessArray(k_DEFAULT_CAPACITY, Allocator.Persistent);

            var maybe_pixelate = FindObjectsByType<SnapTransform>(FindObjectsSortMode.None)
                .Where(x => !x.IsRegistered);
            foreach (var pixelate in maybe_pixelate)
                pixelate.Register(this);
        }

        private void Snappable_Dispose()
        {
            if (_Snappables.IsCreated)
                _Snappables.Dispose();
        }

        internal i32 Snappable_Register(Transform transform)
        {
            if (!_Snappables.IsCreated)
                return -1;
            var index = _Snappables.Register(transform);
            return index;
        }

        internal void Snappable_Unregister(i32 index)
        {
            if (!_Snappables.IsCreated)
                return;
            _Snappables.Deregister(index);
        }
    }
}
