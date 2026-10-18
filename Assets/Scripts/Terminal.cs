using UnityEngine;
using UnityEngine.UI; // We need this for the Button

/// <summary>
/// This script manages a "dialogue" interaction. Place it on your
/// Terminal object in the scene.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Terminal : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The first panel to show when dialogue starts.")]
    public GameObject firstPanel;
    [Tooltip("The 'Press E to interact' prompt UI object.")]
    public GameObject interactPrompt;

    // This makes sure we can't open the inventory
    // or another terminal at the same time.
    public static bool isDialogueActive = false;

    // --- Private References ---
    private bool playerIsInRange = false;
    private PlayerMovement playerMovement;
    private PlayerWallSlide playerWallSlide;

    // NEW: A variable to remember which panel is currently open
    private GameObject currentActivePanel;

    void Awake()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider2D>().isTrigger = true;
        
        // Hide all UI at the start
        if(interactPrompt != null) interactPrompt.SetActive(false);
        if(firstPanel != null) firstPanel.SetActive(false);
        // (We don't need to hide all panels, as they should be disabled in the prefab/scene)
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered terminal range.");
            playerIsInRange = true;
            if(interactPrompt != null) interactPrompt.SetActive(true);

            // Get the player's scripts so we can freeze them
            playerMovement = collision.GetComponent<PlayerMovement>();
            playerWallSlide = collision.GetComponent<PlayerWallSlide>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player left terminal range.");
            playerIsInRange = false;
            if(interactPrompt != null) interactPrompt.SetActive(false);
            
            // We're leaving, so if dialogue is open, close it
            if(isDialogueActive)
            {
                CloseDialogue(); 
            }

            // Clear the references
            playerMovement = null;
            playerWallSlide = null;
        }
    }

    void Update()
    {
        // Check if the player is in range and presses the interact key
        if (playerIsInRange && Input.GetKeyDown(KeyCode.E) && !isDialogueActive)
        {
            OpenDialogue();
        }
    }

    /// <summary>
    /// Pauses the game, freezes the player, and shows the first panel.
    /// </summary>
    public void OpenDialogue()
    {
        Debug.Log("Dialogue Opened.");
        isDialogueActive = true;
        Time.timeScale = 0f; // Pause the game
        
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerWallSlide != null) playerWallSlide.enabled = false;

        if (firstPanel != null)
        {
            firstPanel.SetActive(true);
            currentActivePanel = firstPanel; // Remember this is the open panel
        }
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    // --- NEW GENERIC PANEL-SWITCHING FUNCTION ---
    /// <summary>
    /// This is the function ALL your dialogue buttons will call.
    /// It closes the current panel and opens the one you specify.
    /// </summary>
    public void GoToPanel(GameObject panelToOpen)
    {
        // 1. Close the old panel (if one is open)
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }

        // 2. Open the new panel
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
            currentActivePanel = panelToOpen; // Remember this new one
        }
        else
        {
            // If the "next panel" is null, they must have clicked an "End" button
            CloseDialogue();
        }
    }

    /// <summary>
    /// A public function your "Exit" buttons will call.
    /// Unpauses, re-enables the player, and closes all panels.
    /// </summary>
    public void CloseDialogue()
    {
        Debug.Log("Dialogue Closed.");
        
        // Close the currently active panel
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }
        
        Time.timeScale = 1f; // Unpause the game
        
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerWallSlide != null) playerWallSlide.enabled = true;
        
        isDialogueActive = false;
        currentActivePanel = null;
        
        // Show the prompt again if we're still in range
        if(interactPrompt != null && playerIsInRange)
        {
            interactPrompt.SetActive(true);
        }
    }
}