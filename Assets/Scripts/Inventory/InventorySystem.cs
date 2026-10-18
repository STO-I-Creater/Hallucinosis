using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manages the player's items and coordinates with GameManager for persistence.
/// Place this on the Player GameObject.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Inventory Settings")]
    public List<InventorySlot> slots = new List<InventorySlot>();
    public int inventorySize = 24;

    [Header("Events")]
    public UnityEvent OnInventoryChanged;
    public UnityEvent OnMapUsed;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        // --- Singleton Pattern ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerHealth = GetComponent<PlayerHealth>();

        // Load items from the GameManager "vault"
        LoadInventoryFromManager();
    }

    // ------------------------------------------------------------------
    // PERSISTENCE: Loading & Saving to GameManager
    // ------------------------------------------------------------------

    private void LoadInventoryFromManager()
    {
        slots.Clear();

        // If GameManager is empty (New Game), create blank slots
        if (GameManager.Instance == null || GameManager.Instance.savedInventory.Count == 0)
        {
            for (int i = 0; i < inventorySize; i++)
                slots.Add(new InventorySlot(null, 0));
            return;
        }

        // Reconstruct the inventory using names stored in the GameManager
        for (int i = 0; i < inventorySize; i++)
        {
            if (i < GameManager.Instance.savedInventory.Count)
            {
                SavedInventoryItem saved = GameManager.Instance.savedInventory[i];
                
                if (!string.IsNullOrEmpty(saved.itemName))
                {
                    // Search in Assets/Resources/Items/ for the matching file name
                    ItemData loadedItem = Resources.Load<ItemData>("Items/" + saved.itemName);
                    slots.Add(new InventorySlot(loadedItem, saved.quantity));
                }
                else
                {
                    slots.Add(new InventorySlot(null, 0));
                }
            }
            else
            {
                slots.Add(new InventorySlot(null, 0));
            }
        }
    }

    /// <summary>
    /// Updates the GameManager's list so data survives scene transitions.
    /// </summary>
    public void SaveToManager()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.savedInventory.Clear();
        foreach (InventorySlot slot in slots)
        {
            if (slot.item != null)
            {
                GameManager.Instance.savedInventory.Add(new SavedInventoryItem(slot.item.itemName, slot.quantity));
            }
            else
            {
                GameManager.Instance.savedInventory.Add(new SavedInventoryItem("", 0));
            }
        }
    }

    // ------------------------------------------------------------------
    // CORE METHODS: Adding and Using Items
    // ------------------------------------------------------------------

    public bool AddItem(ItemData itemToAdd, bool showNotification = true)
    {
        bool success = false;

        // 1. Try stacking (matches item name)
        if (itemToAdd.isStackable)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item != null && slot.item.itemName == itemToAdd.itemName)
                {
                    slot.quantity++;
                    success = true;
                    break;
                }
            }
        }

        // 2. Find first empty slot
        if (!success)
        {
            foreach (InventorySlot slot in slots)
            {
                if (slot.item == null)
                {
                    slot.item = itemToAdd;
                    slot.quantity = 1;
                    success = true;
                    break;
                }
            }
        }

        if (success)
        {
            SaveToManager(); 
            OnInventoryChanged?.Invoke();
        }
        return success;
    }

    public void UseItem(InventorySlot slot)
    {
        if (slot == null || slot.item == null) return;

        bool wasConsumed = false;

        switch (slot.item.itemType)
        {
            case ItemData.ItemType.Consumable:
                if (playerHealth != null)
                {
                    playerHealth.Heal(slot.item.healAmount);
                    
                    if (slot.item.consumptionSound != null)
                        AudioSource.PlayClipAtPoint(slot.item.consumptionSound, transform.position, slot.item.consumptionVolume);

                    slot.quantity--;
                    if (slot.quantity <= 0) slot.Clear();
                    wasConsumed = true;
                }
                break;

            case ItemData.ItemType.Map:
                // Trigger the map overlay (MapUI listens to this event)
                OnMapUsed?.Invoke();
                // Map is NOT consumed; slot.quantity is NOT reduced.
                break;

            case ItemData.ItemType.Tool:
                Debug.Log("Using tool: " + slot.item.itemName);
                // Tools generally aren't consumed unless you add specific logic
                break;
        }

        SaveToManager(); 
        OnInventoryChanged?.Invoke();
    }
}