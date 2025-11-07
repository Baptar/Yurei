using UnityEngine;
using AK.Wwise;

/// <summary>
/// Very small helper: ping-pongs a test RTPC value between -100 and +100 every frame.
/// Attach to an empty GameObject and set `rtpcName` to "Book_Panning" to test whether
/// RTPC values are received in the Wwise profiler/Capture Log.
/// </summary>
[RequireComponent(typeof(AkGameObj))]
public class DebugRTPCSender : MonoBehaviour
{
    [Tooltip("RTPC name to send (case-sensitive)")]
    public string rtpcName = "Book_Panning";

    [Tooltip("When true, send RTPC without a GameObject (global). When false, send with this GameObject.")]
    public bool sendGlobal = true;

    [Range(0.1f, 10f)] public float speed = 1f;

    float lastSent = float.MinValue;

    void Update()
    {
        // PingPong from 0..200 then shift to -100..+100
        float val = Mathf.PingPong(Time.time * speed * 100f, 200f) - 100f;

        // Send every frame for reliable profiler capture; optionally avoid spamming identical values
        if (Mathf.Approximately(val, lastSent)) return;

        if (sendGlobal)
        {
            // Global RTPC
            AkSoundEngine.SetRTPCValue(rtpcName, val);
        }
        else
        {
            // RTPC bound to this game object
            AkSoundEngine.SetRTPCValue(rtpcName, val, gameObject);
        }

        lastSent = val;
        Debug.Log($"[DebugRTPCSender] Sent RTPC '{rtpcName}' = {val:F2} (global={sendGlobal})");
    }
}
