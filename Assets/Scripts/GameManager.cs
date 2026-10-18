using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A lightweight class to store item data so the GameManager can remember
/// your inventory even when the Player object is destroyed between scenes.
/// </summary>
[System.Serializable]
public class SavedInventoryItem
{
    public string itemName; // The name of the ItemData asset in the Resources/Items folder
    public int quantity;

    public SavedInventoryItem(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
}

/// <summary>
/// The Persistent Singleton that stores all player data across scene transitions.
/// Place this on a GameObject in your first scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Persistence")]
    [Tooltip("Stores health. -1 means it hasn't been set yet.")]
    public int playerStoredHealth = -1;

    [Tooltip("Stores the last checkpoint touched by the player.")]
    public Vector2 savedCheckpoint = Vector2.zero;

    [Header("Inventory Persistence")]
    [Tooltip("The list of items currently held by the player.")]
    public List<SavedInventoryItem> savedInventory = new List<SavedInventoryItem>();

    private void Awake()
    {
        // --- Singleton Pattern ---
        if (Instance == null)
        {
            Instance = this;
            // This line is the secret sauce: it keeps this object alive 
            // even when you load a new scene or a puzzle scene.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another GameManager somehow exists, kill this one.
            Destroy(gameObject);
        }
    }

    // --- Checkpoint Logic ---

    public void SetCheckpoint(Vector2 pos)
    {
        savedCheckpoint = pos;
        Debug.Log("GameManager: Checkpoint updated to " + pos);
    }

    public void ClearCheckpoint()
    {
        savedCheckpoint = Vector2.zero;
        Debug.Log("GameManager: Checkpoint cleared.");
    }

    // --- Data Management ---

    /// <summary>
    /// Resets all session data. Use this when the player returns to the Main Menu
    /// or starts a completely New Game.
    /// </summary>
    public void ResetAllSessionData()
    {
        playerStoredHealth = -1;
        savedCheckpoint = Vector2.zero;
        savedInventory.Clear();
        Debug.Log("GameManager: All session data has been wiped.");
    }
}