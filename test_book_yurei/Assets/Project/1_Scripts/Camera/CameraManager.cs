using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    [SerializeField] private GameObject mainCamera;
    public Camera CurrentCamera { get; private set; }
    public Camera PreviousCamera { get; private set; }
    
    [SerializeField] private DecalProjector projector;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SwitchTo(Camera newCam, bool followPlayer = false, Vector3 zonePosition = default(Vector3), Ease easeFunction = Ease.InOutExpo, float easeDuration = 0.5f)
    {
        if (newCam == CurrentCamera) return;

        // set the new and previous camera
        // we store previous camera in order to NOT deactivate element associated immediately when we leave the zone but when we enter a new zone 
        PreviousCamera = CurrentCamera;
        CurrentCamera = newCam;

        if (mainCamera == null) return;
        
        if (followPlayer)
        {
            mainCamera.transform.DOLocalMove(zonePosition, easeDuration).SetEase(easeFunction);            
        }
    }

    [ContextMenu("modif scale dacal")]
    public void modif_scale_dacale()
    {
        // Animation de la taille avec DOTween
        DOTween.To(
            () => projector.size,
            x => projector.size = x,
            new Vector3(projector.size.x / 2, projector.size.y / 2, projector.size.z),
            1.5f
        ).SetEase(Ease.OutBack);
    }
}
