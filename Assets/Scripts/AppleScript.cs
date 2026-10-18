using UnityEngine;

/// <summary>
/// A simple script for any object that should heal the player on contact.
/// Make sure the player has the "Player" tag!
/// </summary>
public class AppleScript : MonoBehaviour
{
    [Tooltip("The amount of health to restore.")]
    public int healAmount = 25; // This is public and defaults to 25

    /// <summary>
    /// We use OnTriggerEnter2D so the player can pass through the item.
    /// Make sure "Is Trigger" is checked on this object's collider!
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object we collided with has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            // Try to get the PlayerHealth script from the object
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            // If the script was found, tell it to heal
            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);

                // Destroy the apple after it's picked up
                Destroy(gameObject);
            }
        }
    }
}
