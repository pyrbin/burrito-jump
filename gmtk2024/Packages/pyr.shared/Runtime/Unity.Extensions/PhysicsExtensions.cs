using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace pyr.Shared.Extensions;

public static class Collider2DExtensions
{
    public static float3 GetRandomPoint(this BoxCollider2D boxCollider)
    {
        var extents = boxCollider.size / 2f;
        var point = new float2(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y)) +
                    (float2)boxCollider.offset;
        return boxCollider.transform.TransformPoint(new Vector3(point.x, point.y, 0));
    }
}
