using UnityEngine;

public class WwiseRTPCManager : IRTPCService
{
      public void SetRTPCValue(string rtpcName, float value, GameObject emitter = null)
      {
            if (string.IsNullOrEmpty(rtpcName)) return;

            if (emitter != null)
            {
                  AkUnitySoundEngine.SetRTPCValue(rtpcName, value, emitter);
            }
            else
            {
                  AkUnitySoundEngine.SetRTPCValue(rtpcName, value);
            }
      }
      
      public void SetGlobalRTPCValue(string rtpcName, float value)
      {
            if (string.IsNullOrEmpty(rtpcName)) return;
            AkUnitySoundEngine.SetRTPCValue(rtpcName, value);
      }

}