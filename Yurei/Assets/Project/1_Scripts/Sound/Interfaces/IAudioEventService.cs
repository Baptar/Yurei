using UnityEngine;

public interface IAudioEventService
{
    void PostEvent(string eventName, GameObject emitter = null);
    void PostEvent(uint eventId, GameObject emitter = null);
    void PostEvent(AK.Wwise.Event wwiseEvent, GameObject emitter = null);

    void StopAllEventOnEmitter(string eventName, GameObject emitter = null);
}