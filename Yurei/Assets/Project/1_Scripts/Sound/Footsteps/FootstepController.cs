using UnityEngine;

[DisallowMultipleComponent]
public class FootstepController : MonoBehaviour
{
      [Header("Raycast")]
      [SerializeField] private Transform rayAnchor; //Endroit d'où est tiré le raycast - à placer entre les deux pieds
      private float maxGroundDist = 20f;

      [Header("Audio Settings")]
      [SerializeField] private AK.Wwise.Event playerWalkEvent;
      //[SerializeField] private AK.Wwise.Event playerRunEvent;
      [SerializeField] private GameObject footstepSource; // Source audio (pied player)

      [Header("Material Detection")]
      [SerializeField] private FootstepMaterialDatabase materialDatabase;

      [Header("Debug")]
      [SerializeField] private bool useSurfaceProvider = true;
      [SerializeField] private bool useMaterialDetection = true;

      [Header("Fallback")] //Si rien renseigné : paramètres par défaut
      private AK.Wwise.Switch defaultMaterial;
      private AK.Wwise.Switch defaultCondition;

      private FootstepSurfaceProvider currentProvider;
      private FootstepSurfaceProvider lastProvider;
      private FootstepMaterialDatabase.MaterialMapping currentMaterialMapping;

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
            if (Physics.Raycast(rayAnchor.position, Vector3.down, out RaycastHit hit,
                maxGroundDist))
            {
                  // When the player is on a surface with FootstepSurfaceProvider, it updates the current surface.
                  if (hit.transform.TryGetComponent<FootstepSurfaceProvider>(out var provider))
                  {
                        currentProvider = provider;
                        // Debug : si on change de surface, afficher le nouveau matériau
                        if (currentProvider != lastProvider)
                        {
                        lastProvider = currentProvider;

                        string mat = currentProvider.SurfaceMaterial?.Name ?? "Default";
                        string cond = currentProvider.SurfaceCondition?.Name ?? "Default";

                        //Debug.Log($"Ground Surface changed → {mat}, {cond}", this);
                        }
                  }
                  else if (useMaterialDetection && materialDatabase != null)
                  {
                        currentMaterialMapping = DetectMaterialAtHit(hit);

                        if (currentMaterialMapping != null)
                        {
                              string mat = currentMaterialMapping.surfaceMaterial?.Name ?? "Unknown";
                              string cond = currentMaterialMapping.surfaceCondition?.Name ?? "Unknown";
                              //Debug.Log($"[Material] Surface → {mat}, {cond} (from {hit.collider.name})", this);
                        }
                  }
                  
            }
      }

      // Fonction appelée à chaque pas
      public void TriggerFootstep()
      {
                  if (useSurfaceProvider && currentProvider != null)
                  {
                        currentProvider.SurfaceMaterial?.SetValue(footstepSource);
                        currentProvider.SurfaceCondition?.SetValue(footstepSource);
                        Debug.Log("Footstep - Used FootstepProvider("+ currentProvider.SurfaceMaterial?.Name+")");
                  }
                  else if (useMaterialDetection && currentMaterialMapping != null)
                  {
                        currentMaterialMapping.surfaceMaterial?.SetValue(footstepSource);
                        currentMaterialMapping.surfaceCondition?.SetValue(footstepSource);
                        Debug.Log("Footstep - Used MaterialMapping("+ currentMaterialMapping.surfaceMaterial?.Name+")");
                  }
                  else
                  {
                        // Si pas de FootstepProvider ni de material trouvé, utilise les valeurs par défaut
                        defaultMaterial?.SetValue(footstepSource);
                        defaultCondition?.SetValue(footstepSource);
                        Debug.Log("Footstep - Used Default Material (Fallback)");
                  }
                  AudioServices.Events.PostEvent(playerWalkEvent, footstepSource);
      }
      private FootstepMaterialDatabase.MaterialMapping DetectMaterialAtHit(RaycastHit hit)
    {
        Material hitMaterial = null;

        // CAS 1 : MeshCollider - Détection précise par submesh (pour meshes multi-matériaux)
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
        // CAS 2 : BoxCollider, SphereCollider, CapsuleCollider, etc.
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

#if UNITY_EDITOR //DEBUG à checker
    private void OnDrawGizmos()
    {
        if (rayAnchor == null) return;

        // Couleur selon détection
        Color rayColor = Color.yellow;
        if (currentProvider != null) rayColor = Color.green;
        else if (currentMaterialMapping != null) rayColor = Color.cyan;

        Gizmos.color = rayColor;
        Gizmos.DrawRay(rayAnchor.position, Vector3.down * maxGroundDist);

        if (Physics.Raycast(rayAnchor.position, Vector3.down, out RaycastHit hit,
            maxGroundDist))
        {
            Gizmos.DrawWireSphere(hit.point, 0.1f);

            // Label
            string label = "No detection";
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