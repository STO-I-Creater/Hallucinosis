using UnityEngine;
using System.Collections;

public class VentManager : MonoBehaviour
{
    [Header("UI & Panels")]
    public GameObject interactButton;   // Optional: shows when player is near
    public GameObject puzzlePanel;      // Panel with screws
    public GameObject ventPanel;        // Vent cover in scene (optional)

    [Header("Animations")]
    public Animator anim1;
    public Animator anim2;
    public Animator anim3;
    public string animationTrigger = "Play"; // All animations use this trigger

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip ventOpenSound;

    [Header("Settings")]
    public int totalScrews = 4;

    private int removedScrews = 0;
    private bool ventOpened = false;

    // Show interact button when player enters
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !ventOpened)
        {
            if (interactButton) interactButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player") && !ventOpened)
        {
            if (interactButton) interactButton.SetActive(false);
        }
    }

    // Called by the interact button
    public void OnInteract()
    {
        if (puzzlePanel && !ventOpened)
        {
            puzzlePanel.SetActive(true);
            if (interactButton) interactButton.SetActive(false);
        }
    }

    // Called by each UIScrew when removed
    public void ScrewRemoved()
    {
        if (ventOpened) return;

        removedScrews++;
        Debug.Log("Screw removed: " + removedScrews + "/" + totalScrews);

        if (removedScrews >= totalScrews)
        {
            ventOpened = true;
            StartCoroutine(OpenVentSequence());
        }
    }

    private IEnumerator OpenVentSequence()
    {
        // Hide vent panel if assigned
        if (ventPanel) ventPanel.SetActive(false);

        // Close puzzle panel immediately
        if (puzzlePanel) puzzlePanel.SetActive(false);

        // Play animations
        if (anim1) anim1.SetTrigger(animationTrigger);
        if (anim2) anim2.SetTrigger(animationTrigger);
        if (anim3) anim3.SetTrigger(animationTrigger);

        // Play vent sound
        if (audioSource && ventOpenSound) audioSource.PlayOneShot(ventOpenSound);

        // Wait for longest animation length to finish
        float longestAnim = 0f;
        if (anim1) longestAnim = Mathf.Max(longestAnim, anim1.GetCurrentAnimatorStateInfo(0).length);
        if (anim2) longestAnim = Mathf.Max(longestAnim, anim2.GetCurrentAnimatorStateInfo(0).length);
        if (anim3) longestAnim = Mathf.Max(longestAnim, anim3.GetCurrentAnimatorStateInfo(0).length);

        yield return new WaitForSeconds(longestAnim);

        Debug.Log("Vent sequence completed!");
    }
}
