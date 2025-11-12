using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AkAudioListener))]
public class ListenerManager : MonoBehaviour
{
    public static ListenerManager Instance;

    [Header("Listener Settings")]
    [Tooltip("Si activé, le listener reste sur le joueur au lieu de suivre les zones caméra.")]
    public bool stayOnPlayer = false;
    private Transform playerTransform;

    private void Awake()
    {
        Instance = this;

        // Trouve le joueur via son tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void Update()
    {
        if (stayOnPlayer && playerTransform != null)
        {
            transform.position = playerTransform.position;
            transform.rotation = playerTransform.rotation;
        }
    }
    public void MoveListenerTo(CameraZone zoneCamera)
    {
        if (stayOnPlayer)
            return;
            
        Sequence sequence = DOTween.Sequence();

        sequence.Append(gameObject.transform.DOMove(zoneCamera.GetCameraAssociated().transform.position, zoneCamera.timeCameraListener)).SetEase(zoneCamera.easeCameraListener)
            .Join(gameObject.transform.DORotateQuaternion(zoneCamera.GetCameraAssociated().transform.rotation, zoneCamera.timeCameraListener)).SetEase(zoneCamera.easeCameraListener);
    }
}
