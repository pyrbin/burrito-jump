using ObservableCollections;

namespace gmtk2024.Runtime.Stat;

[Serializable]
public sealed class Stats : IDisposable
{
    [SerializeField]
    private SerializedReferenceDictionary<StatType, Stat> _Stats = new();

    // #FB_PERF: not that nice to duplicate the list, but it's not a big deal (?)
    private ObservableList<Stat> _ObservableStats = new();

    public IReadOnlyDictionary<StatType, Stat> All => _Stats;

    public IObservableCollection<Stat> Values => _ObservableStats;

    public i32 Count => Values.Count;

    public void Dispose()
    {
        ///
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<T> Get<T>()
        where T : Stat
    {
        if (_Stats.TryGetValue(typeof(T).ToStatType(), out var value) && value is not null)
            return (T)value;
        return None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Option<Stat> Get(StatType type)
    {
        return _Stats.TryGetValue(type, out var value) ? value : None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T>(out T stat)
        where T : Stat
    {
        if (_Stats.TryGetValue(typeof(T).ToStatType(), out var value) && value is not null)
        {
            stat = (T)value;
            return true;
        }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        stat = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T>(StatType type, out T stat)
        where T : Stat
    {
        if (_Stats.TryGetValue(type, out var value) && value is not null)
        {
            stat = (T)value;
            return true;
        }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        stat = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stats Add<T>(T stat)
        where T : Stat
    {
        return Add(typeof(T).ToStatType(), stat);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stats Add<T>()
        where T : Stat
    {
        return Add(typeof(T).ToStatType(), Activator.CreateInstance<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stats Add(StatType type, Stat stat)
    {
        _Stats.Add(type, stat);
        _ObservableStats.Add(stat);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stats Remove<T>()
        where T : Stat
    {
        return Remove(typeof(T).ToStatType());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stats Remove(StatType type)
    {
        if (_Stats.TryGetValue(type, out var value))
        {
            _ObservableStats.Remove(value);
            _Stats.Remove(type);
        }

        return this;
    }

    public void Clear()
    {
        _Stats.Clear();
        _ObservableStats.Clear();
    }

    public void CloneFrom(Stats other)
    {
        foreach (var kvp in other._Stats)
        {
            var clone = (Stat)Activator.CreateInstance(kvp.Value.GetType());
            clone._BaseValue.Value = kvp.Value._BaseValue.Value;
            Add(kvp.Key, clone);
        }
    }
}
