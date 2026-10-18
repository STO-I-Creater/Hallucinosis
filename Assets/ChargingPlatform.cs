using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

/// <summary>
/// Autonomous charging platform that cycles between Safe (Charging) and Dangerous (ZapReady) states.
/// Deals instant damage/freeze if the player touches it during the ZapReady state.
/// </summary>
public class ChargingPlatform : MonoBehaviour
{
    [Header("Cycle Timing")]
    [Tooltip("Time the platform takes to fully charge (SAFE).")]
    public float chargeUpTime = 2.0f;
    [Tooltip("Time the platform remains dangerous/red (DANGEROUS).")]
    public float zapReadyTime = 0.5f; 

    [Header("Hazard Effects")]
    [Tooltip("Damage dealt when the platform fully charges.")]
    public int damage = 10;
    [Tooltip("How long the player is frozen on shock.")]
    public float freezeDuration = 0.5f;

    [Header("Evasion Buffer")]
    [Tooltip("Time the player is considered 'touching' after brief frame-skips (0.05 is safe).")]
    public float antiEvasionTime = 0.05f; 

    [Header("Visual Feedback")]
    [Tooltip("Drag the Slider component of the charge bar UI here (must be World Space).")]
    public Slider chargeSlider; 
    [Tooltip("Drag the Animator component of the platform/indicator here.")]
    public Animator animator; 
    [Tooltip("Sound played continuously while hazard is charging.")]
    public AudioClip chargingSound; 
    
    // --- Private State ---
    // Make sure this constant EXACTLY matches the Boolean parameter name in the Animator!
    private const string IS_SHOCKING = "IsShocking"; 
    
    private bool isZapReady = false; 
    private SpriteRenderer sr;
    private AudioSource audioSource;
    private Collider2D platformCollider;
    
    // Tracks if the player is logically touching (using the buffer timer)
    private bool isPlayerLogicallyTouching = false; 
    private float evasionBufferTimer = 0f; 
    
    // References to the player's scripts (used on shock)
    private PlayerHealth playerHealthRef;
    private PlayerEffects playerEffectsRef;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>(); 
        
        if (platformCollider.isTrigger)
        {
            Debug.LogError("ChargingPlatform collider must NOT be a trigger!");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
        }

        // Initialize slider state (Ensuring it starts visible and empty)
        if (chargeSlider != null)
        {
            chargeSlider.gameObject.SetActive(true); 
            chargeSlider.minValue = 0f;
            chargeSlider.maxValue = 1f;
            chargeSlider.value = 0f;
        }
        
        // Initialize Animator State (Idle)
        if (animator != null)
        {
             animator.SetBool(IS_SHOCKING, false);
        }
    }

    void Start()
    {
        // Start the continuous cycle immediately upon loading
        StartCoroutine(HazardCycle());
    }
    
    /// <summary>
    /// The main autonomous loop that controls the platform's state.
    /// </summary>
    private IEnumerator HazardCycle()
    {
        while(true)
        {
            // --- STATE 1: CHARGING/SAFE ---
            isZapReady = false;
            if (animator != null) animator.SetBool(IS_SHOCKING, false); // Idle Animation ON
            
            if (chargingSound != null)
            {
                audioSource.clip = chargingSound;
                audioSource.Play();
            }
            if (chargeSlider != null) 
            {
                 chargeSlider.gameObject.SetActive(true); 
                 chargeSlider.value = 0f; // Reset to empty
            }

            float timer = 0;
            while(timer < chargeUpTime)
            {
                timer += Time.deltaTime;
                
                // VISUAL CONTROL: Update Slider Value (proportional to time passed)
                float chargeRatio = timer / chargeUpTime;
                if (chargeSlider != null) chargeSlider.value = chargeRatio;
                
                yield return null;
            }
            
            // --- STATE 2: ZAP READY/DANGEROUS ---
            isZapReady = true;
            if (chargeSlider != null) chargeSlider.value = 1f; // Ensure it's fully charged
            if (animator != null) animator.SetBool(IS_SHOCKING, true); // Shocking Animation ON
            
            if (audioSource.isPlaying) audioSource.Stop(); // Stop charging sound
            
            // Continuous check loop for the duration
            float zapTimer = 0;
            while (zapTimer < zapReadyTime)
            {
                zapTimer += Time.deltaTime;
                
                // CRITICAL CHECK: If player is logically touching (using the buffer)
                if (isPlayerLogicallyTouching)
                {
                    ApplyShock(playerHealthRef.gameObject);
                    
                    // After shock, immediately reset bar to empty
                    if (chargeSlider != null) chargeSlider.value = 0f; 
                    
                    yield break; // Restart the cycle
                }
                
                yield return null;
            }
            
            // If zap time runs out without shock, the cycle restarts.
        }
    }
    
    /// <summary>
    /// Finds the length of a clip by name (essential for accurate timing).
    /// </summary>
    private float GetClipLength(Animator animator, string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0f;
        
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0f;
    }

    void Update()
    {
        // Update the logical state based on the buffer timer
        if (evasionBufferTimer > 0)
        {
            evasionBufferTimer -= Time.deltaTime;
            isPlayerLogicallyTouching = true;
        }
        else
        {
            isPlayerLogicallyTouching = false;
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        // We only care about the player and when they are standing on top.
        if (collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y < -0.9f)
        {
            // Reset the timer: This keeps the player logically 'on' the platform
            evasionBufferTimer = antiEvasionTime; 
            
            // Grab references if we don't have them yet
            if (playerHealthRef == null)
            {
                 playerHealthRef = collision.gameObject.GetComponent<PlayerHealth>();
                 playerEffectsRef = collision.gameObject.GetComponent<PlayerEffects>();
            }
        }
    }

    /// <summary>
    /// Applies the damage and status effect instantly and forces the cycle to reset.
    /// </summary>
    private void ApplyShock(GameObject player)
    {
        // 1. Apply Damage
        if (playerHealthRef != null)
        {
            playerHealthRef.TakeDamage(damage);
        }
        
        // 2. Apply Freeze
        if (playerEffectsRef != null)
        {
            playerEffectsRef.ApplyFreeze(freezeDuration);
        }
        
        // Reset buffer timer and force reset the cycle
        evasionBufferTimer = 0f;
        StopCoroutine(HazardCycle());
        StartCoroutine(HazardCycle());
    }
}
