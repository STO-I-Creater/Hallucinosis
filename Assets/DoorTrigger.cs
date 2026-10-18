using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("References")]
    public AudioSource doorSound;        // Sound on interaction
    public GameObject interactButton;    // UI button shown when near
    public CanvasGroup infoTextGroup;    // Text that fades out (CanvasGroup)

    [Header("Settings")]
    public string playerTag = "Player";
    public float textFadeDuration = 1f;  // How long text takes to fade out
    public float textVisibleTime = 1f;   // How long text stays visible before fading

    private bool playerNear = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Hide interact button at start
        if (interactButton != null)
        {
            interactButton.SetActive(false);

            Button btn = interactButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(Interact);
        }

        // Hide fading text at start
        if (infoTextGroup != null)
            infoTextGroup.alpha = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag)) return;

        playerNear = true;

        if (interactButton != null)
            interactButton.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag)) return;

        playerNear = false;

        if (interactButton != null)
            interactButton.SetActive(false);

        // Fade text out when walking away
        if (infoTextGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTextOut());
        }
    }

    private void Interact()
    {
        if (!playerNear) return;

        if (doorSound != null)
            doorSound.Play();

        // Show text instantly when interacting
        if (infoTextGroup != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(ShowAndFadeText());
        }

        Debug.Log("Door interacted!");
    }

    private IEnumerator ShowAndFadeText()
    {
        // Make text visible
        infoTextGroup.alpha = 1f;

        // Keep it on screen for a moment
        yield return new WaitForSeconds(textVisibleTime);

        // Fade out
        yield return StartCoroutine(FadeTextOut());
    }

    private IEnumerator FadeTextOut()
    {
        float startAlpha = infoTextGroup.alpha;
        float t = 0f;

        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            infoTextGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / textFadeDuration);
            yield return null;
        }

        infoTextGroup.alpha = 0f;
    }
}
