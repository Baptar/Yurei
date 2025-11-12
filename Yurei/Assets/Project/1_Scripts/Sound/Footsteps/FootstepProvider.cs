using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class FootstepProvider : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Switch surfaceMaterial;
    public AK.Wwise.Switch SurfaceMaterial => surfaceMaterial;
    
    [SerializeField]
    private AK.Wwise.Switch surfaceCondition;
    public AK.Wwise.Switch SurfaceCondition => surfaceCondition;
}