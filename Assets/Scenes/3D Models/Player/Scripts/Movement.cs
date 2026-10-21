
using UnityEngine;

public class Movement : MonoBehaviour
{
    // ============================================================
    // GROUND AND FLIGHT SPEEDS
    // ============================================================

    [Header("Ground Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Flight Movement")]
    [SerializeField] private float flySpeed = 12f;
    [SerializeField] private float flyboost = 18f;
    [SerializeField] private float flyAcceleration = 20f;
    [SerializeField] private float flyDrag = 5f;
    [SerializeField] private float flightTurnSpeed = 10f;

    [Header("Camera Look")]
    [SerializeField] private float lookSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 80f;

    // ============================================================
    // MOVEMENT STATE
    // ============================================================

    private CharacterController controller;
    private Transform cameraTransform;
    private Animator animate;

    private Vector3 flyVelocity;
    private Vector3 velocity;

    private float verticalLookRotation;
    private bool isGrounded;
    private bool isFlying;
    private bool jumpAnimationPending;
    private bool isJumpAnimationActive;
    private bool isRunJumping;

    // ============================================================
    // ANIMATOR HASHES
    // Keep these names identical to your Animator parameters.
    // ============================================================

    private int WalkingHash;
    private int RunningHash;
    private int RunStopHash;
    private int WalkingBackHash;
    private int WalkingLeftHash;
    private int WalkingRightHash;
    private int JumpingHash;
    private int RunJumpingHash;
    private int FlyingHash;
    private int DescendingHash;
    private int AscendingHash;

    // ============================================================
    // INITIALIZATION
    // ============================================================

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main != null
            ? Camera.main.transform
            : null;

        animate = GetComponent<Animator>();

        if (controller == null)
        {
            Debug.LogError(
                "Movement requires a CharacterController component.",
                this
            );

            enabled = false;
            return;
        }

        if (animate == null)
        {
            Debug.LogWarning(
                "Animator component not found on the player.",
                this
            );
        }
        else
        {
            WalkingHash = Animator.StringToHash("IsWalking");
            RunningHash = Animator.StringToHash("IsRunning");
            RunStopHash = Animator.StringToHash("IsRunStopping");
            WalkingBackHash = Animator.StringToHash("IsWalkingBack");
            WalkingLeftHash = Animator.StringToHash("IsWalkingLeft");
            WalkingRightHash = Animator.StringToHash("IsWalkingRight");
            JumpingHash = Animator.StringToHash("IsJumping");
            RunJumpingHash = Animator.StringToHash("IsRunJumping");
            FlyingHash = Animator.StringToHash("IsFlying");
            DescendingHash = Animator.StringToHash("IsDescending");
            AscendingHash = Animator.StringToHash("IsAscending");
        }

        SetCursorState(true);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    private void Update()
    {
        // Toggle flight mode.
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlight();
        }

        // Handle movement.
        Move();

        // Cursor controls.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(false);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            SetCursorState(true);
        }
    }

    // ============================================================
    // FLIGHT MODE TOGGLE
    // ============================================================

    private void ToggleFlight()
    {
        isFlying = !isFlying;

        // Reset old momentum when switching movement modes.
        flyVelocity = Vector3.zero;
        velocity = Vector3.zero;

        if (animate != null)
        {
            animate.SetBool(FlyingHash, isFlying);

            // Clear flight-specific animations when landing.
            if (!isFlying)
            {
                animate.SetBool(AscendingHash, false);
                animate.SetBool(DescendingHash, false);
            }
            else
            {
                jumpAnimationPending = false;
                isJumpAnimationActive = false;
                isRunJumping = false;
                animate.SetBool(JumpingHash, false);
                animate.SetBool(RunJumpingHash, false);
            }
        }
    }

    // ============================================================
    // MOVEMENT ROUTER
    // ============================================================

    private void Move()
    {
        if (controller == null)
            return;

        if (animate != null)
        {
            animate.SetBool(FlyingHash, isFlying);
        }

        if (isFlying)
        {
            HandleFlightMovement();
        }
        else
        {
            HandleGroundMovement();
        }
    }

    // ============================================================
    // GROUND MOVEMENT
    // ============================================================

    private void HandleGroundMovement()
    {
        // Ground detection.
        Vector3 groundCheckPosition =
            controller.bounds.center -
            Vector3.up *
            (controller.bounds.extents.y + groundCheckRadius);

        isGrounded = Physics.CheckSphere(
            groundCheckPosition,
            groundCheckRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        bool jumpTriggered = false;

        if (isGrounded)
        {
            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );

                jumpTriggered = true;
            }
        }

        // WASD and arrow-key input.
        Vector2 moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow))
            moveInput.y += 1f;

        if (Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow))
            moveInput.y -= 1f;

        if (Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow))
            moveInput.x -= 1f;

        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow))
            moveInput.x += 1f;

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool isRunning =
            moveInput.y > 0.1f &&
            Input.GetKey(KeyCode.LeftShift);

        bool runStopped =
            Input.GetKeyUp(KeyCode.LeftShift);

        if (jumpTriggered)
        {
            jumpAnimationPending = true;
            isRunJumping =
                Input.GetKey(KeyCode.W) &&
                Input.GetKey(KeyCode.LeftShift);
        }

        if (jumpAnimationPending && !isGrounded)
        {
            jumpAnimationPending = false;
            isJumpAnimationActive = true;
        }
        else if (isJumpAnimationActive && isGrounded)
        {
            isJumpAnimationActive = false;
            isRunJumping = false;
        }

        if (isMoving && cameraTransform != null)
        {
            // Keep ground movement relative to the camera.
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 worldMoveDirection =
                forward * moveInput.y +
                right * moveInput.x;

            worldMoveDirection.Normalize();

            float speed = isRunning ? runSpeed : walkSpeed;

            controller.Move(
                worldMoveDirection * speed * Time.deltaTime
            );

            // Face the camera's horizontal viewing direction.
            if (forward.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }

        // Apply gravity separately because CharacterController.Move
        // does not apply gravity automatically.
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        // Ground animation states.
        if (animate != null)
        {
            animate.SetBool(
                WalkingHash,
                moveInput.y > 0.1f && !isRunning
            );

            animate.SetBool(RunningHash, isRunning);
            animate.SetBool(RunStopHash, runStopped);

            animate.SetBool(
                WalkingBackHash,
                moveInput.y < -0.1f
            );

            animate.SetBool(
                WalkingLeftHash,
                moveInput.x < -0.1f
            );

            animate.SetBool(
                WalkingRightHash,
                moveInput.x > 0.1f
            );

            animate.SetBool(
                JumpingHash,
                !isRunJumping &&
                (jumpAnimationPending || isJumpAnimationActive)
            );

            animate.SetBool(
                RunJumpingHash,
                isRunJumping &&
                (jumpAnimationPending || isJumpAnimationActive)
            );

            // Prevent flight animations from carrying over.
            animate.SetBool(AscendingHash, false);
            animate.SetBool(DescendingHash, false);
        }
    }

    // ============================================================
    // FLIGHT MOVEMENT
    // ============================================================

    private void HandleFlightMovement()
    {
        if (cameraTransform == null)
            return;

        // --------------------------------------------------------
        // READ FLIGHT INPUT
        // --------------------------------------------------------

        float horizontalInput = 0f;
        float verticalInput = 0f;

        bool movingForward =
            Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow);

        bool movingLeft =
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow);

        bool movingRight =
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow);

        bool ascending = Input.GetKey(KeyCode.Space);
        bool descending = Input.GetKey(KeyCode.LeftShift);

        if (movingRight) horizontalInput += 1f;
        if (movingLeft) horizontalInput -= 1f;

        if (movingForward) verticalInput += 1f;

        // --------------------------------------------------------
        // CAMERA-RELATIVE 3D MOVEMENT
        // --------------------------------------------------------

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        Vector3 inputDirection =
            cameraForward * verticalInput +
            cameraRight * horizontalInput;

        if (ascending)
            inputDirection += Vector3.up;

        if (descending)
            inputDirection -= Vector3.up;

        // Prevent diagonal movement from being faster.
        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        // --------------------------------------------------------
        // FLIGHT BOOST
        // Hold left mouse button while moving forward to boost.
        // Right mouse button remains available for HeatVision.
        // --------------------------------------------------------

        bool isBoosting =
            movingForward && Input.GetMouseButton(0);

        float maxSpeed = isBoosting ? flyboost : flySpeed;

        // Desired flight velocity.
        Vector3 targetVelocity = inputDirection * maxSpeed;

        // --------------------------------------------------------
        // ACCELERATION AND DECELERATION
        // --------------------------------------------------------

        if (inputDirection.sqrMagnitude > 0.001f)
        {
            // Accelerate smoothly toward the target velocity.
            flyVelocity = Vector3.MoveTowards(
                flyVelocity,
                targetVelocity,
                flyAcceleration * Time.deltaTime
            );
        }
        else
        {
            // Gradually slow down after releasing movement keys.
            flyVelocity = Vector3.MoveTowards(
                flyVelocity,
                Vector3.zero,
                flyDrag * Time.deltaTime
            );
        }

        // Enforce the current speed limit, including boost changes.
        flyVelocity = Vector3.ClampMagnitude(
            flyVelocity,
            maxSpeed
        );

        // --------------------------------------------------------
        // APPLY FLIGHT MOVEMENT
        // --------------------------------------------------------

        CollisionFlags collisionFlags = controller.Move(
            flyVelocity * Time.deltaTime
        );

        // Remove velocity directed into a blocking surface.
        if ((collisionFlags & CollisionFlags.Above) != 0 &&
            flyVelocity.y > 0f)
        {
            flyVelocity.y = 0f;
        }

        if ((collisionFlags & CollisionFlags.Below) != 0 &&
            flyVelocity.y < 0f)
        {
            flyVelocity.y = 0f;
        }

        // --------------------------------------------------------
        // FULL 3D PLAYER ROTATION
        // --------------------------------------------------------

        Vector3 lookDirection = cameraTransform.forward;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                flightTurnSpeed * Time.deltaTime
            );
        }

        // --------------------------------------------------------
        // FLIGHT ANIMATIONS
        // --------------------------------------------------------

        if (animate != null)
        {
            animate.SetBool(AscendingHash, ascending);
            animate.SetBool(DescendingHash, descending);

            // Clear ground animations during flight.
            animate.SetBool(WalkingHash, false);
            animate.SetBool(RunningHash, false);
            animate.SetBool(RunStopHash, false);
            animate.SetBool(WalkingBackHash, false);
            animate.SetBool(WalkingLeftHash, false);
            animate.SetBool(WalkingRightHash, false);
            animate.SetBool(JumpingHash, false);
            animate.SetBool(RunJumpingHash, false);
        }
    }

    // ============================================================
    // CURSOR CONTROLS
    // ============================================================

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }

    // ============================================================
    // OPTIONAL GROUND-CHECK VISUALIZATION
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        CharacterController characterController =
            GetComponent<CharacterController>();

        if (characterController == null)
            return;

        Vector3 groundCheckPosition =
            characterController.bounds.center -
            Vector3.up *
            (characterController.bounds.extents.y + groundCheckRadius);

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            groundCheckPosition,
            groundCheckRadius
        );
    }
}
