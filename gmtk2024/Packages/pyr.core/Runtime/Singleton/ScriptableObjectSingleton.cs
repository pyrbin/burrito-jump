using System;
using System.Reflection;
using UnityEngine;

namespace pyr.Core.Singleton;

[AttributeUsage(AttributeTargets.Class)]
public class AssetNameAttribute : Attribute
{
    public string Name { get; }

    public AssetNameAttribute(string name)
        => Name = name;
}

public abstract class ScriptableObjectSingleton<T> : ScriptableObject, ISingleton
    where T : ScriptableObjectSingleton<T>
{
    private static T s_Instance;

    public static T Instance
    {
        get
        {
            if (s_Instance != null) return s_Instance;
            var name = typeof(T).GetCustomAttribute(typeof(AssetNameAttribute));

            if (name is null)
                throw new Exception($"{typeof(T).Name} is missing the AssetNameAttribute");

            var assetName = ((AssetNameAttribute)name).Name;
            s_Instance = Resources.Load<T>(assetName);

            if (s_Instance != null) return s_Instance;
            s_Instance = CreateInstance<T>();

            s_Instance.InitializeSingleton();
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(s_Instance, $"Assets/Resources/{assetName}.asset");
#endif
            return s_Instance;
        }
    }

    public virtual void InitializeSingleton()
    {
    }

    public virtual void ClearSingleton()
    {
        // Should never be called.
    }
}
