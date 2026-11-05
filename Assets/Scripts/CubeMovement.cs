using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class CubeMovement : MonoBehaviour
{
    [Header("Camera (optional, uses Main Camera if empty)")]
    public Transform cameraTransform;
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpHeight = 1.8f;
    public float gravity = 25f;
    [Header("Dash")]
    public float dashSpeed = 16f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;

    private CharacterController controller;
    private Vector3 lastDirection = Vector3.forward;
    private Vector3 dashDirection;
    private float verticalSpeed;
    private float dashTimer;
    private float cooldownTimer;
    private float coyoteTimer;
    private float jumpBuffer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        Vector2 input = Vector2.zero;
        bool jumpPressed = false;
        bool dashPressed = false;
#if ENABLE_INPUT_SYSTEM
        Keyboard keys = Keyboard.current;
        if (keys != null)
        {
            input.x = (keys.dKey.isPressed || keys.rightArrowKey.isPressed ? 1 : 0)
                    - (keys.aKey.isPressed || keys.leftArrowKey.isPressed ? 1 : 0);
            input.y = (keys.wKey.isPressed || keys.upArrowKey.isPressed ? 1 : 0)
                    - (keys.sKey.isPressed || keys.downArrowKey.isPressed ? 1 : 0);
            jumpPressed = keys.spaceKey.wasPressedThisFrame;
            dashPressed = keys.leftShiftKey.wasPressedThisFrame;
        }
#elif ENABLE_LEGACY_INPUT_MANAGER
        input.x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
        input.y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
        jumpPressed = Input.GetKeyDown(KeyCode.Space);
        dashPressed = Input.GetKeyDown(KeyCode.LeftShift);
#endif
        input = Vector2.ClampMagnitude(input, 1f);
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        cooldownTimer = Mathf.Max(0f, cooldownTimer - dt);
        jumpBuffer = Mathf.Max(0f, jumpBuffer - dt);
        if (jumpPressed) jumpBuffer = 0.15f;

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f && cameraTransform != null)
            forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();
        Vector3 direction = forward * input.y + Vector3.Cross(Vector3.up, forward) * input.x;
        if (direction.sqrMagnitude > 0.001f) lastDirection = direction.normalized;

        if (controller.isGrounded && verticalSpeed <= 0f)
        {
            verticalSpeed = -2f;
            coyoteTimer = 0.12f;
        }
        else coyoteTimer = Mathf.Max(0f, coyoteTimer - dt);

        if (dashPressed && dashTimer <= 0f && cooldownTimer <= 0f)
        {
            dashDirection = lastDirection;
            dashTimer = Mathf.Max(0.01f, dashDuration);
            jumpBuffer = 0f;
        }

        Vector3 movement;
        if (dashTimer > 0f)
        {
            // Horizontal dash; pause vertical movement during the dash.
            // Average the final partial frame to keep dash distance consistent.
            float dashStep = Mathf.Min(dt, dashTimer);
            movement = dashDirection * dashSpeed * dashStep;
            dashTimer = Mathf.Max(0f, dashTimer - dt);
            if (dashTimer <= 0f) cooldownTimer = dashCooldown;
            jumpBuffer = 0f;
        }
        else
        {
            if (jumpBuffer > 0f && coyoteTimer > 0f)
            {
                verticalSpeed = Mathf.Sqrt(2f * Mathf.Max(0.01f, gravity) * Mathf.Max(0f, jumpHeight));
                jumpBuffer = 0f;
                coyoteTimer = 0f;
            }
            verticalSpeed = Mathf.Max(verticalSpeed - Mathf.Max(0.01f, gravity) * dt, -35f);
            movement = (direction * moveSpeed + Vector3.up * verticalSpeed) * dt;
        }
        CollisionFlags hits = controller.Move(movement);
        if ((hits & CollisionFlags.Above) != 0 && verticalSpeed > 0f)
            verticalSpeed = 0f;
    }

    private void OnDisable()
    {
        dashTimer = 0f;
        cooldownTimer = 0f;
        jumpBuffer = 0f;
        coyoteTimer = 0f;
        verticalSpeed = 0f;
    }
}
