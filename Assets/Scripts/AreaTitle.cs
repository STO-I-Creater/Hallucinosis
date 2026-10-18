using System.Collections;
using UnityEngine;
using TMPro; // Needed for the title text

/// <summary>
/// Place this on a trigger zone to display an area title once.
/// It temporarily boosts BGM volume for dramatic effect.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AreaTitle : MonoBehaviour
{
    [Header("Area Settings")]
    [Tooltip("The name of the area to display.")]
    public string areaTitle = "The Forgotten Crypt";
    [Tooltip("How long the title remains on screen (after fade in).")]
    public float displayDuration = 3.0f;
    [Tooltip("How long the title takes to fade in and out.")]
    public float titleFadeTime = 1.0f; // NEW: Fade duration

    [Header("Volume Control")]
    [Tooltip("The AudioSource playing the main background music (BGM).")]
    public AudioSource backgroundMusicSource;
    [Tooltip("How much to temporarily increase the volume (e.g., 0.2).")]
    public float volumeBoost = 0.2f;
    [Tooltip("Time it takes for volume to fade up/down.")]
    public float volumeFadeTime = 0.5f;

    [Header("UI Reference")]
    [Tooltip("Drag the TextMeshPro UI object that will display the title.")]
    public TextMeshProUGUI titleTextUI;

    // --- Private State ---
    private bool hasBeenShown = false;
    private float originalVolume;
    private bool isPlayerInRange = false;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        
        if (titleTextUI != null)
        {
            // Set the text, but hide it by setting its alpha to 0 (completely transparent)
            titleTextUI.text = areaTitle;
            Color color = titleTextUI.color;
            titleTextUI.color = new Color(color.r, color.g, color.b, 0f);
        }

        if (backgroundMusicSource != null)
        {
            originalVolume = backgroundMusicSource.volume;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (!hasBeenShown)
            {
                // Title has never been shown, start the effect
                StartCoroutine(ShowTitleSequence());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private IEnumerator ShowTitleSequence()
    {
        hasBeenShown = true;
        
        // --- STEP 1: Increase Volume ---
        if (backgroundMusicSource != null)
        {
            StartCoroutine(FadeVolume(originalVolume + volumeBoost, volumeFadeTime));
        }

        // --- STEP 2: Fade In Title ---
        if (titleTextUI != null)
        {
            titleTextUI.gameObject.SetActive(true); // Must be active to fade
            yield return StartCoroutine(FadeTitle(1f, titleFadeTime)); // Fade in to 100% alpha
        }

        // --- STEP 3: Wait ---
        yield return new WaitForSeconds(displayDuration);

        // --- STEP 4: Fade Out Title ---
        if (titleTextUI != null)
        {
            yield return StartCoroutine(FadeTitle(0f, titleFadeTime)); // Fade out to 0% alpha
            titleTextUI.gameObject.SetActive(false); // Hide the object completely afterward
        }

        // --- STEP 5: Decrease Volume ---
        if (backgroundMusicSource != null)
        {
            StartCoroutine(FadeVolume(originalVolume, volumeFadeTime));
        }
    }

    /// <summary>
    /// Smoothly fades the volume up or down.
    /// </summary>
    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        if (backgroundMusicSource == null) yield break;

        float startVolume = backgroundMusicSource.volume;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            backgroundMusicSource.volume = Mathf.Lerp(startVolume, targetVolume, (Time.time - startTime) / duration);
            yield return null;
        }

        // Ensure final volume is exactly the target
        backgroundMusicSource.volume = targetVolume;
    }

    /// <summary>
    /// NEW: Smoothly fades the title text's alpha to a target value (0 for out, 1 for in).
    /// </summary>
    private IEnumerator FadeTitle(float targetAlpha, float duration)
    {
        if (titleTextUI == null) yield break;

        Color startColor = titleTextUI.color;
        float startAlpha = startColor.a;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, (Time.time - startTime) / duration);
            titleTextUI.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null;
        }

        // Ensure final alpha is exactly the target
        titleTextUI.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }
}