using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SilentItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemData itemData;

    [Header("Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    [Header("UI Notification")]
    public GameObject notificationPanel; // Assign your panel
    public float displayTime = 1.5f;     // How long the panel stays visible

    private void Awake()
    {
        // Ensure collider is a trigger
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
            col.isTrigger = true;

        // Ensure panel is hidden at start
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only respond to the player
        if (!collision.CompareTag("Player"))
            return;

        // Show notification immediately on collision
        if (notificationPanel != null)
            StartCoroutine(ShowNotification());

        // Try to add item to inventory
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory != null && itemData != null)
        {
            bool added = inventory.AddItem(itemData, false);

            // Play pickup sound if added successfully
            if (added && pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
        }

        // Destroy the pickup object
        Destroy(gameObject);
    }

    private IEnumerator ShowNotification()
    {
        notificationPanel.SetActive(true);            // Activate panel
        yield return new WaitForSeconds(displayTime); // Wait
        notificationPanel.SetActive(false);           // Deactivate panel
    }
}
