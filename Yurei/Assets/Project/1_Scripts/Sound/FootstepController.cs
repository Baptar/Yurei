using UnityEngine;
using System.Collections;

public class FootstepController : MonoBehaviour
{
      [SerializeField] private string footstepEvent = "Play_Footstep";
      [SerializeField] private string surfaceSwitch = "Surface_Type";
      [SerializeField] private LayerMask groundLayer;

      private void Start()
      {
            //AudioServices.Events.RegisterGameObject(gameObject);
      }

      public void OnFootstep()
      {
            // Détecter la surface sous le joueur
            string surfaceType = DetectSurface();

            // Configurer le switch avant de poster l'event
            AudioServices.Switches.SetSwitch(surfaceSwitch, surfaceType, gameObject);

            // Poster l'event
            AudioServices.Events.PostEvent(footstepEvent, gameObject);
      }

      private string DetectSurface()
      {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f, groundLayer))
            {
                  if (hit.collider.CompareTag("Wood"))
                        return "Wood";
                  else if (hit.collider.CompareTag("Metal"))
                        return "Metal";
                  else if (hit.collider.CompareTag("Water"))
                        return "Water";
            }

            return "Concrete"; // Default
      }

      private void OnDestroy()
      {
            //AudioServices.Events.UnregisterGameObject(gameObject);
      }
}