using pyr.Union;
using Unity.Mathematics;
using UnityEngine;

namespace pyr.Shared.Types;

[Union]
public partial record struct Shape
{
    public static partial Shape Sphere(float radius);

    public static partial Shape Box(float3 halfExtends);

    public static partial Shape Capsule(float radius, float height);

    public float Area => Match
    (
        Sphere: radius => 4f * math.PI * radius * radius,
        Box: halfExtents =>
            8f * (halfExtents.x * halfExtents.y + halfExtents.x * halfExtents.z + halfExtents.y * halfExtents.z),
        Capsule: (radius, height) => 2f * math.PI * radius * (radius + height)
    );

    public float Volume => Match
    (
        Sphere: radius => 4f / 3f * math.PI * radius * radius * radius,
        Box: halfExtents => 8f * halfExtents.x * halfExtents.y * halfExtents.z,
        Capsule: (radius, height) => math.PI * radius * radius * (4f / 3f * radius + height)
    );

    public Collider Collider => Match<Collider>
    (
        Sphere: radius => new SphereCollider { radius = radius, center = float3.zero },
        Box: halfExtents => new BoxCollider { size = halfExtents * 2f, center = float3.zero },
        Capsule: (radius, height) => new CapsuleCollider
            { radius = radius, height = height, direction = 1, center = float3.zero }
    );

    public Shape Scaled(float scale)
    {
        return Match
        (
            Sphere: radius => Sphere(radius * scale),
            Box: halfExtents => Box(halfExtents * scale),
            Capsule: (radius, height) => Capsule(radius * scale, height * scale)
        );
    }
}
