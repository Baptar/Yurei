using UnityEngine;
using AK.Wwise;

public class WwiseListenerManager : MonoBehaviour
{
    public static WwiseListenerManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Si true, ajoute automatiquement AkAudioListener et AkGameObj aux caméras qui n'en ont pas")]
    [SerializeField] private bool autoAddComponents = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Switch l'AkAudioListener actif vers la caméra cible
    /// </summary>
    /// <param name="targetCamera">La caméra qui doit avoir le listener actif</param>
    public void SwitchListenerTo(Camera targetCamera)
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("WwiseListenerManager: targetCamera is null!");
            return;
        }

        // Désactiver tous les listeners
        AkAudioListener[] allListeners = FindObjectsOfType<AkAudioListener>();
        foreach (AkAudioListener listener in allListeners)
        {
            listener.enabled = false;
        }

        // Récupérer ou créer le listener sur la caméra cible
        AkAudioListener targetListener = targetCamera.GetComponent<AkAudioListener>();
        
        if (targetListener == null && autoAddComponents)
        {
            // Ajouter AkGameObj si nécessaire (requis pour AkAudioListener)
            if (targetCamera.GetComponent<AkGameObj>() == null)
            {
                targetCamera.gameObject.AddComponent<AkGameObj>();
                Debug.Log($"🎧 AkGameObj ajouté à {targetCamera.name}");
            }

            // Ajouter AkAudioListener
            targetListener = targetCamera.gameObject.AddComponent<AkAudioListener>();
            Debug.Log($"AkAudioListener ajouté à {targetCamera.name}");
        }

        // Activer le listener cible
        if (targetListener != null)
        {
            targetListener.enabled = true;
            Debug.Log($"AkAudioListener activé sur: {targetCamera.name}");
        }
        else
        {
            Debug.LogWarning($"WwiseListenerManager: Pas de AkAudioListener trouvé sur {targetCamera.name} et autoAddComponents est désactivé!");
        }
    }

    /// <summary>
    /// Retourne le listener actuellement actif
    /// </summary>
    public AkAudioListener GetActiveListener()
    {
        AkAudioListener[] allListeners = FindObjectsOfType<AkAudioListener>();
        foreach (AkAudioListener listener in allListeners)
        {
            if (listener.enabled)
                return listener;
        }
        return null;
    }

    /// <summary>
    /// Setup initial : désactive tous les listeners sauf celui spécifié
    /// </summary>
    /// <param name="initialCamera">La caméra de départ</param>
    public void InitializeListener(Camera initialCamera)
    {
        SwitchListenerTo(initialCamera);
    }
}