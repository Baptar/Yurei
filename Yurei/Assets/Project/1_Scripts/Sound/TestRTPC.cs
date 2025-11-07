using UnityEngine;

public class TestRTPC : MonoBehaviour
{
    void Update()
    {
        float val = Mathf.PingPong(Time.time * 100f, 200f) - 100f;
        AkSoundEngine.SetRTPCValue("Book_Panning", val, gameObject);
        Debug.Log($"RTPC sent: {val}");
    }
}
