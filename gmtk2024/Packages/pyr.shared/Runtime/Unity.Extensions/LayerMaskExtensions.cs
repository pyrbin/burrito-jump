using UnityEngine;

namespace pyr.Shared.Extensions;

public static class LayerMaskExtensions
{
    public static LayerMask Add(this LayerMask mask, int layer)
    {
        mask |= 1 << layer;
        return mask;
    }

    public static LayerMask Add(this LayerMask mask, string layerName)
    {
        mask |= 1 << LayerMask.NameToLayer(layerName);
        return mask;
    }

    public static LayerMask Merge(this LayerMask mask, LayerMask otherMask)
    {
        return mask | otherMask;
    }

    public static bool Has(this LayerMask mask, int layer)
    {
        return (mask & (1 << layer)) > 0;
    }

    public static bool Has(this LayerMask mask, string layerName)
    {
        return (mask & (1 << LayerMask.NameToLayer(layerName))) > 0;
    }

    public static int MaskValue(this LayerMask me)
    {
        return (int)Mathf.Log(me.value, 2);
    }
}
