namespace gmtk2024.Runtime.Renderer.Pixelate;

public class SnapTransform : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    public i32 _InstanceId = -1;

    internal Pixelate? _RegisteredPixelate;

    public bool IsRegistered => _InstanceId != -1;

    public void Start()
    {
        Register(None);
    }

    public void OnEnable()
    {
        Register(None);
    }

    public void OnDisable()
    {
        Unregister();
    }

    public void OnDestroy()
    {
        Unregister();
    }

    [Button("Unregister")]
    public void Test_Unregister()
    {
        Unregister();
    }

    public void Register(Option<Pixelate> toPixelate)
    {
        Unregister();

        toPixelate = toPixelate.Match(
            Some: v => v,
            None: () =>
                FindObjectsByType<Pixelate>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.EnableTransformSnapping)
                    .ToOption()
        );

        if (!toPixelate.IsSome(out var pixelate))
            return;

        _InstanceId = pixelate.Snappable_Register(transform);
        _RegisteredPixelate = pixelate;
    }

    public void Unregister()
    {
        if (_InstanceId == -1)
            return;

        if (_RegisteredPixelate is not null)
            _RegisteredPixelate.Snappable_Unregister(_InstanceId);

        _InstanceId = -1;
    }
}
