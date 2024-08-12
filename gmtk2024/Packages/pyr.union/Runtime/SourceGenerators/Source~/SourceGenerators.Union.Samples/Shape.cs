using pyr.Union;

namespace SourceGenerators.Union.Samples;

[Union]
public partial record struct Shape
{
    public static partial Shape Sphere(float radius);

    public static partial Shape Box(int halfExtends);

    public static partial Shape Capsule(float radius, float height);

    public static partial Shape Cylinder(float? radius, float height);
}
