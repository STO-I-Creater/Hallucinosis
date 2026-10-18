using UnityEngine;

/// <summary>
/// This pad grants the player a temporary super-jump and sets a flag
/// to apply a penalty when the player lands.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ChargingPad : MonoBehaviour
{
    [Header("Charge Settings")]
    [Tooltip("The multiplier for the player's base jump force (e.g., 2.0 for double height).")]
    public float jumpMultiplier = 2.0f;

    [Header("Landing Penalty")]
    [Tooltip("Damage dealt to the player upon landing after a super-jump.")]
    public int landingDamage = 10;
    [Tooltip("Duration the player is frozen upon landing.")]
    public float stunDuration = 0.5f;

    [Header("Visuals")]
    [Tooltip("Optional: Particle system to play when the pad is activated.")]
    public ParticleSystem activationParticles;

    // No Audio fields remain here.

    private bool playerIsOnPad = false; // Tracks if player is currently standing here
    private PlayerMovement playerMovementRef;

    void Awake()
    {
        // Must be a trigger to grant the buff without blocking the player
        GetComponent<Collider2D>().isTrigger = true;
        if(activationParticles != null) activationParticles.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerMovementRef = collision.GetComponent<PlayerMovement>();
            
            if (playerMovementRef != null)
            {
                // Instant and Permanent Activation of the Super Jump state in PlayerMovement
                playerMovementRef.ActivateSuperJump(
                    jumpMultiplier, 
                    landingDamage, 
                    stunDuration
                );

                playerIsOnPad = true;
                Debug.Log("Charging Pad Activated. Jump granted.");

                if (activationParticles != null) activationParticles.Play();
                
                // Audio is now handled by PlayerMovement.HandleRegularJump()
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerMovementRef != null && playerIsOnPad)
            {
                // Deactivate ONLY if the player leaves and hasn't jumped yet.
                playerMovementRef.DeactivateSuperJump(); 
            }
            playerIsOnPad = false;
        }
    }
}