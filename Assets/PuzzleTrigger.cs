using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PuzzleTrigger : MonoBehaviour
{
    [Header("UI References")]
    public Button interactButton;    // Assign your interact button in the inspector
    public string puzzleSceneName;   // Scene name for the puzzle

    // --- Internal state ---
    private bool hasTriggered = false;   // Ensure it triggers only once
    private bool playerInside = false;   // Track if player is in trigger
    private bool enabledByUI = false;    // Track if UI called EnableFromUI

    private void Start()
    {
        // Hide the interact button at start
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
            interactButton.onClick.AddListener(OnPuzzleButtonClicked);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            playerInside = true;

            // Show button only if puzzle is not solved
            if (PlayerPrefs.GetInt("PuzzleCompleted", 0) == 0)
            {
                interactButton.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;

            // Hide the button when leaving
            if (interactButton != null)
                interactButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by the interact button to start the puzzle
    /// </summary>
    public void OnPuzzleButtonClicked()
    {
        if (hasTriggered) return;

        hasTriggered = true;

        // Hide the button immediately
        if (interactButton != null)
            interactButton.gameObject.SetActive(false);

        // Mark as triggered by UI (optional, for logic elsewhere)
        enabledByUI = true;

        // Save that the puzzle is being solved (optional)
        PlayerPrefs.SetInt("SolvedPuzzle", 1);
        PlayerPrefs.Save();

        // Load the puzzle scene
        if (!string.IsNullOrEmpty(puzzleSceneName))
            SceneManager.LoadScene(puzzleSceneName);
    }

    /// <summary>
    /// Called externally from UI to trigger puzzle without waiting for player to enter
    /// </summary>
    public void EnableFromUI()
    {
        enabledByUI = true;

        // If player is already inside the trigger, trigger puzzle immediately
        if (playerInside && !hasTriggered)
        {
            OnPuzzleButtonClicked();
        }
    }
}
