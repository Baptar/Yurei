using UnityEngine;

[DisallowMultipleComponent]
public class FootstepController : MonoBehaviour
{
      [Header("Raycast")]
      [SerializeField] private Transform rayAnchor; //Endroit d'où est tiré le raycast - à placer entre les deux pieds
      private float maxGroundDist = 20f;

      [Header("Audio Settings")]
      [SerializeField] private AK.Wwise.Event footstepEvent;
      [SerializeField] private GameObject footstepSource; // Source audio (pied player)

      [Header("Fallback")] //Si rien renseigné : paramètres par défaut
      [SerializeField] private AK.Wwise.Switch defaultMaterial;
      [SerializeField] private AK.Wwise.Switch defaultCondition;

      private FootstepProvider currentProvider;

      private void Start()
      {
            // Si pas défini, son des pas sera émis du gameObject sur lequel est ce script
            if (footstepSource == null)
            {
                  footstepSource = gameObject;
            }
      }

      // Si le personnage est sur une surface avec FootstepProvider, le détecte et donne la valeur à TriggerFootstep()
      void FixedUpdate()
      {
            if (Physics.Raycast(rayAnchor.position, Vector3.down, out RaycastHit hit,
                maxGroundDist))
            {
                  if (hit.transform.TryGetComponent<FootstepProvider>(out var provider))
                  {
                        currentProvider = provider;
                  }
            }
      }

      // Fonction appelée à chaque pas
      public void TriggerFootstep(AnimationEvent animationEvent)
      {
            if (animationEvent.animatorClipInfo.weight > 0.5f) //J'ai pické ce if du character controller de Unity
            {
                  if (currentProvider != null)
                  {
                        currentProvider.SurfaceMaterial?.SetValue(footstepSource);
                        currentProvider.SurfaceCondition?.SetValue(footstepSource);

                        AudioServices.Events.PostEvent(footstepEvent, footstepSource);
                  }
                  else
                  {
                        // Si pas de FootstepProvider trouvé, utilise les valeurs par défaut
                        defaultMaterial?.SetValue(footstepSource);
                        defaultCondition?.SetValue(footstepSource);

                        AudioServices.Events.PostEvent(footstepEvent, footstepSource);
                  }
            }
      }
}