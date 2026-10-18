using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    public float hideDelay = 1f; // Time before panel hides

    public void OpenPanel()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;   // Freeze game
    }

    // Immediately closes the panel
    public void ClosePanel()
    {
        StartCoroutine(ClosePanelWithDelay());
    }

    // Toggle panel open/close
    public void TogglePanel()
    {
        bool newState = !panel.activeSelf;
        panel.SetActive(newState);
        Time.timeScale = newState ? 0f : 1f;
    }

    // Coroutine to delay hiding the panel
    private IEnumerator ClosePanelWithDelay()
    {
        yield return new WaitForSecondsRealtime(hideDelay); // Use Realtime so it works even when Time.timeScale = 0
        panel.SetActive(false);
        Time.timeScale = 1f;   // Resume game
    }
}
