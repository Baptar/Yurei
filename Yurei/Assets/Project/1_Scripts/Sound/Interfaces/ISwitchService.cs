using UnityEngine;

public interface ISwitchService
{
    void SetSwitch(string switchGroup, string switchState, GameObject emitter);
}