using System.Collections;
using UnityEngine;

/// <summary>
/// Handles only wall sliding and wall jumping.
/// Requires the PlayerMovement script to be on the same GameObject.
/// </summary>
public class PlayerWallSlide : MonoBehaviour
{
    [Header("Wall Slide")]
    [Tooltip("The max speed the player slides down the wall.")]
    public float wallSlideSpeed = 2f;
    [Tooltip("How long (in seconds) it takes to decelerate to wall slide speed. Lower = stickier.")]
    public float wallSlideSmoothTime = 0.1f;

    [Header("Wall Jump")]
    [Tooltip("The angle (in degrees) of the wall jump. 45-60 is a good start.")]
    public float wallJumpAngle = 45f;
    [Tooltip("The force applied when wall jumping.")]
    public float wallJumpForce = 20f;
    [Tooltip("How long to prevent wall jumping after a wall jump.")]
    public float wallJumpControlCooldown = 20f;

    [Header("Coyote Time")]
    [Tooltip("Time (in seconds) the player can still wall jump after leaving the wall.")]
    public float wallJumpCoyoteTime = 0.15f;

    [Header("Ability Toggles")]
    public bool hasWallClaws = false;

    // --- Public Properties ---
    public bool IsWallSliding { get; private set; }
    public bool IsTouchingWall { get { return isTouchingWall; } }
    public float WallSide { get { return wallSide; } }

    // --- Private State ---
    private bool isTouchingWall = false;
    private float wallSide = 0f;
    private bool inWallJumpCooldown = false;
    private float currentVelocityY;
    private float wallJumpCoyoteTimer;

    // --- References ---
    private Rigidbody2D rb;
    private PlayerMovement movementScript;
    private Animator anim; // optional for animation

    void Awake()
    {
        movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null)
            rb = movementScript.rb;
        else
            Debug.LogError("PlayerWallSlide script requires a PlayerMovement script on the same GameObject!");

        anim = GetComponent<Animator>(); // optional
    }

    void Update()
    {
        UpdateWallSlidingState();

        // Coyote timer update
        if (IsWallSliding)
            wallJumpCoyoteTimer = wallJumpCoyoteTime;
        else
            wallJumpCoyoteTimer -= Time.deltaTime;

        // Animation update
        if (anim != null)
            anim.SetBool("isWallSliding", IsWallSliding);
    }

    void FixedUpdate()
    {
        if (IsWallSliding)
        {
            float targetYVelocity = -wallSlideSpeed;

            float newYVelocity = Mathf.SmoothDamp(
                rb.linearVelocity.y,
                targetYVelocity,
                ref currentVelocityY,
                wallSlideSmoothTime
            );

            // Keep X velocity unaffected if needed (important!)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, newYVelocity);
        }
        else
        {
            currentVelocityY = 0f;
        }

        // Reset each physics step
        isTouchingWall = false;
        wallSide = 0f;
    }

    private void UpdateWallSlidingState()
    {
        bool isGrounded = movementScript.IsGrounded;

        // Wallslide triggers only when touching wall, moving down, and not grounded
        IsWallSliding = isTouchingWall && rb.linearVelocity.y < 0f && !isGrounded;
    }

    public void HandleWallJump()
    {
        if (!IsWallSliding && wallJumpCoyoteTimer <= 0)
            return;

        if (!hasWallClaws)
            return;

        if (inWallJumpCooldown)
            return;

        float angleInRadians = wallJumpAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        dir.x *= -wallSide;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir.normalized * wallJumpForce, ForceMode2D.Impulse);

        StartCoroutine(WallJumpCooldown());
    }

    private IEnumerator WallJumpCooldown()
    {
        inWallJumpCooldown = true;
        wallJumpCoyoteTimer = 0f;
        yield return new WaitForSeconds(wallJumpControlCooldown);
        inWallJumpCooldown = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.collider.CompareTag("Wall") && Mathf.Abs(contact.normal.x) > 0.5f)
            {
                isTouchingWall = true;
                wallSide = -Mathf.Sign(contact.normal.x);
                break;
            }
        }
    }
}