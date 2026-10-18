using UnityEngine;

/// <summary>
/// A hazard that damages and freezes the player on contact.
/// This should be placed on a solid wall (Is Trigger is OFF).
/// </summary>
public class ElectricWallHazard : MonoBehaviour
{
    [Header("Hazard Effects")]
    [Tooltip("The damage dealt to the player on hit.")]
    public int damage = 10;
    [Tooltip("How long the player is frozen in place.")]
    public float freezeDuration = 0.25f;

    /// <summary>
    /// Called by Unity when another collider *first* makes contact with this one.
    /// We use this so the player only gets zapped once per touch.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object we collided with has the "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player hit an electric wall!");

            // Get the player's scripts
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            PlayerEffects playerEffects = collision.gameObject.GetComponent<PlayerEffects>();

            // 1. Apply Damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // 2. Apply Freeze
            if (playerEffects != null)
            {
                playerEffects.ApplyFreeze(freezeDuration);
            }
        }
    }
}
