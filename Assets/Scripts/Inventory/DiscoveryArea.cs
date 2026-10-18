using UnityEngine;

/// <summary>
/// Triggers an event (like receiving a map item) when the player enters.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DiscoveryZone : MonoBehaviour
{
    [Header("Map Item")]
    [Tooltip("The ItemData object for the Map to be added to the inventory.")]
    public ItemData mapItemData; 
    
    [Header("References")]
    [Tooltip("Set this to TRUE if the item should only be picked up once.")]
    public bool destroyOnPickup = true;
    
    // Private flag to ensure the pickup only happens once
    private bool hasCollected = false; 
    
    void Awake()
    {
        // Ensure the collider is set as a trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the Player entered AND if we haven't collected already
        if (collision.CompareTag("Player") && !hasCollected)
        {
            // Mark as collected immediately to prevent re-triggering
            hasCollected = true;

            // Attempt to add item to InventorySystem if it exists
            InventorySystem inventory = InventorySystem.Instance;
            if (inventory != null && mapItemData != null)
            {
                bool added = inventory.AddItem(mapItemData);
                if (added)
                {
                    Debug.Log("Map item '" + mapItemData.itemName + "' added to inventory.");
                }
                else
                {
                    Debug.LogWarning("Inventory full or item could not be added: " + mapItemData.itemName);
                }
            }
            else
            {
                // Optional: Log that inventory was missing but still trigger display
                Debug.Log("No InventorySystem found; skipping item addition.");
            }

            // Call the visual feedback regardless of InventorySystem
            MapDisplay displayManager = FindObjectOfType<MapDisplay>();
            if (displayManager != null)
            {
                displayManager.ShowMapIcon();
            }

            // --- Cleanup ---
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                // If not destroying, disable the collider
                GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}
