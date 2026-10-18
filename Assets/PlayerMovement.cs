using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles the player's movement, jumps, wall interactions, Super Jump, audio, 
/// particle effects, and mobile/PC input.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    // --- MERGED: Movement Settings & Smoothing ---
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;

    [Header("Movement Smoothing")]
    [Tooltip("How long it takes to reach target speed on ground. Lower = snappier.")]
    public float groundSmoothTime = 0.1f;
    [Tooltip("How long it takes to stop on ground.")]
    public float groundStopSmoothTime = 0.05f;
    [Tooltip("How long it takes to reach target speed on ice.")]
    public float iceSmoothTime = 0.5f;
    [Tooltip("How long it takes to stop on ice.")]
    public float iceStopSmoothTime = 1f;
    [Tooltip("How long it takes to change direction in mid-air. Higher = floatier.")]
    public float airSmoothTime = 0.25f;
    [Tooltip("How long it takes to stop moving in mid-air (if not pressing keys).")]
    public float airStopSmoothTime = 0.2f;

    // --- MERGED: Ground Check ---
    [Header("Ground Check (Raycast)")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRayLength = 0.2f;
    public float groundCheckRaySpread = 0.4f;

    // --- MERGED: Mobile Controls (ADDED Joystick) ---
    [Header("Mobile Controls")]
    [Tooltip("Reference to the external Joystick script.")]
    public Joystick joystick;
    private bool jumpPressed = false; // Flag for mobile jump button

    // --- MERGED: Audio ---
    [Header("Audio")]
    public AudioClip runningSound;
    public AudioClip landingSound;
    public AudioClip fallingSound;
    [Range(0f, 1f)] public float runningVolume = 0.5f;
    [Range(0f, 1f)] public float landingVolume = 0.8f;
    public float runningAudioCutoffDelay = 0.15f;
    public float fallingAudioStopDelay = 0.1f;
    public float fallingAudioFadeOutDuration = 0.2f;
    public AudioClip superJumpSound;
    [Range(0f, 1f)] public float superJumpVolume = 1.0f;

    // --- MERGED: Components & Effects ---
    [Header("Components & Effects")]
    public Animator anim;
    [Tooltip("The Particle System component for running dust.")]
    [SerializeField] private ParticleSystem DustParticles;
    [Tooltip("The Particle System component for landing dust.")]
    [SerializeField] private ParticleSystem LandingDustParticles;

    // --- Internal Components ---
    [HideInInspector] public Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private AudioSource audioSource; // For running
    private AudioSource fallingSource; // For falling
    private PlayerWallSlide wallSlideScript;

    // --- Movement State ---
    public bool IsGrounded { get; private set; }
    private bool wasGrounded = true;
    private bool isOnSlipperySurface = false;
    private bool facingLeft = true;
    private bool canControl = true;
    private float horizontalInput = 0f;
    private float currentVelocityX;
    private float audioCutoffTimer;
    private Coroutine fallingStopCoroutine;
    private bool audioFrozen = false;
    
    // --- Super Jump State ---
    private bool isSuperJumpActive = false;
    private float superJumpMultiplier = 1f;
    private int penaltyDamage;
    private float penaltyStunDuration;
    private bool isLandingCharged = false;

    // --- References ---
    private PlayerHealth playerHealth;
    private PlayerEffects playerEffects;

    void Start()
    {
        // Get Components
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        wallSlideScript = GetComponent<PlayerWallSlide>();
        playerHealth = GetComponent<PlayerHealth>();
        playerEffects = GetComponent<PlayerEffects>();

        // Ensure Rigidbody is active
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        // Animator setup
        if (anim == null) anim = GetComponent<Animator>();

        // Audio setup (Running Audio)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        // Audio setup (Falling Audio) - Dedicated source
        fallingSource = gameObject.AddComponent<AudioSource>();
        fallingSource.playOnAwake = false;
        fallingSource.loop = true;
        fallingSource.volume = runningVolume; // Use running volume as default
    }

    void Update()
    {
        // --- Input Handling: Merged PC (Input.GetAxisRaw) and Mobile (joystick) ---
        if (canControl)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            // Overlay mobile input if joystick is present and active
            if (joystick != null)
            {
                // Prioritize joystick input if it's being used
                if (Mathf.Abs(joystick.Horizontal) > 0.01f)
                {
                    horizontalInput = joystick.Horizontal;
                }
            }
        }
        else
        {
            horizontalInput = 0f;
        }
        
        // Combine PC jump input and mobile jump button flag
        bool jumpInput = Input.GetButtonDown("Jump") || jumpPressed;
        jumpPressed = false; // Reset mobile flag

        if (jumpInput)
        {
            if (wallSlideScript != null && wallSlideScript.IsWallSliding)
                wallSlideScript.HandleWallJump();
            else if (IsGrounded)
                HandleRegularJump();
        }

        // --- Visual & Audio Updates (Frame-rate dependent) ---
        HandleFlip();
        HandleRunningAudio();
        ControlFallingSound();
        StartStopRunningParticles();

        // --- Animator ---
        if (anim != null)
        {
            anim.SetBool("isGrounded", IsGrounded);
            // Use the horizontal input to determine speed for animation
            anim.SetFloat("speed", IsGrounded ? Mathf.Abs(horizontalInput) : 0f);
        }
    }

    void FixedUpdate()
    {
        // --- Landing Check (Must run before CheckGrounded is updated) ---
        if (!wasGrounded && IsGrounded)
        {
            PlayLandingSound();
            SpawnLandingDust();
            ApplyLandingPenaltyCheck();
        }
        wasGrounded = IsGrounded;

        // --- Core Movement Logic (Physics-based) ---
        CheckGrounded();
        HandleMovement();
    }

    // --------------------------------------------------------------------------------
    // Core Movement & Jumping
    // --------------------------------------------------------------------------------

    private void CheckGrounded()
    {
        // Raycast logic for robust ground detection
        if (groundCheck == null)
        {
            IsGrounded = false;
            isOnSlipperySurface = false;
            return;
        }

        Vector2 pos = groundCheck.position;
        Vector2 leftRayPos = new Vector2(pos.x - groundCheckRaySpread, pos.y);
        Vector2 rightRayPos = new Vector2(pos.x + groundCheckRaySpread, pos.y);

        RaycastHit2D hitMiddle = Physics2D.Raycast(pos, Vector2.down, groundCheckRayLength, groundLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(leftRayPos, Vector2.down, groundCheckRayLength, groundLayer);
        RaycastHit2D hitRight = Physics2D.Raycast(rightRayPos, Vector2.down, groundCheckRayLength, groundLayer);

        if (hitMiddle.collider != null || hitLeft.collider != null || hitRight.collider != null)
        {
            IsGrounded = true;
            RaycastHit2D hit = hitMiddle.collider != null ? hitMiddle : hitLeft.collider != null ? hitLeft : hitRight;
            
            // Check for slippery surface
            isOnSlipperySurface = hit.collider.CompareTag("Slippery");

            // Super Jump Landing Safe Check
            if (hit.collider.CompareTag("ChargingPad") && isLandingCharged)
            {
                isLandingCharged = false;
                DeactivateSuperJump();
            }
        }
        else
        {
            IsGrounded = false;
            isOnSlipperySurface = false;
        }
    }

    private void HandleMovement()
    {
        if (!canControl) return;

        bool isWallSliding = wallSlideScript != null && wallSlideScript.IsWallSliding;
        if (isWallSliding)
        {
            currentVelocityX = 0f;
            return;
        }

        float wallSide = wallSlideScript != null ? wallSlideScript.WallSide : 0f;
        bool isTouchingWall = wallSlideScript != null && wallSlideScript.IsTouchingWall;

        float targetSpeed = horizontalInput * moveSpeed;
        float currentSpeed = rb.linearVelocity.x;

        // Prevent movement into the wall if touching it
        if (isTouchingWall && ((horizontalInput > 0 && wallSide == 1) || (horizontalInput < 0 && wallSide == -1)))
        {
            targetSpeed = 0f;
            currentSpeed = 0f;
            currentVelocityX = 0f;
        }

        // Determine smoothing time based on state
        float smoothTime;
        if (IsGrounded)
        {
            if (isOnSlipperySurface)
            {
                bool isTryingToMove = Mathf.Abs(horizontalInput) > 0.01f;
                bool isReversing = (currentSpeed > 0 && horizontalInput < 0) || (currentSpeed < 0 && horizontalInput > 0);
                smoothTime = (isReversing || !isTryingToMove) ? iceStopSmoothTime : iceSmoothTime;
            }
            else
            {
                smoothTime = Mathf.Abs(targetSpeed) > 0.01f ? groundSmoothTime : groundStopSmoothTime;
            }
        }
        else // In the air
        {
            smoothTime = Mathf.Abs(targetSpeed) > 0.01f ? airSmoothTime : airStopSmoothTime;
        }

        // Apply smoothed horizontal movement
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref currentVelocityX, smoothTime);
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    private void HandleRegularJump()
    {
        float finalJumpForce = jumpForce * superJumpMultiplier;
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset vertical velocity for consistent jump height
        rb.AddForce(Vector2.up * finalJumpForce, ForceMode2D.Impulse);

        // Super Jump Logic
        if (isSuperJumpActive)
        {
            isLandingCharged = true;
            if (superJumpSound != null)
            {
                AudioSource.PlayClipAtPoint(superJumpSound, transform.position, superJumpVolume);
            }
        }
        
        DeactivateSuperJump();
    }
    
    // Public function for the mobile jump button to call
    public void OnJumpButtonPressed() => jumpPressed = true;

    // --------------------------------------------------------------------------------
    // Visuals & Effects
    // --------------------------------------------------------------------------------

    private void HandleFlip()
    {
        bool isWallSliding = wallSlideScript != null && wallSlideScript.IsWallSliding;
        bool isTouchingWall = wallSlideScript != null && wallSlideScript.IsTouchingWall;
        bool isTryingToMove = Mathf.Abs(horizontalInput) > 0.01f;
        
        if (isWallSliding)
        {
            float wallSide = wallSlideScript.WallSide;
            Flip(wallSide == 1); // Face away from the wall (face left if wall is right)
        }
        else if (canControl && isTryingToMove && !(isTouchingWall && IsGrounded))
        {
            if (horizontalInput > 0 && facingLeft) Flip(false); // Face RIGHT
            else if (horizontalInput < 0 && !facingLeft) Flip(true); // Face LEFT
        }
    }

    private void Flip(bool faceLeft)
    {
        // 1. Flip the player sprite
        facingLeft = faceLeft;
        sr.flipX = !faceLeft;

        // 2. REVERTED: Flip the running dust particles using Y-rotation.
        // This is the original logic requested to be restored.
        if (DustParticles != null)
        {
            float rotationY = faceLeft ? 90f : 270f;
            DustParticles.transform.localRotation = Quaternion.Euler(0, rotationY, 0);
        }
    }
    
    /// <summary>
    /// Controls the running dust particles, ensuring they only play when the player is both moving AND grounded.
    /// </summary>
    private void StartStopRunningParticles()
    {
        if (DustParticles == null) return;
        
        // Only run when grounded AND moving
        bool shouldEmitDust = IsGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        
        if (shouldEmitDust && !DustParticles.isPlaying)
        {
            DustParticles.Play();
        }
        else if (!shouldEmitDust && DustParticles.isPlaying)
        {
            DustParticles.Stop();
        }
    }

    /// <summary>
    /// Positions the landing particle system at the player's feet and plays it once.
    /// </summary>
    private void SpawnLandingDust()
    {
        if (LandingDustParticles != null && col != null)
        {
            Vector3 spawnPosition = transform.position;
            // Place particles at the bottom edge of the player's collider
            spawnPosition.y = col.bounds.min.y; 
            
            LandingDustParticles.transform.position = spawnPosition;
            LandingDustParticles.Play();
        }
    }

    // --------------------------------------------------------------------------------
    // Audio Control
    // --------------------------------------------------------------------------------

    private void PlayLandingSound()
    {
        // Ensure falling sound fade-out is cancelled
        if (fallingSource.isPlaying && fallingStopCoroutine != null)
        {
            StopCoroutine(fallingStopCoroutine);
            fallingStopCoroutine = null;
            // Restore volume in case it was fading
            fallingSource.volume = runningVolume; 
            fallingSource.Stop(); 
        }

        if (landingSound != null)
            AudioSource.PlayClipAtPoint(landingSound, transform.position, landingVolume);
    }

    private void HandleRunningAudio()
    {
        if (audioFrozen) return;

        bool isMovingHorizontally = Mathf.Abs(rb.linearVelocity.x) > 0.01f;
        bool isWallSliding = wallSlideScript != null && wallSlideScript.IsWallSliding;

        if (IsGrounded && isMovingHorizontally && !isWallSliding)
        {
            audioCutoffTimer = runningAudioCutoffDelay;
            if (!audioSource.isPlaying && runningSound != null)
            {
                audioSource.clip = runningSound;
                audioSource.volume = runningVolume;
                audioSource.Play();
            }
        }
        else if (audioSource.isPlaying)
        {
            audioCutoffTimer -= Time.deltaTime;
            if (audioCutoffTimer <= 0 || isWallSliding) 
            {
                audioSource.Stop();
            }
        }
    }

    private void ControlFallingSound()
    {
        if (audioFrozen) return;
        if (fallingSound == null || fallingSource == null) return;

        bool isWallSliding = wallSlideScript != null && wallSlideScript.IsWallSliding;
        bool isFallingFast = !IsGrounded && rb.linearVelocity.y < -1f && !isWallSliding;

        if (isFallingFast)
        {
            // Cancel any fading coroutine immediately
            if (fallingStopCoroutine != null)
            {
                StopCoroutine(fallingStopCoroutine);
                fallingStopCoroutine = null;
                fallingSource.volume = runningVolume;
            }

            if (!fallingSource.isPlaying)
            {
                fallingSource.clip = fallingSound;
                fallingSource.volume = runningVolume;
                fallingSource.Play();
            }
        }
        else
        {
            if (fallingSource.isPlaying && fallingStopCoroutine == null)
                fallingStopCoroutine = StartCoroutine(StopFallingSoundWithDelay());
        }
    }

    private IEnumerator StopFallingSoundWithDelay()
    {
        yield return new WaitForSeconds(fallingAudioStopDelay);
        
        // Fade out
        float startVolume = fallingSource.volume;
        float targetVolume = startVolume * 0.25f; // Fade to a quarter volume before stopping
        float fadeStartTime = Time.time;
        float fadeDuration = fallingAudioFadeOutDuration;

        while (Time.time < fadeStartTime + fadeDuration)
        {
            float newVolume = Mathf.Lerp(startVolume, targetVolume, (Time.time - fadeStartTime) / fadeDuration);
            fallingSource.volume = newVolume;
            yield return null;
        }

        fallingSource.Stop();
        fallingSource.volume = runningVolume; // Reset volume for next use
        fallingStopCoroutine = null;
    }
    
    // Public function for external scripts (like PlayerWallSlide) to call
    public void StopAllMovementAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        if (fallingSource != null && fallingSource.isPlaying)
        {
            if (fallingStopCoroutine != null)
            {
                StopCoroutine(fallingStopCoroutine);
                fallingStopCoroutine = null;
                fallingSource.volume = runningVolume; 
            }
            fallingSource.Stop();
        }
    }

    // --- PAUSE/RESUME audio externally ---
    public void SetAudioFrozen(bool frozen)
    {
        audioFrozen = frozen;
        if (frozen)
        {
            if (audioSource.isPlaying) audioSource.Pause();
            if (fallingSource.isPlaying) fallingSource.Pause();
        }
        else
        {
            if (audioSource.clip != null) audioSource.UnPause();
            if (fallingSource.clip != null) fallingSource.UnPause();
        }
    }

    // --------------------------------------------------------------------------------
    // Super Jump Logic & Penalty
    // --------------------------------------------------------------------------------

    public void ActivateSuperJump(float multiplier, int damage, float stunTime)
    {
        isSuperJumpActive = true;
        superJumpMultiplier = multiplier;
        penaltyDamage = damage;
        penaltyStunDuration = stunTime;
        isLandingCharged = false;
    }

    public void DeactivateSuperJump()
    {
        isSuperJumpActive = false;
        superJumpMultiplier = 1f;
    }

    private void ApplyLandingPenaltyCheck()
    {
        if (isLandingCharged && playerHealth != null && playerEffects != null)
        {
            // Apply Damage and Stun if the player fails to land on a ChargingPad
            // Note: These lines are commented out or mocked based on context from previous turns.
            // playerHealth.TakeDamage(penaltyDamage);
            // playerEffects.ApplyFreeze(penaltyStunDuration);
            isLandingCharged = false;
            
            Debug.Log($"Super Jump Penalty: Taking {penaltyDamage} damage and stunned for {penaltyStunDuration}s.");
        }
    }
}