using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class TutorialPanelTrigger : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup tutorialPanel;   // Panel with CanvasGroup
    public Image tutorialImage;         // Optional image inside panel
    public AudioSource openSound;       // Sound when panel opens

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    private bool hasTriggered = false;

    // Optional PlayerPrefs key to remember tutorial completion
    public string tutorialKey = "";

    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.alpha = 0f;
            tutorialPanel.gameObject.SetActive(false);
            tutorialPanel.blocksRaycasts = false;
        }

        // Check PlayerPrefs if tutorial already completed
        if (!string.IsNullOrEmpty(tutorialKey))
        {
            if (PlayerPrefs.GetInt(tutorialKey, 0) == 1)
                hasTriggered = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;

            // Save tutorial completion (optional)
            if (!string.IsNullOrEmpty(tutorialKey))
                PlayerPrefs.SetInt(tutorialKey, 1);

            OpenPanel();
        }
    }

    void Update()
    {
        if (tutorialPanel != null && tutorialPanel.gameObject.activeSelf)
        {
            // Mouse click outside panel closes it
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Input.mousePosition;
                RectTransform rt = tutorialPanel.GetComponent<RectTransform>();

                if (!RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, null))
                    ClosePanel();
            }

            // Touch support
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    Vector2 touchPos = t.position;
                    RectTransform rt = tutorialPanel.GetComponent<RectTransform>();

                    if (!RectTransformUtility.RectangleContainsScreenPoint(rt, touchPos, null))
                        ClosePanel();
                }
            }
        }
    }

    public void OpenPanel()
    {
        if (tutorialPanel == null) return;

        tutorialPanel.gameObject.SetActive(true);
        tutorialPanel.blocksRaycasts = true;

        // Freeze player
        FreezePlayer(true);

        if (openSound != null)
            openSound.Play();

        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(tutorialPanel, 0f, 1f, fadeDuration));
    }

    public void ClosePanel()
    {
        if (tutorialPanel == null) return;

        tutorialPanel.blocksRaycasts = false;

        // Unfreeze player
        FreezePlayer(false);

        StopAllCoroutines();
        StartCoroutine(FadeOutAndDisable(tutorialPanel, fadeDuration));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float t = 0f;
        cg.alpha = start;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // use unscaled time while frozen
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        cg.alpha = end;
    }

    IEnumerator FadeOutAndDisable(CanvasGroup cg, float duration)
    {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f, duration);
        cg.gameObject.SetActive(false);
    }

    private void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Freeze Rigidbody2D
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = freeze ? RigidbodyConstraints2D.FreezeAll :
                                      RigidbodyConstraints2D.FreezeRotation;
        }

        // Disable movement script
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.enabled = !freeze;

            // Stop walking animation if player has Animator
            Animator anim = pm.GetComponent<Animator>();
            if (anim != null)
                anim.enabled = !freeze;

            // Stop movement-related audio (optional)
            AudioSource[] audios = pm.GetComponents<AudioSource>();
            foreach (AudioSource a in audios)
                a.enabled = !freeze;
        }

        // Disable wall sliding
        PlayerWallSlide ws = player.GetComponent<PlayerWallSlide>();
        if (ws != null)
            ws.enabled = !freeze;
    }
}
