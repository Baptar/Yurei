using UnityEngine;
using AK.Wwise;

[RequireComponent(typeof(AkGameObj))]
public class BookPanning : MonoBehaviour
{
    [Header("🎧 Wwise")]
    public AK.Wwise.Event soundEvent;
    public AK.Wwise.RTPC panRTPC;

    [Header("⚙️ Settings")]
    [Range(0f, 100f)] public float panRange = 100f;
    public float updateInterval = 0.05f; // Plus fréquent pour plus de fluidité
    [Tooltip("Changement minimum requis pour update (mettre très bas pour précision)")]
    public float panDeadZone = 0.1f; // Réduit à 0.1 au lieu de 0.5

    private float lastPanValue;
    private float timeSinceLastUpdate;

    void Start()
    {
        if (soundEvent != null)
            soundEvent.Post(gameObject);
        else
            Debug.LogWarning($"{name} : aucun Event Wwise assigné.");

        UpdatePan(true);
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            UpdatePan();
            timeSinceLastUpdate = 0f;
        }
    }

    void UpdatePan(bool force = false)
    {
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        float panValue = Mathf.Clamp((vp.x - 0.5f) * 2f * panRange, -100f, 100f);

        if (force || Mathf.Abs(panValue - lastPanValue) >= panDeadZone)
        {
            if (panRTPC != null)
                panRTPC.SetValue(gameObject, panValue);
            else
                AkSoundEngine.SetRTPCValue("Book_Panning", panValue, gameObject);

            lastPanValue = panValue;
            Debug.Log($"{name} → Pan RTPC: {panValue:F2}");
        }
    }
}