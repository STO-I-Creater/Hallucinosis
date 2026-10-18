
using UnityEngine;
using System.Collections.Generic; // Added back, just in case

/// <summary>
/// A "danger zone" that slows and hurts the player *while they are inside*, 
/// then applies a 5-second lingering poison when they leave.
/// The object MUST have a 2D Collider set to "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AcidHazard : MonoBehaviour
{
    [Header("Acid Effect Settings")]
    [Tooltip("The damage dealt every 1 second.")]
    public int damagePerSecond = 5;

    [Tooltip("The multiplier for movement speed. 0.5 = 50% slower.")]
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f;

    [Tooltip("The duration of the lingering poison after the player leaves.")]
    public float lingeringDuration = 5f;

    [Header("Feedback (Optional)")]
    [Tooltip("A particle effect to play when the player enters.")]
    public ParticleSystem splashEffect;
    [Tooltip("A sound to play when the player enters.")]
    public AudioClip splashSound;
    
    // We no longer use the poisonedObjects list for continuous damage, but keep for clarity.
    // private List<Collider2D> poisonedObjects = new List<Collider2D>(); 

    private void Awake()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check if it's the player
        if (collision.CompareTag("Player"))
        {
            // Find the PlayerEffects script on the player
            PlayerEffects playerEffects = collision.GetComponent<PlayerEffects>();
            
            if (playerEffects != null)
            {
                // 2. Tell the player to START the continuous effect
                playerEffects.StartContinuousAcid(damagePerSecond, slowMultiplier);
            }

            // 3. (Optional) Play feedback effects
            if (splashEffect != null)
            {
                splashEffect.Play();
            }
            if (splashSound != null)
            {
                AudioSource.PlayClipAtPoint(splashSound, transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerEffects playerEffects = collision.GetComponent<PlayerEffects>();
            
            if (playerEffects != null)
            {
                // 4. Tell the player to STOP the continuous effect and start the lingering one
                playerEffects.StopContinuousAcid(damagePerSecond, slowMultiplier, lingeringDuration);
            }
        }
    }
}