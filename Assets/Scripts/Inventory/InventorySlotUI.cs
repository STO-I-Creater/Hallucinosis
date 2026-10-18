
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro

public class InventorySlotUI : MonoBehaviour
{
    [Tooltip("The Image component for the item's icon.")]
    public Image itemIcon;
    [Tooltip("The Text component for the item's quantity.")]
    public TextMeshProUGUI quantityText;

    private Button button;
    private InventoryUI inventoryUI;
    private InventorySlot mySlot;

    /// <summary>
    /// Called by InventoryUI to set up this slot.
    /// </summary>
    public void Init(InventoryUI ui, InventorySlot slot)
    {
        inventoryUI = ui;
        mySlot = slot;

        button = GetComponent<Button>();
        button.onClick.AddListener(OnSlotClicked);

        UpdateSlotDisplay();
    }

    /// <summary>
    /// Updates the icon and quantity text based on the slot's data.
    /// </summary>
    public void UpdateSlotDisplay()
    {
        if (mySlot.item != null)
        {
            // Item exists, show icon and quantity
            itemIcon.sprite = mySlot.item.icon;
            itemIcon.enabled = true;

            if (mySlot.item.isStackable && mySlot.quantity > 1)
            {
                quantityText.text = mySlot.quantity.ToString();
            }
            else
            {
                quantityText.text = "";
            }
        }
        else
        {
            // No item, hide icon and text
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            quantityText.text = "";
        }
    }

    /// <summary>
    /// Called when this slot's button is clicked.
    /// </summary>
    void OnSlotClicked()
    {
        if (inventoryUI != null)
        {
            inventoryUI.OnSlotClicked(mySlot);
        }
    }
}
