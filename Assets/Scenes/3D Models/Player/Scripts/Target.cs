using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float defaultDepth = 15f; // Distance when looking at the sky
    [SerializeField] private LayerMask aimLayers;      // Layers the target can snap to (e.g., Ground, Buildings)

    private Rigidbody rb;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        if (mainCamera == null) return;

        // 1. Create a ray extending straight from the center of the viewport/crosshair
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPosition;

        // 2. Cast the ray into the world to see what the camera is looking at
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayers))
        {
            // Snap the target directly to the surface of the object you are aiming at
            targetPosition = hit.point;
        }
        else
        {
            // If aiming at the sky or open air, project it at a fixed distance forward
            targetPosition = ray.GetPoint(defaultDepth);
        }

        // 3. Move the object
        if (rb != null)
        {
            rb.MovePosition(targetPosition);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
}