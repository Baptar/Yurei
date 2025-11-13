using UnityEngine;

public class BubbleTransform : MonoBehaviour
{
    public static BubbleTransform Instance;
    private Camera cam;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (cam == null) return;
        
        // Faire face à la caméra
        Vector3 dir = transform.position - cam.transform.position;
        dir.y = 0f;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void SetCameraBubble(Camera camera)
    {
        cam = camera;
    }
}
