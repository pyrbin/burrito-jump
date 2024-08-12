using System.Reflection;
using R3;

namespace gmtk2024.Runtime.Stat;

// #FB_TODO: Implement a DerivedStat<T1, T2...> that allows for a stat to be derived from multiple stats.

[Serializable]
public abstract class Stat : Observable<f32>, IDisposable
{
    [SerializeField]
    internal SerializableReactiveProperty<f32> _BaseValue = new(new f32());

    internal readonly List<Modifier> _Modifiers;

    [SerializeField]
    internal readonly ReactiveProperty<f32> _Value;

#if UNITY_EDITOR
    public readonly string IconIdentifier;
#endif

    internal Func<f32, f32> _Clamp = static value => value;

    internal Func<f32, f32> _Round = static value => value;

    protected DisposableBag _Subscriptions;

    protected Stat()
    {
        ObservableSystem.DefaultTimeProvider = UnityTimeProvider.Update;
        ObservableSystem.DefaultFrameProvider = UnityFrameProvider.Update;

        var type = GetType();
#if UNITY_EDITOR
        IconIdentifier = type.GetCustomAttribute<IconIdentifierAttribute>()?.Icon ?? "📈";
#endif
        _Clamp = InitializeClampFunction(type);
        _Round = InitializeRoundFunction(type);
        _Modifiers ??= new List<Modifier>();
        _Value ??= new ReactiveProperty<f32>(CalculateFinalValue());
        _BaseValue ??= new SerializableReactiveProperty<f32>();
        _Subscriptions.Add(
            _BaseValue
                .CombineLatest(
                    Observable.Merge(
                        _BaseValue.AsObservable(),
                        Observable.EveryValueChanged(
                            _Modifiers.ToObservable(),
                            _ => CalculateFinalValue()
                        )
                    ),
                    (_, _) => CalculateFinalValue()
                )
                .Subscribe(v =>
                {
                    _Value.Value = v;
                    _Value.ForceNotify();
                    OnValueChanged(v);
                })
        );
    }

    [ShowInInspector]
    public f32 Value => _Value.Value;

    [ShowInInspector]
    public f32 BaseValue => _BaseValue.Value;

    [ShowInInspector]
    public List<Modifier> Modifiers => _Modifiers;

    public void Dispose()
    {
        _Value.Dispose();
        _BaseValue.Dispose();
        _Subscriptions.Dispose();
    }

    ~Stat()
    {
        Dispose();
    }

    protected virtual void OnValueChanged(f32 value) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private f32 CalculateFinalValue()
    {
        const f32 BASE_ADDITIVE = 1.0f;
        const f32 BASE_MULTIPLICATIVE = 1.0f;

        f32 flat = 0;
        var additive = BASE_ADDITIVE;
        var multiplicative = BASE_MULTIPLICATIVE;

        foreach (var mod in _Modifiers)
            switch (mod.Type)
            {
                case ModifierType.Flat:
                    flat += mod.Value;
                    break;
                case ModifierType.Additive:
                    additive += mod.Value;
                    break;
                case ModifierType.Multiply:
                    multiplicative += mod.Value;
                    break;
            }

#if UNITY_EDITOR
        _BaseValue.Value = _Round(_Clamp(_BaseValue.Value));
#endif
        return _Round(_Clamp((_BaseValue.Value + flat) * additive * multiplicative));
    }

    protected override IDisposable SubscribeCore(Observer<f32> observer)
    {
        return _Value.Subscribe(observer.Wrap());
    }

    protected static Func<f32, f32> InitializeClampFunction(Type type)
    {
        var clampMinAttr = type.GetCustomAttribute<ClampMinAttribute>();
        var clampMaxAttr = type.GetCustomAttribute<ClampMaxAttribute>();
        var clampMinMaxAttr = type.GetCustomAttribute<ClampMinMaxAttribute>();
        return (clampMinAttr, clampMaxAttr, clampMinMaxAttr) switch
        {
            (not null, _, _) => value => Math.Max(value, clampMinAttr.MinValue),
            (_, not null, _) => value => Math.Min(value, clampMaxAttr.MaxValue),
            (_, _, not null)
                => value => Math.Clamp(value, clampMinMaxAttr.MinValue, clampMinMaxAttr.MaxValue),
            _ => static value => Math.Max(0, value) // Default clamp above 0
        };
    }

    protected static Func<f32, f32> InitializeRoundFunction(Type type)
    {
        var roundTo = type.GetCustomAttribute<RoundToAttribute>();

        if (roundTo is null)
            return static value => value;

        return value =>
            roundTo.Mode switch
            {
                RoundingMode.Floor => Math.FloorToInt(value),
                RoundingMode.Ceil => Math.CeilToInt(value),
                RoundingMode.Round or _ => Math.RoundToInt(value)
            };
    }
}
