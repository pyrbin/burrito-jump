using System;
using UnityEngine;

namespace pyr.Core.Singleton;

[AttributeUsage(AttributeTargets.Class)]
public class DontDestroyOnLoadAttribute : Attribute
{
}

public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton
    where T : MonoSingleton<T>
{
    #region Unity Messages

    protected virtual void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this as T;

            InitializeSingleton();
        }
        else
        {
            if (Application.isPlaying)
                Destroy(gameObject);
            else
                DestroyImmediate(gameObject);
        }
    }

    #endregion

    #region Fields

    private static T s_Instance;

    private SingletonInitializationStatus _InitializationStatus = SingletonInitializationStatus.None;

    #endregion

    #region Properties

    public static T Instance
    {
        get
        {
            if (s_Instance != null) return s_Instance;
            s_Instance = FindFirstObjectByType<T>();
            if (s_Instance != null) return s_Instance;
            var obj = new GameObject
            {
                name = typeof(T).Name
            };
            s_Instance = obj.AddComponent<T>();
            s_Instance.OnMonoSingletonCreated();
            return s_Instance;
        }
    }

    public virtual bool IsInitialized =>
        _InitializationStatus == SingletonInitializationStatus.Initialized;

    #endregion

    #region Protected Methods

    protected virtual void OnMonoSingletonCreated()
    {
    }

    protected virtual void OnInitializing()
    {
        var isDontDestroyOnLoad = Attribute.IsDefined(
            typeof(T),
            typeof(DontDestroyOnLoadAttribute)
        );
        if (Application.isPlaying && isDontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnInitialized()
    {
    }

    #endregion

    #region Public Methods

    public virtual void InitializeSingleton()
    {
        if (_InitializationStatus != SingletonInitializationStatus.None) return;

        _InitializationStatus = SingletonInitializationStatus.Initializing;
        OnInitializing();
        _InitializationStatus = SingletonInitializationStatus.Initialized;
        OnInitialized();
    }

    public virtual void ClearSingleton()
    {
    }

    public static void CreateInstance()
    {
        DestroyInstance();
        s_Instance = Instance;
    }

    private static void DestroyInstance()
    {
        if (s_Instance == null) return;

        s_Instance.ClearSingleton();
        s_Instance = default;
    }

    #endregion
}
