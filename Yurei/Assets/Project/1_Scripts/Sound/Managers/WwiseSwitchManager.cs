using UnityEngine;

public class WwiseSwitchManager : ISwitchService
{
    public void SetSwitch(string switchGroup, string switchState, GameObject emitter)
    {
        if (string.IsNullOrEmpty(switchGroup) || string.IsNullOrEmpty(switchState)) return;
        if (emitter == null)
        {
            Debug.LogWarning("Cannot set switch on null emitter");
            return;
        }
        AkUnitySoundEngine.SetSwitch(switchGroup, switchState, emitter);
    }
}
