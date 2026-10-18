using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TwineDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject terminalPanel;           // The terminal/tablet panel
    public TextMeshProUGUI dialogueText;       // Drag your TMP object here
    public Transform buttonParent;             // Parent to hold choice buttons
    public Button yesButtonPrefab;             // Button prefab for yes
    public Button noButtonPrefab;              // Button prefab for no
    public Button questionButtonPrefab;        // Button prefab for general questions

    [Header("Player References")]
    public PlayerMovement playerMovement;
    public PlayerWallSlide playerWallSlide;

    [Header("Twine JSON")]
    public TextAsset twineJsonFile;            // Drag your exported Twine JSON here

    private TwineData twineData;
    private Passage currentPassage;

    void Start()
    {
        terminalPanel.SetActive(false);

        if (twineJsonFile != null)
        {
            twineData = JsonUtility.FromJson<TwineData>(twineJsonFile.text);
        }
        else
        {
            Debug.LogError("No Twine JSON assigned!");
        }
    }

    public void OpenTerminal(string startPassageName = "")
    {
        terminalPanel.SetActive(true);
        Time.timeScale = 0f;

        if (playerMovement) playerMovement.enabled = false;
        if (playerWallSlide) playerWallSlide.enabled = false;

        string startName = string.IsNullOrEmpty(startPassageName) ? twineData.startnode : startPassageName;
        currentPassage = GetPassageByName(startName);

        DisplayCurrentPassage();
    }

    public void CloseTerminal()
    {
        terminalPanel.SetActive(false);
        Time.timeScale = 1f;

        if (playerMovement) playerMovement.enabled = true;
        if (playerWallSlide) playerWallSlide.enabled = true;

        ClearButtons();
    }

    private void DisplayCurrentPassage()
    {
        if (currentPassage == null) return;

        // Show dialogue
        dialogueText.text = currentPassage.text;

        // Show links as buttons
        ClearButtons();

        foreach (var link in currentPassage.links)
        {
            Button prefab = questionButtonPrefab; // Default button
            if (link.name.ToLower() == "yes") prefab = yesButtonPrefab;
            else if (link.name.ToLower() == "no") prefab = noButtonPrefab;

            Button btn = Instantiate(prefab, buttonParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = link.name;
            btn.onClick.AddListener(() => OnLinkClicked(link));
        }
    }

    private void OnLinkClicked(Link link)
    {
        // Find next passage
        currentPassage = GetPassageByName(link.link);
        if (currentPassage == null)
        {
            CloseTerminal();
        }
        else
        {
            DisplayCurrentPassage();
        }
    }

    private Passage GetPassageByName(string name)
    {
        foreach (var passage in twineData.passages)
        {
            if (passage.name == name)
                return passage;
        }
        Debug.LogWarning("Passage not found: " + name);
        return null;
    }

    private void ClearButtons()
    {
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }
    }
}
