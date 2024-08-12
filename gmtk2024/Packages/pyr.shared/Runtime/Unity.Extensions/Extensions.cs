using pyr.Union.Monads;
using UnityEngine;

namespace pyr.Shared.Extensions;

public static class Extensions
{
    public static bool IsWithinFrustum(this Mesh mesh, Camera camera)
    {
        if (camera == null) return false;

        var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, mesh.bounds);
    }

    public static bool IsWithinFrustum(this Renderer renderer, Camera camera)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    public static bool IsWithinFrustum(this Vector3 position, Camera camera)
    {
        if (camera == null) return false;

        var viewPos = camera.WorldToViewportPoint(position);
        return viewPos.x.IsBetween(0, 1) && viewPos.y.IsBetween(0, 1) && viewPos.z > 0;
    }

    public static bool HasValidCollider(this RaycastHit hit)
    {
        return hit.IsNotNull() && hit.colliderInstanceID != 0;
    }

    public static bool HasValidCollider(this ColliderHit hit)
    {
        return hit.IsNotNull() && hit.instanceID != 0;
    }

    public static bool HasValidCollider(this Collider collider)
    {
        return collider.IsNotNull() && collider.GetInstanceID() != 0;
    }
}
