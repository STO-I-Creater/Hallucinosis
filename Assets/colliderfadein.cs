using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeOnCollision : MonoBehaviour
{
    [Tooltip("The UI panel to fade in/out")]
    public CanvasGroup panel;

    [Tooltip("Time it takes to fade in/out")]
    public float fadeDuration = 1f;

    private bool hasCollided = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) return; // Do nothing on second collision

        if (other.CompareTag("Player")) // Change to match your player's tag
        {
            hasCollided = true;
            StartCoroutine(FadePanel());
        }
    }

    private IEnumerator FadePanel()
    {
        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            panel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        // Optional: keep fully visible for a short time
        yield return new WaitForSeconds(0.5f);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            panel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
    }
}
