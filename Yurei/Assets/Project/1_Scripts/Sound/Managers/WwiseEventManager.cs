using UnityEngine;

public class WwiseEventManager : IAudioEventService
{
      public void PostEvent(string eventName, GameObject emitter = null)
      {
            if (emitter != null)
            {
                  AkUnitySoundEngine.PostEvent(eventName, emitter);
            }
            else
            {
                  AkUnitySoundEngine.PostEvent(eventName, null);
            }
      }

      public void PostEvent(uint eventId, GameObject emitter = null)
      {
            if (emitter != null)
            {
                  AkUnitySoundEngine.PostEvent(eventId, emitter);
            }
            else
            {
                  AkUnitySoundEngine.PostEvent(eventId, null);
            }
      }
      public void PostEvent(AK.Wwise.Event wwiseEvent, GameObject emitter = null)
      {
      if (wwiseEvent == null)
      {
            Debug.LogWarning("Tried to post a null Wwise event");
            return;
      }

      if (emitter != null)
            wwiseEvent.Post(emitter);
      else
            wwiseEvent.Post(null);
      }

      public void StopAllEventOnEmitter(string eventName, GameObject emitter)
      {
            if (emitter != null)
            {
                  AkUnitySoundEngine.StopAll(emitter);
            }
      }
}