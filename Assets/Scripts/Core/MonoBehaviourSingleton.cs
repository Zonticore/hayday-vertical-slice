using UnityEngine;

public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private const bool verbose = false;

    private static T _instance;
    private static readonly object _lock = new object();

    public static T instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<T>();
                return _instance;
            }
        }
    }

    public static bool hasInstance => _instance != null;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            if (verbose)
            {
                Log.error($"[Singleton] Another instance of {typeof(T)} already exists. Destroying this duplicate.");
            }

            Destroy(gameObject);
            return;
        }
    }
}
