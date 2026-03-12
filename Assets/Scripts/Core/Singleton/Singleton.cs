using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance {get; private set;}

    [Header("Singleton Settings")]
    [Tooltip("If true, this singleton will persist across scene loads. " +
             "If false, a new instance will be created for each scene and the old one destroyed.")]
    [SerializeField] private bool _persistAcrossScenes = false;


    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"[Singleton] Multiple instances of singleton {typeof(T).Name} detected! Destroying duplicate on {gameObject.name}.");
            Destroy(gameObject);
            return;
        }

        Instance = this as T;

        if (_persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) // Clean up the static reference if this instance is being destroyed
        {
            Instance = null;
        }
    }
}
