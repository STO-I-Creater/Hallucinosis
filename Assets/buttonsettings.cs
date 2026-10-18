using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OpenSettingsWithButtonClick : MonoBehaviour
{
    public Sprite normalSprite;        // Default button sprite
    public Sprite clickedSprite;       // Sprite to show on click
    public float animationTime = 0.15f; // How long to show clicked sprite
    public GameObject settingsPanel;    // The panel to open
    public float panelDelay = 0.5f;     // Delay before opening panel

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        if(img != null)
            img.sprite = normalSprite;
    }

    public void OnClick()
    {
        StartCoroutine(ClickAnimationAndOpenPanel());
    }

    private IEnumerator ClickAnimationAndOpenPanel()
    {
        if(img != null)
            img.sprite = clickedSprite;              // Show clicked sprite

        yield return new WaitForSecondsRealtime(animationTime); // Wait briefly

        if(img != null)
            img.sprite = normalSprite;               // Revert sprite

        // Wait before opening the panel
        yield return new WaitForSecondsRealtime(panelDelay);

        if(settingsPanel != null)
            settingsPanel.SetActive(true);          // Open panel
        else
            Debug.LogWarning("Settings panel not assigned!");
    }
}
