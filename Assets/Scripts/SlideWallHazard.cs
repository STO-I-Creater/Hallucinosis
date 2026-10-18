using System.Collections;
using UnityEngine;

/// <summary>
/// A "Slide Hazard Wall" that damages and freezes the player *after* a short charge-up.
/// This rewards fast wall-jumps but punishes slow slides.
/// Place on a solid wall (Is Trigger is OFF) with the "Wall" tag.
/// </summary>
public class SlideWallHazard : MonoBehaviour
{
    [Header("Hazard Effects")]
    [Tooltip("The damage dealt to the player on hit.")]
    public int damage = 10;
    public float freezeDuration = 0.25f;

    [Header("Buildup Mechanic")]
    public float chargeUpTime = 0.5f;

    [Header("Feedback (Optional)")]
    public ParticleSystem warningParticles;
    public AudioClip chargeUpSound;
    public AudioClip shockSound;

    // --- Private State ---
    private PlayerHealth playerHealth;
    private PlayerEffects playerEffects;
    private AudioSource warningAudioSource;
    private Animator anim; // <-- NEW REFERENCE
    private Coroutine chargeUpCoroutine;
    private bool isPlayerOnWall = false; // Tracks if the player is currently touching

    void Awake()
    {
        // --- Setup for the looping audio ---
        warningAudioSource = GetComponent<AudioSource>();
        if (warningAudioSource == null)
        {
            warningAudioSource = gameObject.AddComponent<AudioSource>();
            warningAudioSource.playOnAwake = false;
            warningAudioSource.loop = true;
        }

        // --- NEW: Get Animator Reference ---
        anim = GetComponent<Animator>(); 
        if (anim == null)
        {
            Debug.LogWarning("SlideWallHazard is missing an Animator component!");
        }
        
        if (warningParticles != null)
        {
            warningParticles.Stop(); // Ensure particles are off
        }
    }

    /// <summary>
    /// Called when the player first makes physical contact.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Only start the charge-up if the player is touching it
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player touched electric wall.");
        isPlayerOnWall = true;

        // 1. Get the player's scripts
        playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        playerEffects = collision.gameObject.GetComponent<PlayerEffects>();

        // 2. Start the charge-up coroutine
        if (chargeUpCoroutine == null)
        {
            chargeUpCoroutine = StartCoroutine(ChargeUp());
        }
    }

    /// <summary>
    /// Called when the player leaves contact with the wall.
    /// </summary>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player left electric wall.");
        isPlayerOnWall = false;

        // Player jumped away in time, stop the charge-up
        if (chargeUpCoroutine != null)
        {
            StopCoroutine(chargeUpCoroutine);
            chargeUpCoroutine = null;
        }

        // Stop the feedback
        StopFeedback();

        // Clear player references
        playerHealth = null;
        playerEffects = null;
    }

    /// <summary>
    /// Coroutine that manages the charge, shock, and reset cycle.
    /// </summary>
    private IEnumerator ChargeUp()
    {
        // Loop this coroutine as long as the player is on the wall
        while (isPlayerOnWall)
        {
            // 1. Start Feedback (Start animation, particles, sound)
            StartFeedback();

            // 2. Wait for the charge-up time
            yield return new WaitForSeconds(chargeUpTime);

            // 3. Check if player is *still* on the wall (and not dead)
            if (isPlayerOnWall && playerHealth != null && playerHealth.currentHealth > 0)
            {
                Debug.Log("Player stayed too long! ZAP!");

                // 4. Play shock sound
                if (shockSound != null)
                {
                    AudioSource.PlayClipAtPoint(shockSound, transform.position);
                }

                // 5. Apply damage and freeze
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
                if (playerEffects != null)
                {
                    // This will also freeze the player, stopping them from moving
                    playerEffects.ApplyFreeze(freezeDuration);
                }

                // 6. Stop warning feedback (The shock is the feedback now)
                StopFeedback(); 

                // 7. Wait for the freeze to end before starting the next charge-up
                // This gives the player a tiny moment to escape.
                yield return new WaitForSeconds(freezeDuration);
            }
            else
            {
                // Player died or left the wall during the charge-up
                isPlayerOnWall = false; // Ensure loop terminates
            }
        }

        // Loop has ended (player left the wall)
        StopFeedback();
        chargeUpCoroutine = null;
    }

    private void StartFeedback()
    {
        // --- NEW: Animator State ON ---
        if (anim != null)
        {
            anim.SetBool("IsActive", true); // Activates the shock/warning animation
        }
        
        if (warningParticles != null && !warningParticles.isPlaying)
        {
            warningParticles.Play();
        }
        if (chargeUpSound != null && warningAudioSource != null && !warningAudioSource.isPlaying)
        {
            warningAudioSource.clip = chargeUpSound;
            warningAudioSource.Play();
        }
    }

    private void StopFeedback()
    {
        // --- NEW: Animator State OFF ---
        if (anim != null)
        {
            anim.SetBool("IsActive", false); // Returns to the idle animation
        }
        
        if (warningParticles != null)
        {
            warningParticles.Stop();
        }
        if (warningAudioSource != null)
        {
            warningAudioSource.Stop();
        }
    }
}