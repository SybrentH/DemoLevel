using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
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
    private Animator animator;
    private Vector2 moveInput;
    private Vector3 lastDirection = Vector3.forward;
    private Vector3 dashDirection;
    private float verticalSpeed;
    private float dashTimer;
    private float cooldownTimer;
    private float coyoteTimer;
    private float jumpBuffer;
    private float rotationSpeed = 12f;
    private bool dashQueued;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void OnMove(InputValue value)
    {
        moveInput = Vector2.ClampMagnitude(value.Get<Vector2>(), 1f);
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBuffer = 0.15f;
        }
    }

    void OnDash(InputValue value)
    {
        if (value.isPressed)
        {
            dashQueued = true;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        cooldownTimer = Mathf.Max(0f, cooldownTimer - dt);
        jumpBuffer = Mathf.Max(0f, jumpBuffer - dt);

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f && cameraTransform != null)
        {
            forward = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
        }
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }
        forward.Normalize();

        Vector3 direction = forward * moveInput.y + Vector3.Cross(Vector3.up, forward) * moveInput.x;
        if (direction.sqrMagnitude > 0.001f)
        {
            lastDirection = direction.normalized;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }

        if (direction.sqrMagnitude > 0.001f && dashTimer <= 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * dt);
        }

        if (controller.isGrounded && verticalSpeed <= 0f)
        {
            verticalSpeed = -2f;
            coyoteTimer = 0.12f;
        }
        else
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - dt);
        }

        if (dashQueued && dashTimer <= 0f && cooldownTimer <= 0f)
        {
            dashDirection = lastDirection;
            dashTimer = Mathf.Max(0.01f, dashDuration);
            jumpBuffer = 0f;
        }
        dashQueued = false;

        Vector3 movement;
        if (dashTimer > 0f)
        {
            float dashStep = Mathf.Min(dt, dashTimer);
            movement = dashDirection * dashSpeed * dashStep;
            dashTimer = Mathf.Max(0f, dashTimer - dt);

            if (dashTimer <= 0f)
            {
                cooldownTimer = dashCooldown;
            }
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
        {
            verticalSpeed = 0f;
        }
    }

    void OnDisable()
    {
        dashTimer = 0f;
        cooldownTimer = 0f;
        jumpBuffer = 0f;
        coyoteTimer = 0f;
        verticalSpeed = 0f;
        dashQueued = false;
        moveInput = Vector2.zero;
    }
}