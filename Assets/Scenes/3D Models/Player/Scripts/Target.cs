using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private float depth = 6f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = depth;

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

        if (rb != null)
        {
            rb.MovePosition(worldPosition);
        }
        else
        {
            transform.position = worldPosition;
        }
    }
}