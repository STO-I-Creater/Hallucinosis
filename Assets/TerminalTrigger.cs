using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TerminalTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactButton; // Optional terminal interact button
    public GameObject tabletUI;       // Terminal panel
    public Button closeButton;

    private bool isPlayerNearby = false;

    void Start()
    {
        if (interactButton != null)
            interactButton.SetActive(false);

        if (tabletUI != null)
            tabletUI.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTablet);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Always show terminal interact button regardless of puzzle
            if (interactButton != null)
                interactButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactButton != null)
                interactButton.SetActive(false);
        }
    }

    public void OpenTablet()
    {
        if (tabletUI != null)
            tabletUI.SetActive(true);

        if (interactButton != null)
            interactButton.SetActive(false);
    }

    public void CloseTablet()
    {
        if (tabletUI != null)
            tabletUI.SetActive(false);
    }
}
