using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UnifiedPuzzleTrigger : MonoBehaviour
{
    [Header("UI References")]
    public Button interactButton;       // Button player presses to interact
    public GameObject panelUI;          // Panel to open after puzzle is completed
    public Button closeButton;          // Close button for panel
    public TMP_Text messageText;        // Optional TMP message

    [Header("Puzzle Settings")]
    public string puzzleSceneName;      // Scene name for the puzzle
    public string returnSceneName;      // Scene to return to after puzzle
    public float messageDuration = 0.5f;

    private bool playerInside = false;

    void Start()
    {
        // Initialize puzzle state to 0 only if key doesn't exist
        if (!PlayerPrefs.HasKey("UniversalPuzzleCompleted"))
        {
            PlayerPrefs.SetInt("UniversalPuzzleCompleted", 0);
            PlayerPrefs.Save();
            Debug.Log("UniversalPuzzleCompleted initialized to 0");
        }

        // Hide UI elements at start
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(false);
            interactButton.onClick.AddListener(OnInteractButtonPressed);
        }

        if (panelUI != null)
            panelUI.SetActive(false);

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = true;

        // Show interact button
        if (interactButton != null)
            interactButton.gameObject.SetActive(true);

        int completed = PlayerPrefs.GetInt("UniversalPuzzleCompleted", 0);
        Debug.Log($"UnifiedPuzzleTrigger: Puzzle completed? {completed == 1}");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerInside = false;

        if (interactButton != null)
            interactButton.gameObject.SetActive(false);
    }

    public void OnInteractButtonPressed()
    {
        int completed = PlayerPrefs.GetInt("UniversalPuzzleCompleted", 0);

        if (completed == 0)
        {
            // Puzzle not completed → load puzzle scene
            if (!string.IsNullOrEmpty(puzzleSceneName))
            {
                Debug.Log("UnifiedPuzzleTrigger: Loading puzzle scene...");
                SceneManager.LoadScene(puzzleSceneName);
            }
        }
        else
        {
            // Puzzle completed → show panel
            if (panelUI != null)
                panelUI.SetActive(true);

            if (messageText != null)
            {
                messageText.text = "Panel opened!";
                messageText.gameObject.SetActive(true);
                StartCoroutine(HideMessageAfterDelay(messageDuration));
            }

            if (interactButton != null)
                interactButton.gameObject.SetActive(false); // hide button while panel is open
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    public void ClosePanel()
    {
        if (panelUI != null)
            panelUI.SetActive(false);

        if (interactButton != null && playerInside)
            interactButton.gameObject.SetActive(true);
    }
}
