using UnityEngine;
using System.Collections;

public class Fadinnotification : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Reference to CanvasGroup for fading
    public float fadeDuration = 0.5f;
    public float displayTime = 1.5f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Ensure panel is active but invisible at start
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
    }

    public void ShowNotification()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInAndOut());
    }

    private IEnumerator FadeInAndOut()
    {
        // Make sure panel is active
        canvasGroup.gameObject.SetActive(true);

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Wait
        yield return new WaitForSecondsRealtime(displayTime);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        // Optional: deactivate panel after fade
        // canvasGroup.gameObject.SetActive(false);
    }
}
