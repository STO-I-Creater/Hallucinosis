using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneSlideshow : MonoBehaviour
{
    [Header("Slides")]
    public Sprite[] frames;
    public float slideDuration = 3.0f;

    [Header("Fade Settings")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 1f;

    [Header("UI Image")]
    public Image cutsceneImage;

    [Header("Audio")]
    public AudioSource sfx;

    [Header("Next Scene")]
    public string nextSceneName = "Level1";

    bool skipRequested = false;

    void Update()
    {
        // Touch or click anywhere on screen → skip slide
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            skipRequested = true;
        }
    }

    void Start()
    {
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        Time.timeScale = 1f;

        fadeGroup.alpha = 1f;
        yield return StartCoroutine(FadeTo(0f));

        for (int i = 0; i < frames.Length; i++)
        {
            skipRequested = false;
            cutsceneImage.sprite = frames[i];

            if (i == 1 && sfx != null)
                sfx.Play();

            float t = 0f;

            // Wait until slideDuration OR skip
            while (t < slideDuration && !skipRequested)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // If last frame and skip pressed → exit immediately
            if (i == frames.Length - 1 && skipRequested)
                break;
        }

        // Fade OUT
        yield return StartCoroutine(FadeTo(1f));

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = fadeGroup.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = targetAlpha;
    }
}
