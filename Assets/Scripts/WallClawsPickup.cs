using UnityEngine;

/// <summary>
/// This script goes on the "Wall Claws" artifact. When the player
/// touches it, it grants them the 'hasWallClaws' ability,
/// adds the item to their inventory, and disappears.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WallClawsPickup : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("The 'Wall Claws' ItemData asset this represents. This will be added to the inventory.")]
    public ItemData itemData; // NEW: The item to add to the inventory

    [Header("Feedback (Optional)")]
    [Tooltip("A particle effect to play when picked up.")]
    public GameObject pickupEffect;
    [Tooltip("A sound to play when picked up.")]
    public AudioClip pickupSound;

    private void Awake()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it's the player
        if (collision.CompareTag("Player"))
        {
            // Find BOTH scripts on the player
            PlayerWallSlide playerWallSlide = collision.GetComponent<PlayerWallSlide>();
            InventorySystem inventorySystem = collision.GetComponent<InventorySystem>(); // NEW
            
            // NEW: Check for all components
            if (playerWallSlide != null && inventorySystem != null && itemData != null)
            {
                Debug.Log("Player has collected the Wall Claws!");
                
                // --- 1. GRANT THE ABILITY ---
                playerWallSlide.hasWallClaws = true;

                // --- 2. ADD TO INVENTORY (for visuals) ---
                // We call AddItem. We don't need to check if it's successful,
                // because the ability is the important part.
                inventorySystem.AddItem(itemData);

                // --- 3. PLAY FEEDBACK ---
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                }

                // Destroy the artifact
                Destroy(gameObject);
            }
            else
            {
                if(itemData == null)
                {
                    Debug.LogError("WallClawsPickup is missing its ItemData asset!");
                }
            }
        }
    }
}