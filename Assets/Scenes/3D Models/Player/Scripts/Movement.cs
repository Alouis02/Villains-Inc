using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    // Variables
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float flySpeed = 6f;

    // Fly Movement Variables
    [SerializeField] private float flyAcceleration = 20f;
    [SerializeField] private float flyDrag = 5f;
    [SerializeField] private float lookSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 80f;

    private Vector3 flyVelocity;
    private Vector3 velocity;
    private float verticalLookRotation;
    private bool isGrounded;
    private bool isFlying = false;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main?.transform;
        SetCursorState(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isFlying = !isFlying;
            flyVelocity = Vector3.zero;
            velocity = Vector3.zero;
        }
        
        Move();

        if (Input.GetKeyDown(KeyCode.Escape))
            SetCursorState(false);
        else if (Input.GetMouseButtonDown(0))
            SetCursorState(true);
    }

    private void Move()
    {
        if (controller == null) return;

        if (isFlying)
        {
            HandleFlightMovement();
            return;
        }

        HandleGroundMovement();
    }

    // ============================================================
    // GROUND MOVEMENT
    // ============================================================
    private void HandleGroundMovement()
    {
        Vector3 groundCheckPosition =
            controller.bounds.center +
            Vector3.down * (controller.bounds.extents.y + groundCheckRadius);

        isGrounded = Physics.CheckSphere(groundCheckPosition, groundCheckRadius, groundMask);

        if (isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            if (Input.GetKeyDown(KeyCode.Space))
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;

        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 worldMoveDirection =
                (forward * moveInput.y + right * moveInput.x).normalized;

            bool isRunning = moveInput.y > 0.1f &&
                             Keyboard.current.leftShiftKey.isPressed &&
                             Keyboard.current.wKey.isPressed;

            float speed = isRunning ? runSpeed : walkSpeed;

            controller.Move(worldMoveDirection * speed * Time.deltaTime);

            if (cameraTransform != null)
            {
                Vector3 lookDirection = cameraTransform.forward;
                lookDirection.y = 0f;

                if (lookDirection.magnitude > 0.1f)
                    transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ============================================================
    // FLIGHT MOVEMENT
    // ============================================================
    
    private void HandleFlightMovement()
    {
        if (cameraTransform == null)
            return;

        Vector3 input = Vector3.zero;

        if (Input.GetKey("w"))
            input += cameraTransform.forward;

        if (Input.GetKey(KeyCode.Space))
            input += Vector3.up;

        if (Input.GetKey(KeyCode.LeftShift))
            input -= Vector3.up;

        input.Normalize();

        // Smooth acceleration
        flyVelocity += input * flyAcceleration * Time.deltaTime;

        // Limit max speed
        flyVelocity = Vector3.ClampMagnitude(flyVelocity, flySpeed);

        // Smooth slowdown
        flyVelocity = Vector3.Lerp(
            flyVelocity,
            Vector3.zero,
            flyDrag * Time.deltaTime);

        controller.Move(flyVelocity * Time.deltaTime);

        // Face wherever the camera looks
        Vector3 look = cameraTransform.forward;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(look),
            10f * Time.deltaTime);
    }

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}