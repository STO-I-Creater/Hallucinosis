using System.Collections;
using UnityEngine;

/// <summary>
/// Controls a cycling hazard like a Tesla arc.
/// Manages the ON/OFF state. The actual damage is handled by TeslaArc.cs.
/// </summary>
public class TeslaHazard : MonoBehaviour
{
    [Header("Hazard Timing")]
    [Tooltip("How long the hazard is ON (dangerous).")]
    public float onDuration = 2f; 
    [Tooltip("How long the hazard is OFF (safe/idle).")]
    public float offDuration = 3f;
    [Tooltip("Time spent in the intermediate CHARGE UP animation/state.")]
    public float chargeUpTime = 1.0f; // Duration of the warning phase

    [Header("Hazard Effects")]
    [Tooltip("The damage dealt to the player on hit.")]
    public int damage = 25;

    [Header("Object References")]
    [Tooltip("Drag the visual part of the arc (Sprite, LineRenderer) here.")]
    public GameObject arcVisual;
    [Tooltip("Drag the Collider2D for the arc's trigger area here.")]
    public Collider2D arcCollider;
    
    [Header("Audio")]
    [Tooltip("The sound clip to loop while the arc is ON.")]
    public AudioClip arcLoopSound; 
    [Tooltip("The sound clip to play during the ChargeUp animation.")]
    public AudioClip chargeUpSound; // Sound for the warning phase

    // --- Private State ---
    private float arcEndTime = 0f;
    private AudioSource audioSource;
    private Animator arcAnimator; 

    void Awake()
    {
        // Get or add the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            // Set looping to false initially, as we control the loops manually
            audioSource.loop = false; 
        }

        // Enable 3D Spatial Sound (crucial for distance fading of the loop sound)
        audioSource.spatialBlend = 1.0f;
        
        // Get the Animator from the visual object
        if (arcVisual != null)
        {
            arcAnimator = arcVisual.GetComponent<Animator>();
        }
    }

    void Start()
    {
        if (arcVisual == null || arcCollider == null)
        {
            Debug.LogError("TeslaHazard is missing references! Assign Arc Visual and Arc Collider.");
            return;
        }
        
        // Start the ON/OFF cycle
        StartCoroutine(HazardCycle());
    }

    private IEnumerator HazardCycle()
    {
        while (true)
        {
            // --- STATE 1: OFF/IDLE ---
            arcCollider.enabled = false;
            if (arcAnimator != null) 
            {
                arcAnimator.SetBool("IsShocking", false);
                arcAnimator.SetBool("IsCharging", false); 
            }
            audioSource.Stop(); // Ensure any sound is stopped

            yield return new WaitForSecondsRealtime(offDuration);
            
            // --- STATE 2: CHARGING/WARNING ---
            if (arcAnimator != null) arcAnimator.SetBool("IsCharging", true);
            
            // FIX: Play the sound on the main AudioSource so we can stop it
            if (chargeUpSound != null)
            {
                audioSource.clip = chargeUpSound;
                audioSource.loop = false; // Ensure it plays once
                audioSource.Play();
            }

            // Wait for charge up time
            yield return new WaitForSecondsRealtime(chargeUpTime);

            // --- FIX: Stop the sound immediately before the shock state begins ---
            audioSource.Stop();
            
            // --- STATE 3: SHOCKING/DANGEROUS ---
            // 1. Turn on the hitbox and set animator to SHOCKING state
            arcCollider.enabled = true;
            if (arcAnimator != null) 
            {
                arcAnimator.SetBool("IsCharging", false);
                arcAnimator.SetBool("IsShocking", true);
            }
            
            // 2. Play the looping arc sound
            if (arcLoopSound != null)
            {
                audioSource.clip = arcLoopSound;
                audioSource.loop = true; // Set loop back to true for the main sound
                audioSource.Play();
            }

            arcEndTime = Time.time + onDuration;
            yield return new WaitForSecondsRealtime(onDuration);
        }
    }

    /// <summary>
    /// Called by TeslaArc when the player is zapped.
    /// This calculates the remaining time and applies all effects.
    /// </summary>
    public void ApplyDamageAndFreeze(PlayerHealth health, PlayerEffects effects)
    {
        // 1. Calculate the remaining time in the "ON" cycle
        float remainingFreezeTime = arcEndTime - Time.time;
        if (remainingFreezeTime < 0) remainingFreezeTime = 0; 

        // 2. Apply Damage
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // 3. Apply Freeze with the *calculated* duration
        if (effects != null)
        {
            effects.ApplyFreeze(remainingFreezeTime);
        }
    }
}