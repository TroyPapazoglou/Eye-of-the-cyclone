using UnityEngine;
[RequireComponent(typeof(CharacterController))]

[RequireComponent(typeof(GameplayPlayer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] private PlayerLook playerLook;

    [Header("Hands")]
    [SerializeField] private HandController handController;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Knockback")]
    [SerializeField] private float knockbackDecay = 20f;

    private Vector3 knockbackVelocity;

    private CharacterController characterController;

    private Vector2 moveInput;
    private Vector3 velocity;
    private bool wasMoving;
    private bool jumpHeld;

    private GameplayPlayer gameplayPlayer;


    public CharacterController CharacterController => characterController;

    public bool IsMoving { get; private set; }

    public bool Grounded => characterController.isGrounded;

    public float VerticalVelocity => velocity.y;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        gameplayPlayer = GetComponent<GameplayPlayer>();
    }

    private void OnDisable()
    {
        playerLook.CanLook = false;
    }

    private void OnEnable()
    {
        playerLook.CanLook = true;
    }

    private void Update()
    {
        if (!gameplayPlayer.isOwned)
            return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        IsMoving = moveInput.sqrMagnitude > 0.001f;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        move = Vector3.ClampMagnitude(move, 1f);

        bool grounded = characterController.isGrounded;

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (jumpHeld && grounded)
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 totalVelocity =
            moveSpeed * move +
            velocity +
            knockbackVelocity;

        characterController.Move(
            totalVelocity * Time.deltaTime
        );

        knockbackVelocity = Vector3.MoveTowards(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );

        bool groundedAfterMove = characterController.isGrounded;

        UpdateHandPose(groundedAfterMove);
    }

    private void UpdateHandPose(bool grounded)
    {
        if (handController == null)
            return;

        if (!grounded)
        {          
            handController.SetJumping();

            wasMoving = false;
            return;
        }

        if (IsMoving)
        {
            if (!wasMoving)
            {
                Vector3 movementDirection = new Vector3(moveInput.x, 0f, moveInput.y);

                handController.SetRunning(movementDirection);
            }

            wasMoving = true;
        }
        else
        {
            handController.SetIdle();

            wasMoving = false;
        }
    }

    public void ApplyExplosionForce(Vector3 direction, float force)
    {
        knockbackVelocity += direction * force;
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetJumpHeld(bool held)
    {
        jumpHeld = held;
    }
}
