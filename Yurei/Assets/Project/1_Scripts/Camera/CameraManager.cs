using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public GameObject mainCamera;
    public Camera CurrentCamera { get; private set; }
    public Camera PreviousCamera { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Start(){
        
    WwiseListenerManager.Instance?.InitializeListener(CurrentCamera);
    }

    public void SwitchTo(Camera newCam, bool followPlayer = false, Vector3 zonePosition = default(Vector3), Ease easeFunction = Ease.InOutExpo, float easeDuration = 0.5f)
    {
        SetNewCameraInputRef(newCam);
        WwiseListenerManager.Instance?.SwitchListenerTo(newCam);
        if (followPlayer)
        {
            mainCamera.transform.DOLocalMove(zonePosition, easeDuration).SetEase(easeFunction);            
        }
    }
    
    public void SetNewCameraInputRef(Camera newCam)
    {
        if (newCam == CurrentCamera) return;
        
        // set the new and previous camera
        // we store previous camera in order to NOT deactivate element associated immediately when we leave the zone but when we enter a new zone 
        PreviousCamera = CurrentCamera;
        CurrentCamera = newCam;

        if (mainCamera == null);
    }

    public void MoveCameraTo(Vector3 targetPosition, Quaternion targetRotation, Ease easeFunction = Ease.InOutExpo, float easeDuration = 0.5f, Action onComplete = null)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(mainCamera.transform.DOMove(targetPosition, easeDuration).SetEase(easeFunction))
            .Join(transform.DORotateQuaternion(targetRotation, easeDuration).SetEase(easeFunction))
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        
    }
}
