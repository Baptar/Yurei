using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraZone : MonoBehaviour
{
    // The camera linked to this area
    public Camera zoneCamera;

    [Header("Parameters")] 
    [SerializeField] private int pageNumberAssociated;
    
    [Space(10)]
    [Header("Follow Player")]
    [SerializeField] private bool followPlayer;
    [Tooltip("the local position of the camera if follow player in this zone")]
    [SerializeField] private Vector3 zonePosition = Vector3.zero;
    [SerializeField] private Ease easeFunction = Ease.InOutExpo;
    [SerializeField] private float easeDuration = 0.5f;
    
    [Space(10)]
    [Header("Audio Listener")]
    public Ease easeAudioListener = Ease.InOutExpo;
    public float timeAudioListener = 1.0f;
    

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        // case new case in a different page
        if (pageNumberAssociated != BookManager.Instance.GetCurrentRightPageNumber())
        {
            BookManager.Instance.OnPlayerFinishedPage(pageNumberAssociated < BookManager.Instance.GetCurrentRightPageNumber(),
                () =>
                {
                    ListenerManager.Instance.MoveListenerTo(this);
                    CameraManager.Instance.SwitchTo(zoneCamera, true, zonePosition, easeFunction, easeDuration);
                });
        }
        // case new case in the same page
        else
        {
            CameraManager.Instance.SwitchTo(zoneCamera, followPlayer, zonePosition, easeFunction, easeDuration);
            ListenerManager.Instance.MoveListenerTo(this);
        }
    }

    public Camera GetCameraAssociated() => zoneCamera;
}