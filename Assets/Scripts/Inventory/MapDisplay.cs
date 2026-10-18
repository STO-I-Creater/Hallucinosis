using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    public GameObject mapDisplayObject;   // Panel (should have a CanvasGroup!)
    public float fadeDuration = 0.5f;
    public float displayDuration = 3f;

    private CanvasGroup canvasGroup;
    private Coroutine displayCoroutine;
    private bool isShowing = false;

    void Start()
    {
        // Make sure panel starts hidden
        if (mapDisplayObject != null)
        {
            canvasGroup = mapDisplayObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = mapDisplayObject.AddComponent<CanvasGroup>();

            mapDisplayObject.SetActive(false);
            canvasGroup.alpha = 0;
        }
    }

    public void ShowMapIcon()
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(DisplaySequence());
    }

    private IEnumerator DisplaySequence()
    {
        isShowing = true;

        mapDisplayObject.SetActive(true);

        // --- FADE IN ---
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                break;
        }

        canvasGroup.alpha = 1;

        // --- WAIT FOR DURATION ---
        float timer = 0;
        while (timer < displayDuration)
        {
            timer += Time.unscaledDeltaTime;

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                break;

            yield return null;
        }

        // --- FADE OUT ---
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        mapDisplayObject.SetActive(false);

        isShowing = false;
        displayCoroutine = null;
    }

    void Update()
    {
        if (!isShowing) return;

        // If panel is showing and player clicks ANYWHERE → close instantly.
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            if (displayCoroutine != null)
                StopCoroutine(displayCoroutine);

            StartCoroutine(FadeOutInstant());
        }
    }

    private IEnumerator FadeOutInstant()
    {
        isShowing = false;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        mapDisplayObject.SetActive(false);
        displayCoroutine = null;
    }
}
