using UnityEngine;
using WwiseEvent = AK.Wwise.Event;
public class AudioEventAsset : MonoBehaviour
{
    [Tooltip("Nom de l'événement Wwise à jouer")]
    public AK.Wwise.Event eventName;

    [Tooltip("Émetteur du son (facultatif)")]
    public GameObject emitter;

    public void Play()
    {
        AudioServices.Events.PostEvent(eventName, emitter);
    }
}
