using UnityEngine;
using UnityEngine.UI;

public class LightZoneManager : MonoBehaviour
{
    /*public static LightZoneManager Instance;
    private CameraZone actualCameraZone;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void OnEnterNewZone(CameraZone newZone)
    {
        if (newZone == null || actualCameraZone == newZone) return;

        if (actualCameraZone == null)
        {
            actualCameraZone = newZone;
            return;
        }
        
        Debug.Log("OnEnterNewZone");
        actualCameraZone.ManagerObjectToActivate(false);
        actualCameraZone.ManageRenderTexturesLight(false);
        actualCameraZone = newZone;
    }*/
}
