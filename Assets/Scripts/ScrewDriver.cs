using UnityEngine;
using System.Collections;

public class UIScrew : MonoBehaviour
{
    public Animator animator;        // Screw click animation
    public VentManager ventManager;  // Assign in inspector
    public string animationName = "ScrewClick";
    public int framesToWait = 40;

    private bool clicked = false;

    public void OnClick()
    {
        if (clicked) return;
        clicked = true;

        // Play screw animation
        if (animator) animator.Play(animationName);

        // Wait for a few frames then notify VentManager
        StartCoroutine(RemoveAfterFrames());
    }

    private IEnumerator RemoveAfterFrames()
    {
        for (int i = 0; i < framesToWait; i++)
            yield return null;

        // Hide the screw object
        gameObject.SetActive(false);

        // Notify VentManager
        if (ventManager != null)
            ventManager.ScrewRemoved();
    }
}
