namespace pyr.Core.Singleton;

public interface ISingleton
{
    public void InitializeSingleton();
    public void ClearSingleton();
}

public enum SingletonInitializationStatus
{
    None,
    Initializing,
    Initialized
}

public abstract class Singleton<T> : ISingleton
    where T : Singleton<T>, new()
{
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
            lock (typeof(T))
            {
                if (s_Instance != null) return s_Instance;
                s_Instance = new T();
                s_Instance.InitializeSingleton();
            }

            return s_Instance;
        }
    }

    public virtual bool IsInitialized =>
        _InitializationStatus == SingletonInitializationStatus.Initialized;

    #endregion

    #region Protected Methods

    protected virtual void OnInitializing()
    {
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

    public static void DestroyInstance()
    {
        if (s_Instance == null) return;

        s_Instance.ClearSingleton();
        s_Instance = default;
    }

    #endregion
}
