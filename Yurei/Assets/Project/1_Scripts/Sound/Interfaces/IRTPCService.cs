using UnityEngine;

public interface IRTPCService
{
    void SetRTPCValue(string rtpcName, float value, GameObject emitter = null);
    void SetGlobalRTPCValue(string rtpcName, float value);
}