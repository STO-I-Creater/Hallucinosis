using UnityEngine;

public class VentTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject interactButton;
    public GameObject puzzlePanel;

    [Header("Animation Targets")]
    public Animator animator1;
    public Animator animator2;
    public Animator animator3;
    public string triggerName = "Play";

    [HideInInspector]
    public bool puzzleSolved = false;

    void Start()
    {
        if (interactButton) interactButton.SetActive(false);
        if (puzzlePanel) puzzlePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !puzzleSolved)
        {
            interactButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !puzzleSolved)
        {
            interactButton.SetActive(false);
        }
    }

    // Called when player clicks the interact button
    public void OnInteract()
    {
        if (puzzlePanel)
        {
            puzzlePanel.SetActive(true);
            if (interactButton) interactButton.SetActive(false);
        }
    }

    // Called by VentManager when puzzle is solved
    public void HandlePuzzleSolved()
    {
        puzzleSolved = true;

        // Close the puzzle panel
        if (puzzlePanel) puzzlePanel.SetActive(false);
        if (interactButton) interactButton.SetActive(false);

        // Play additional animations if needed
        if (animator1) animator1.SetTrigger(triggerName);
        if (animator2) animator2.SetTrigger(triggerName);
        if (animator3) animator3.SetTrigger(triggerName);
    }
}
