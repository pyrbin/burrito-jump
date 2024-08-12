using R3;

namespace gmtk2024.Runtime.Stat;

[Serializable]
public abstract class Resource : Stat
{
    [SerializeField]
    internal Current? _Current;

    [SerializeField]
    internal f32 _Fraction;

    protected Resource()
    {
        _Current ??= new Current(this);
        _Current._Parent = this;
        _Fraction = Value <= 0 ? 1 : Math.Clamp01(Current.Value / Value);
    }

    public Current Current => _Current!;

    public f32 Fraction => _Fraction;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void OnValueChanged(f32 value)
    {
        if (_Current is not null)
            _Current._Value.Value = value * _Fraction;
    }

    public new void Dispose()
    {
        base.Dispose();
        Current.Dispose();
    }
}

[Serializable]
public sealed class Current : Observable<f32>, IDisposable
{
    [SerializeField]
    internal SerializableReactiveProperty<f32> _Value = new();

    internal Resource _Parent;

    private IDisposable _Subscription;

    public Current(Resource parent)
    {
        _Parent = parent;
        _Value ??= new SerializableReactiveProperty<f32>(parent.Value);
        _Subscription = _Value.Subscribe(OnCurrentChanged);
    }

    public f32 Value
    {
        get => _Parent._Round(_Value.Value);
        set => _Value.Value = math.clamp(value, 0, _Parent.Value);
    }

    public void Dispose()
    {
        _Value.Dispose();
        _Subscription.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OnCurrentChanged(f32 value)
    {
        Value = value;
        _Parent._Fraction =
            _Parent.Value <= 0
                ? 1f
                : Math.Clamp01(Value / Math.Clamp(_Parent.Value, 1, f32.MaxValue));
    }

    protected override IDisposable SubscribeCore(Observer<f32> observer)
    {
        return _Value.Subscribe(observer.Wrap());
    }
}
