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

      [Header("Fallback")] //Si rien renseigné : paramètres par défaut
      private AK.Wwise.Switch defaultMaterial;
      private AK.Wwise.Switch defaultCondition;

      private FootstepSurfaceProvider currentProvider;
      private FootstepSurfaceProvider lastProvider;

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
                  // TODO : Raycast that returns hit.triangleIndex -> find the submesh -> get the associated material -> make a database with material name <-> Wwise surface switch.

                  // When the player is on a surface with FootstepSurfaceProvider, it updates the current surface.
                  if (hit.transform.TryGetComponent<FootstepSurfaceProvider>(out var provider))
                  {
                        currentProvider = provider;
                  }

            // Debug : si on change de surface, afficher le nouveau matériau
            if (currentProvider != lastProvider)
            {
                lastProvider = currentProvider;

                string mat = currentProvider.SurfaceMaterial?.Name ?? "Default";
                string cond = currentProvider.SurfaceCondition?.Name ?? "Default";

                Debug.Log($"Ground Surface changed → {mat}, {cond}", this);
            }
            }
      }

      // Fonction appelée à chaque pas
      public void TriggerFootstep()
      {
                  if (currentProvider != null)
                  {
                        currentProvider.SurfaceMaterial?.SetValue(footstepSource);
                        currentProvider.SurfaceCondition?.SetValue(footstepSource);

                        AudioServices.Events.PostEvent(playerWalkEvent, footstepSource);
                  }
                  else
                  {
                        // Si pas de FootstepProvider trouvé, utilise les valeurs par défaut
                        defaultMaterial?.SetValue(footstepSource);
                        defaultCondition?.SetValue(footstepSource);

                        AudioServices.Events.PostEvent(playerWalkEvent, footstepSource);
                  }
      }
}