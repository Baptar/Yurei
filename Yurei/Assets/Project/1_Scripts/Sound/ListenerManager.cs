using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AkAudioListener))]
public class ListenerManager : MonoBehaviour
{
    public static ListenerManager Instance;
    private bool isInitialized = false;
    

    private void Awake() => Instance = this;
    
    public void MoveListenerTo(CameraZone zoneCamera)
    {
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(gameObject.transform.DOMove(zoneCamera.GetCameraAssociated().transform.position, zoneCamera.timeAudioListener)).SetEase(zoneCamera.easeAudioListener)
            .Join(gameObject.transform.DORotateQuaternion(zoneCamera.GetCameraAssociated().transform.rotation, zoneCamera.timeAudioListener)).SetEase(zoneCamera.easeAudioListener)
            .OnComplete(() =>
            {
                if (isInitialized) WwiseListenerManager.Instance?.InitializeListener(zoneCamera.GetCameraAssociated());
                else WwiseListenerManager.Instance?.SwitchListenerTo(zoneCamera.GetCameraAssociated());
            });
    }
}
