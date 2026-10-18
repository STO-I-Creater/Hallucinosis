using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject 
{
    public string itemName;

    [TextArea(3, 10)] 
    public string description;

    public Sprite icon;

    public enum ItemType
    {
        Consumable, 
        Weapon,
        Tool,
        Map,     // <-- NEW ITEM TYPE ADDED
        Default
    }

    public ItemType itemType;

    [Tooltip("Check this if the item can be stacked (e.g., 99 apples).")]
    public bool isStackable;

    [Header("Consumable")]
    [Tooltip("Only used if itemType is Consumable.")]
    public int healAmount = 0;

    // --- AUDIO FIELDS ---
    [Tooltip("Sound clip played when this item is consumed (eaten).")]
    public AudioClip consumptionSound;
    [Tooltip("Volume of the consumption sound (0.0 to 1.0).")]
    [Range(0f, 1f)]
    public float consumptionVolume = 1.0f;
}
