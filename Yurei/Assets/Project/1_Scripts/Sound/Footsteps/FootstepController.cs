using UnityEngine;

[DisallowMultipleComponent]
public class FootstepController : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform rayAnchor; // Endroit d'où est tiré le raycast - à placer entre les deux pieds
    [SerializeField] private float maxGroundDist = 1f;
    [SerializeField] private LayerMask groundLayer = -1; // -1 = tous les layers

    [Header("Audio Settings")]
    [SerializeField] private AK.Wwise.Event playerWalkEvent;
    [SerializeField] private GameObject footstepSource; // Source audio (pied player)

    [Header("Material Detection")]
    [SerializeField] private FootstepMaterialDatabase materialDatabase;

    [Header("Debug")]
    [SerializeField] private bool useSurfaceProvider = true;
    [SerializeField] private bool useMaterialDetection = true;
    [SerializeField] private bool showDebugLogs = false;

    [Header("Fallback")] // Si rien renseigné : paramètres par défaut
    [SerializeField] private AK.Wwise.Switch defaultMaterial;
    [SerializeField] private AK.Wwise.Switch defaultCondition;

    [Header("Gizmos")]
    [SerializeField] private float wireSphereRadius = 0.1f;

    // Cache
    private FootstepSurfaceProvider currentProvider;
    private FootstepSurfaceProvider lastProvider;
    private FootstepMaterialDatabase.MaterialMapping currentMaterialMapping;
    private FootstepMaterialDatabase.MaterialMapping lastMaterialMapping;

    private void Start()
    {
        // Si pas défini, son des pas sera émis du gameObject sur lequel est ce script
        if (footstepSource == null)
        {
            footstepSource = gameObject;
        }
    }

    void FixedUpdate()
    {
        // Reset les détections
        currentProvider = null;
        currentMaterialMapping = null;

        if (Physics.Raycast(rayAnchor.position, Vector3.down, out RaycastHit hit,
            maxGroundDist, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // PRIORITÉ 1 : FootstepSurfaceProvider (override manuel)
            if (useSurfaceProvider && hit.transform.TryGetComponent<FootstepSurfaceProvider>(out var provider))
            {
                currentProvider = provider;

                // Debug : si on change de surface
                if (currentProvider != lastProvider)
                {
                    lastProvider = currentProvider;
                    lastMaterialMapping = null; // Reset l'autre cache

                    if (showDebugLogs)
                    {
                        string mat = currentProvider.SurfaceMaterial?.Name ?? "Default";
                        string cond = currentProvider.SurfaceCondition?.Name ?? "Default";
                        Debug.Log($"[Provider] Surface → {mat}, {cond}", this);
                    }
                }
            }
            // PRIORITÉ 2 : Détection automatique par Material
            else if (useMaterialDetection && materialDatabase != null)
            {
                currentMaterialMapping = DetectMaterialAtHit(hit);

                // Si on passe de Provider à Material, reset le cache Provider
                if (lastProvider != null)
                {
                    lastProvider = null;
                    if (showDebugLogs)
                    {
                        Debug.Log($"[Switch] Provider → Material detection", this);
                    }
                }

                // Debug : si on change de material
                if (currentMaterialMapping != lastMaterialMapping)
                {
                    lastMaterialMapping = currentMaterialMapping;

                    if (showDebugLogs && currentMaterialMapping != null)
                    {
                        string mat = currentMaterialMapping.surfaceMaterial?.Name ?? "Unknown";
                        string cond = currentMaterialMapping.surfaceCondition?.Name ?? "Unknown";
                        Debug.Log($"[Material] Surface → {mat}, {cond} (from {hit.collider.name})", this);
                    }
                }
            }
            // Aucune détection : reset tout
            else
            {
                if (lastProvider != null || lastMaterialMapping != null)
                {
                    lastProvider = null;
                    lastMaterialMapping = null;
                    if (showDebugLogs)
                    {
                        Debug.Log($"[Switch] No surface detected, using fallback", this);
                    }
                }
            }
        }
        // Raycast ne touche rien : reset tout
        else
        {
            if (lastProvider != null || lastMaterialMapping != null)
            {
                lastProvider = null;
                lastMaterialMapping = null;
                // if (showDebugLogs)
                // {
                //     Debug.Log($"[Switch] Lost ground contact", this);
                // }
            }
        }
    }

    /// <summary>
    /// Fonction appelée à chaque pas (Animation Event)
    /// </summary>
    public void TriggerFootstep()
    {
        // PRIORITÉ 1 : FootstepSurfaceProvider
        if (useSurfaceProvider && currentProvider != null)
        {
            currentProvider.SurfaceMaterial?.SetValue(footstepSource);
            currentProvider.SurfaceCondition?.SetValue(footstepSource);
            
            if (showDebugLogs)
            {
                Debug.Log($"Footstep - Used FootstepProvider ({currentProvider.SurfaceMaterial?.Name})");
            }
        }
        // PRIORITÉ 2 : Material Detection
        else if (useMaterialDetection && currentMaterialMapping != null)
        {
            currentMaterialMapping.surfaceMaterial?.SetValue(footstepSource);
            currentMaterialMapping.surfaceCondition?.SetValue(footstepSource);
            
            if (showDebugLogs)
            {
                Debug.Log($"Footstep - Used MaterialMapping ({currentMaterialMapping.surfaceMaterial?.Name})");
            }
        }
        // PRIORITÉ 3 : Fallback
        else
        {
            defaultMaterial?.SetValue(footstepSource);
            defaultCondition?.SetValue(footstepSource);
            
            if (showDebugLogs)
            {
                Debug.Log("Footstep - Used Default Material (Fallback)");
            }
        }

        // Play le son
        AudioServices.Events.PostEvent(playerWalkEvent, footstepSource);
    }

    /// <summary>
    /// Détecte le material Unity au point de contact
    /// </summary>
    private FootstepMaterialDatabase.MaterialMapping DetectMaterialAtHit(RaycastHit hit)
    {
        Material hitMaterial = null;

        // Si MeshCollider - Détection précise par submesh (pour meshes multi-matériaux)
        if (hit.collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterials.Length > 0)
            {
                // Un seul material
                if (renderer.sharedMaterials.Length == 1)
                {
                    hitMaterial = renderer.sharedMaterial;
                }
                // Plusieurs materials : trouve le submesh exact
                else
                {
                    int submeshIndex = GetSubmeshIndex(meshCollider.sharedMesh, hit.triangleIndex);

                    if (submeshIndex >= 0 && submeshIndex < renderer.sharedMaterials.Length)
                    {
                        hitMaterial = renderer.sharedMaterials[submeshIndex];
                    }
                    else
                    {
                        hitMaterial = renderer.sharedMaterial;
                    }
                }
            }
        }
        // Si BoxCollider, SphereCollider, CapsuleCollider, etc.
        else
        {
            // Cherche d'abord sur le GameObject du collider
            Renderer renderer = hit.collider.GetComponent<Renderer>();

            // Si pas trouvé, cherche sur les enfants (cas où collider est sur parent)
            if (renderer == null)
            {
                renderer = hit.collider.GetComponentInChildren<Renderer>();
            }

            // Si toujours pas trouvé, cherche sur le parent (cas où renderer est sur parent)
            if (renderer == null)
            {
                renderer = hit.collider.GetComponentInParent<Renderer>();
            }

            // Utilise le premier material trouvé
            if (renderer != null && renderer.sharedMaterials.Length > 0)
            {
                hitMaterial = renderer.sharedMaterial;
            }
        }

        // Cherche le mapping dans la database
        if (hitMaterial != null && materialDatabase != null)
        {
            return materialDatabase.GetMapping(hitMaterial);
        }

        return null;
    }

    /// <summary>
    /// Trouve l'index du submesh qui contient un triangle donné
    /// </summary>
    private int GetSubmeshIndex(Mesh mesh, int triangleIndex)
    {
        if (mesh == null) return 0;

        int triangleCount = 0;

        for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
        {
            int[] triangles = mesh.GetTriangles(submesh);
            int submeshTriangleCount = triangles.Length / 3;

            if (triangleIndex < triangleCount + submeshTriangleCount)
            {
                return submesh;
            }

            triangleCount += submeshTriangleCount;
        }

        return 0; // Fallback
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (rayAnchor == null) return;

        // Couleur selon détection
        // Jaune = fallback, Vert = FootstepSurfaceProvider, Cyan = MaterialMapping
        Color rayColor = Color.yellow;
        if (currentProvider != null) rayColor = Color.green;
        else if (currentMaterialMapping != null) rayColor = Color.cyan;

        Gizmos.color = rayColor;
        Gizmos.DrawRay(rayAnchor.position, Vector3.down * maxGroundDist);

        if (Physics.Raycast(rayAnchor.position, Vector3.down, out RaycastHit hit,
            maxGroundDist, groundLayer, QueryTriggerInteraction.Ignore))
        {
            Gizmos.DrawWireSphere(hit.point, wireSphereRadius);

            // Label
            string label = "No detection (Fallback)";
            if (currentProvider != null)
            {
                label = $"[Provider]\n{currentProvider.SurfaceMaterial?.Name ?? "?"}";
            }
            else if (currentMaterialMapping != null)
            {
                label = $"[Material]\n{currentMaterialMapping.surfaceMaterial?.Name ?? "?"}";
            }

            UnityEditor.Handles.Label(hit.point + Vector3.up * 0.3f, label);
        }
    }

    private void OnValidate()
    {
        // Auto-setup du rayAnchor si pas défini
        if (rayAnchor == null)
        {
            rayAnchor = transform;
        }
    }
#endif
}