namespace gmtk2024.Runtime.Stat;

public enum ModifierType
{
    Flat,
    Additive,
    Multiply
}

public sealed class Modifier
{
    public ModifierType Type { get; set; } = ModifierType.Flat;
    public f32 Value { get; set; }

    public static Modifier Create(ModifierType type, f32 value)
    {
        return new Modifier { Type = type, Value = value };
    }

    public static Modifier Flat(f32 value)
    {
        return new Modifier { Type = ModifierType.Flat, Value = value };
    }

    public static Modifier Additive(f32 value)
    {
        return new Modifier { Type = ModifierType.Additive, Value = value };
    }

    public static Modifier Multiply(f32 value)
    {
        return new Modifier { Type = ModifierType.Multiply, Value = value };
    }
}
