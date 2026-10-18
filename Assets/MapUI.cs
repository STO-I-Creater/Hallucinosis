using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public GameObject mapPanel; // Drag MapOverlay here
    public Button closeTrigger; // Drag CloseTrigger button here

    private void Start()
    {
        // When the transparent background is tapped, close the map
        if (closeTrigger != null)
            closeTrigger.onClick.AddListener(CloseMap);
    }

    public void OpenMap()
    {
        mapPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the world
    }

    public void CloseMap()
    {
        mapPanel.SetActive(false);
        Time.timeScale = 1f; // Resume the world
    }
}