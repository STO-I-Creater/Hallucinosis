using UnityEngine;

/// <summary>
/// Defines an area that adds a unique item to the player's inventory upon entry.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class UniqueItemPickup : MonoBehaviour
{
    [Tooltip("The ItemData this pickup represents.")]
    public ItemData itemData;

    [Header("Audio")]
    public AudioClip pickupSound; 
    [Range(0f, 1f)]
    public float pickupVolume = 1.0f; 

    private void Awake()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object is the Player
        if (collision.CompareTag("Player"))
        {
            // IMPORTANT: Get the InventorySystem instance (assuming it's a singleton)
            InventorySystem inventory = InventorySystem.Instance; 
            
            if (inventory != null)
            {
                // Try to add the item. 
                if (inventory.AddItem(itemData))
                {
                    // The InventorySystem handles the notification and UI updates.
                    
                    // --- Play Pickup Sound ---
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
                    }

                    // Destroy the pickup object
                    Destroy(gameObject);
                }
            }
        }
    }
}