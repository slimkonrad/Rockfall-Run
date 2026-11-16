using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves the player left/right and handles jumping using actions from an Input Actions asset.
/// Movement is physics-based through Rigidbody2D. A Physics2D.OverlapCircle at the player's feet
/// checks for ground, so the player can only jump while standing on something.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float fastFallSpeed = 15f;

    [Header("Ground Check")]
    [Tooltip("Empty child object placed at the player's feet.")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Dust puff at the player's feet, fired with Emit() on every jump.")]
    [SerializeField] private ParticleSystem jumpDust;
    [SerializeField] private int jumpDustCount = 10;
    [Tooltip("Tick this if the original sprite art faces right. Leave it unticked if it faces left.")]
    [SerializeField] private bool spriteFacesRight = false;

    private Rigidbody2D rb;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void Update()
    {
        // Ignore input while the game is paused or the win screen is up.
        if (Time.timeScale == 0f)
        {
            return;
        }

        // Input is checked in Update so a quick key press is never missed.
        if (jumpAction.action.WasPressedThisFrame() && isGrounded)
        {
            Jump();
        }

        UpdateFacing();
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
        jumpDust.Emit(jumpDustCount);
    }

    private void FixedUpdate()
    {
        CheckGround();

        // Physics movement belongs in FixedUpdate.
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        float yVelocity = rb.linearVelocity.y;

        // Holding Down in mid-air drops the player quickly.
        if (!isGrounded && moveInput.y < 0f)
        {
            yVelocity = Mathf.Min(yVelocity, -fastFallSpeed);
        }

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, yVelocity);
    }

    /// <summary>
    /// Grounded only when the feet overlap the Ground layer and the player isn't still rising,
    /// so the jump can't be re-triggered in the first moments after leaving the ground.
    /// </summary>
    private void CheckGround()
    {
        bool feetOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = feetOnGround && rb.linearVelocity.y <= 0.01f;
    }

    /// <summary>
    /// Mirrors the sprite to face the direction the player is moving.
    /// Flipping the sprite (instead of rotating the object) leaves the collider and physics untouched.
    /// </summary>
    private void UpdateFacing()
    {
        float moveX = moveAction.action.ReadValue<Vector2>().x;
        if (moveX > 0f)
        {
            spriteRenderer.flipX = !spriteFacesRight;
        }
        else if (moveX < 0f)
        {
            spriteRenderer.flipX = spriteFacesRight;
        }
        // When no key is held, keep facing the last direction.
    }

    /// <summary>Draws the ground check circle in the Scene view so it is easy to position.</summary>
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}