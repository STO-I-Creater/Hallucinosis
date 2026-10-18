using UnityEngine;

/// <summary>
/// This is a plain data class. It does NOT inherit from MonoBehaviour.
/// It acts as the "Pocket" that holds an item and its quantity.
/// </summary>
[System.Serializable]
public class InventorySlot
{
    [Tooltip("The actual item data (Icon, Name, etc.)")]
    public ItemData item;

    [Tooltip("How many of this item are in this slot.")]
    public int quantity;

    // This is the "Constructor" - it lets us create a slot and fill it in one go
    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    /// <summary>
    /// Empties the slot completely.
    /// </summary>
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}