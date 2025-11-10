using UnityEngine;

using WwiseEvent = AK.Wwise.Event;

[AddComponentMenu("Project/Audio/Audio Unity Event")]
public class AudioUnityEvent : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private WwiseEvent playEvent;
    [SerializeField]
    private WwiseEvent stopEvent;

    public void Play()
    {
        Play(target);
    }

    public void Play(GameObject target)
    {
        AudioServices.Events.PostEvent(playEvent, target);
    }

    public void Stop()
    {
        Stop(target);
    }

    public void Stop(GameObject target)
    {
        AudioServices.Events.PostEvent(stopEvent, target);
    }
}