namespace gmtk2024.Runtime.Stat;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StatAttribute : Attribute { }

public enum RoundingMode
{
    Round,
    Floor,
    Ceil
}

[AttributeUsage(AttributeTargets.Class)]
public class RoundToAttribute : Attribute
{
    public RoundToAttribute(RoundingMode mode)
    {
        Mode = mode;
    }

    public RoundingMode Mode { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class IconIdentifierAttribute : Attribute
{
    public IconIdentifierAttribute(string icon)
    {
        Icon = icon;
    }

    public string Icon { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ClampMinAttribute : Attribute
{
    public ClampMinAttribute(f32 minValue)
    {
        MinValue = minValue;
    }

    public f32 MinValue { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ClampMaxAttribute : Attribute
{
    public ClampMaxAttribute(f32 maxValue)
    {
        MaxValue = maxValue;
    }

    public f32 MaxValue { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class ClampMinMaxAttribute : Attribute
{
    public ClampMinMaxAttribute(f32 minValue, f32 maxValue)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public f32 MinValue { get; }
    public f32 MaxValue { get; }
}
