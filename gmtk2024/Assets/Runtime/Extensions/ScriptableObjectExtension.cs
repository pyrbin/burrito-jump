public static class ScriptableObjectExtension
{
    /// <summary>
    /// Creates and returns a clone of any given scriptable object.
    /// </summary>
    public static T CloneScriptableObject<T>(this T scriptableObject)
        where T : ScriptableObject
    {
        if (scriptableObject == null)
        {
            Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
            return (T)ScriptableObject.CreateInstance(typeof(T));
        }

        T instance = UnityEngine.Object.Instantiate(scriptableObject);
        instance.name = scriptableObject.name; // remove (Clone) from name
        return instance;
    }
}
