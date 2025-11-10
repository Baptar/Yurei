using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AkAudioListener))]
public class ListenerManager : MonoBehaviour
{
    public static ListenerManager Instance;

    private void Awake() => Instance = this;
    
    public void MoveListenerTo(CameraZone zoneCamera)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.Append(gameObject.transform.DOMove(zoneCamera.GetCameraAssociated().transform.position, zoneCamera.timeCameraListener)).SetEase(zoneCamera.easeCameraListener)
            .Join(gameObject.transform.DORotateQuaternion(zoneCamera.GetCameraAssociated().transform.rotation, zoneCamera.timeCameraListener)).SetEase(zoneCamera.easeCameraListener);
    }
}
