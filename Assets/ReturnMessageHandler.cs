using UnityEngine;
using TMPro;
using System.Collections;

public class ReturnMessageHandler : MonoBehaviour
{
    [Header("UI & Audio")]
    public TMP_Text messageText;        // TextMeshPro text to display the message
    public AudioSource messageSound;    // Sound to play with the message
    public GameObject panelUI;          // Panel to open after the message

    [Header("Settings")]
    public float delayBeforePanel = 1f; // Delay in seconds before opening panel
    public string returnMessage = "Terminal Fixed!";

    void Start()
    {
        // Hide message and panel initially
        if (messageText != null)
            messageText.gameObject.SetActive(false);

        if (panelUI != null)
            panelUI.SetActive(false);

        // Check if the player just returned from a puzzle
        if (PlayerPrefs.GetInt("ReturnFromPuzzle", 0) == 1)
        {
            PlayerPrefs.SetInt("ReturnFromPuzzle", 0);
            PlayerPrefs.Save();

            StartCoroutine(ShowMessageThenOpenPanel());
        }
    }

    private IEnumerator ShowMessageThenOpenPanel()
    {
        // Show the message
        if (messageText != null)
        {
            messageText.text = returnMessage;
            messageText.gameObject.SetActive(true);
        }

        // Play the sound
        if (messageSound != null)
            messageSound.Play();

        // Wait for the delay
        yield return new WaitForSeconds(delayBeforePanel);

        // Hide the message
        if (messageText != null)
            messageText.gameObject.SetActive(false);

        // Open the panel
        if (panelUI != null)
            panelUI.SetActive(true);
    }
}
