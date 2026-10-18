using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Main Components")]
    public GameObject inventoryPanel;
    public Transform slotGridParent;
    public GameObject slotPrefab;

    [Header("Item Details Panel")]
    public GameObject detailsPanel;
    public TextMeshProUGUI detailItemName;
    public TextMeshProUGUI detailItemDescription;
    public Image detailItemIcon;
    public Button useButton;

    [Header("Script References")]
    public InventorySystem inventorySystem; 
    public PlayerMovement playerMovement;
    public PlayerWallSlide playerWallSlide;

    [Header("Map Settings")]
    [Tooltip("Drag your MapOverlay object here.")]
    public MapUI mapUIScript; 

    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private InventorySlot currentlySelectedSlot;

    void Start()
    {
        if (inventorySystem == null) inventorySystem = InventorySystem.Instance; 

        if (inventorySystem == null)
        {
            Debug.LogError("InventoryUI: InventorySystem is missing!");
            return;
        }
        
        CreateSlotPrefabs(); 
        inventorySystem.OnInventoryChanged.AddListener(UpdateAllSlots);
        
        if (useButton != null) useButton.onClick.AddListener(OnUseButtonPressed);

        inventoryPanel.SetActive(false);
        detailsPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle Inventory with 'I' (For testing on PC)
        if (Input.GetKeyDown(KeyCode.I)) 
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (isActive)
        {
            Time.timeScale = 0f; 
            if (playerMovement) playerMovement.enabled = false;
            if (playerWallSlide) playerWallSlide.enabled = false;
            UpdateAllSlots(); 
        }
        else
        {
            Time.timeScale = 1f; 
            if (playerMovement) playerMovement.enabled = true;
            if (playerWallSlide) playerWallSlide.enabled = true;
            detailsPanel.SetActive(false);
        }
    }

    void CreateSlotPrefabs()
    {
        if (inventorySystem == null) return;

        foreach (Transform child in slotGridParent) Destroy(child.gameObject);
        slotUIs.Clear();

        for (int i = 0; i < inventorySystem.slots.Count; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotGridParent);
            InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();
            
            if (slotUI != null)
            {
                slotUI.Init(this, inventorySystem.slots[i]);
                slotUIs.Add(slotUI);
            }
        }
    }

    public void UpdateAllSlots() 
    {
        if(inventorySystem == null) return;

        if(slotUIs.Count != inventorySystem.slots.Count) CreateSlotPrefabs();

        for (int i = 0; i < slotUIs.Count; i++)
        {
            slotUIs[i].Init(this, inventorySystem.slots[i]); 
            slotUIs[i].UpdateSlotDisplay();
        }
    }

    /// <summary>
    /// Handles clicking a slot. Includes the Mobile Map Shortcut.
    /// </summary>
    public void OnSlotClicked(InventorySlot slot)
    {
        if (slot.item == null)
        {
            detailsPanel.SetActive(false);
            currentlySelectedSlot = null;
            return;
        }

        // --- MOBILE MAP SHORTCUT ---
        // If the player taps a map, open the big map immediately and close inventory
        if (slot.item.itemType == ItemData.ItemType.Map)
        {
            if (mapUIScript != null)
            {
                mapUIScript.OpenMap();
                ToggleInventory(); // Closes bag so player can see the map clearly
                return; 
            }
        }

        // --- NORMAL ITEM LOGIC ---
        currentlySelectedSlot = slot;
        detailsPanel.SetActive(true);
        
        if (detailItemName) detailItemName.text = slot.item.itemName;
        if (detailItemDescription) detailItemDescription.text = slot.item.description;
        if (detailItemIcon)
        {
            detailItemIcon.sprite = slot.item.icon;
            detailItemIcon.enabled = (slot.item.icon != null);
        }

        // Show "Use" button only for Consumables or Tools
        bool isUsable = slot.item.itemType == ItemData.ItemType.Consumable || 
                        slot.item.itemType == ItemData.ItemType.Tool;

        if (useButton) useButton.gameObject.SetActive(isUsable);
    }

    void OnUseButtonPressed()
    {
        if (currentlySelectedSlot != null)
        {
            inventorySystem.UseItem(currentlySelectedSlot);

            if (currentlySelectedSlot.item == null)
            {
                detailsPanel.SetActive(false);
            }
            else
            {
                OnSlotClicked(currentlySelectedSlot);
            }
        }
    }
}