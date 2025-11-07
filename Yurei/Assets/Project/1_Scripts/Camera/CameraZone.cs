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
    
    [Header("Activate / Deactivate elements")]
    [Tooltip("these gameObjects are activated / deactivated when player enters / leave the zone")]
    [SerializeField] private bool deactivateCameraOnStart = false;
    [SerializeField] private GameObject[] objectToActivate;
    
    [Space(10)]
    [Header("Follow Player")]
    [SerializeField] private bool followPlayer;
    [Tooltip("the local position of the camera if follow player in this zone")]
    [SerializeField] private Vector3 zonePosition = Vector3.zero;
    [SerializeField] private Ease easeFunction = Ease.InOutExpo;
    [SerializeField] private float easeDuration = 0.5f;
    
    [Space(10)]
    [Header("Camera Listener")]
    public Ease easeCameraListener = Ease.InOutExpo;
    public float timeCameraListener = 1.0f;
    
    /*[Space(10)]
    [Header("render texture options")]
    [SerializeField] private RawImage renderTexture;
    [Tooltip("The Color of the RenderTexture when player not in the zone")]
    [SerializeField] private Color colorDeactivated;*/
    
    private void Start()
    {
        // if we want to deactivate camera associated at start 
        if (zoneCamera != null)
            zoneCamera.enabled = !deactivateCameraOnStart;
        
        // set this zone as "inactive" : player is not here
        // deactivate all object associated
        foreach (GameObject obj in objectToActivate)
            obj.SetActive(false);
        
        // set render texture associated to this zone as "inactive"
        //ManageRenderTexturesLight(false);
    }

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
            //LightZoneManager.Instance.OnEnterNewZone(this);
            ManagerObjectToActivate(true);
            //ManageRenderTexturesLight(true);
        }
    }

    private void ManagerObjectToActivate(bool activate)
    {
        foreach (GameObject obj in objectToActivate)
            obj.SetActive(activate);
    }
    
    /*public void ManageRenderTexturesLight(bool activate)
    {
        if (renderTexture == null) return;

        renderTexture.DOColor(activate? Color.white : colorDeactivated, 0.2f);
    }*/

    public Camera GetCameraAssociated() => zoneCamera;
}