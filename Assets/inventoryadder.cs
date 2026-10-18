using UnityEngine;

public class InventoryAdder : MonoBehaviour
{
    [System.Serializable]
    public class ItemAmount
    {
        public ItemData item;
        public int amount = 1;
    }

    [Header("Items to Give")]
    public ItemAmount[] itemsToAdd;

    [Header("Trigger Settings")]
    public bool oneTimeOnly = true; // Only give once
    private bool hasGiven = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasGiven && oneTimeOnly) return;

        if (other.CompareTag("Player"))
        {
            foreach (var entry in itemsToAdd)
            {
                for (int i = 0; i < entry.amount; i++)
                {
                    InventorySystem.Instance.AddItem(entry.item);
                }
            }

            hasGiven = true;
        }
    }
}
