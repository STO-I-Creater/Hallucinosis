using System.Collections;
using UnityEngine;

/// <summary>
/// A section of floor that deals continuous damage and applies freeze
/// while the player is inside the trigger zone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ElectrifiedZone : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("The damage dealt every 1 second.")]
    public int damagePerSecond = 10;
    [Tooltip("How long the player is frozen on every damage tick.")]
    public float freezeDuration = 0.1f; // Short stun to disrupt movement

    // --- Private State ---
    private Coroutine continuousDamageCoroutine;

    void Awake()
    {
        // This is necessary for OnTriggerEnter/Exit to fire
        // This MUST be checked ON in the Inspector for this script to work.
        GetComponent<Collider2D>().isTrigger = true; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && continuousDamageCoroutine == null)
        {
            // Start the continuous damage loop
            PlayerEffects playerEffects = collision.GetComponent<PlayerEffects>();
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            
            if (playerEffects != null && playerHealth != null)
            {
                continuousDamageCoroutine = StartCoroutine(DamageOverTime(playerHealth, playerEffects));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && continuousDamageCoroutine != null)
        {
            // Stop the damage loop instantly when the player leaves
            StopCoroutine(continuousDamageCoroutine);
            continuousDamageCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine that runs damage and freeze every 1 second while the player is inside.
    /// </summary>
    private IEnumerator DamageOverTime(PlayerHealth health, PlayerEffects effects)
    {
        while (true)
        {
            // 1. Apply Damage
            health.TakeDamage(damagePerSecond);
            
            // 2. Apply Freeze (brief stun)
            effects.ApplyFreeze(freezeDuration);
            
            // Wait for 1 second before the next damage tick
            yield return new WaitForSeconds(1f);
        }
    }
}
