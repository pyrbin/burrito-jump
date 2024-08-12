using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace pyr.Shared.Extensions;

public static class GameObjectExtensions
{
    public static void SetLayerRecursively(this GameObject root, int layer)
    {
        if (root == null) throw new ArgumentNullException(nameof(root), "Root transform can't be null.");

        foreach (var child in root.transform.EnumerateHierarchy()) child.gameObject.layer = layer;
    }

    public static bool IsInLayerMask(this GameObject gameObject, LayerMask layerMask)
    {
        LayerMask gameObjectMask = 1 << gameObject.layer;
        return (gameObjectMask & layerMask) == gameObjectMask;
    }

    public static void ForEachComponent<T>(this GameObject gameObject, Action<T> action) where T : Component
    {
        foreach (var i in gameObject.GetComponents<T>()) action(i);
    }

    public static void ForEachComponentInChildren<T>(this GameObject gameObject, Action<T> action) where T : Component
    {
        foreach (var i in gameObject.GetComponentsInChildren<T>()) action(i);
    }

    public static bool TryGetComponentSelfOrParent<T>(this Behaviour self, [NotNullWhen(true)] out T component)
        where T : Component
    {
        try
        {
            return self.TryGetComponent(out component)
                   || self.transform.parent.TryGetComponent(out component);
        }
        catch
        {
            component = default!;
            return false;
        }
    }

    public static T? GetComponentSelfOrParent<T>(this Behaviour self) where T : Component
    {
        self.TryGetComponentSelfOrParent<T>(out var component);
        return component;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component;

        if ((component = gameObject.GetComponent<T>()) == null) component = gameObject.AddComponent<T>();

        return component;
    }
}
