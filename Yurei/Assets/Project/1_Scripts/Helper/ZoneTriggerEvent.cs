using UnityEngine;
using UnityEngine.Events;

public class ZoneTriggerEvent : MonoBehaviour
{
    [Header("EVENT")]
    public UnityEvent onEnter;
    
    [Header("Optional Filter")]
    public string targetTag = ""; // Laisse vide pour tout déclencher
    
    private void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            onEnter.Invoke();
        }
    }
}
