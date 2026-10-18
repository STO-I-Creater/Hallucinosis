using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

/// <summary>
/// Controls the sliding animation for the top-right notification panel.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class NotificationPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI notificationText; // Corrected to use TMPro
    
    [Header("Animation Settings")]
    [Tooltip("How long the panel slides in (seconds).")]
    public float slideInDuration = 0.5f;
    [Tooltip("How long the panel remains visible (seconds).")]
    public float displayDuration = 2.0f;
    
    private RectTransform rectTransform;
    private Vector2 visiblePosition = Vector2.zero; // Visible position is the anchor point (0, 0)
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // CRITICAL FIX 1: Force the layout to rebuild to get the true scaled width.
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform); 
        
        // Calculate the hidden position based on calculated width
        float panelWidth = rectTransform.rect.width;
        Vector2 hiddenPosition = new Vector2(panelWidth, 0); 

        // Start the panel in the hidden state (off-screen)
        rectTransform.anchoredPosition = hiddenPosition;
        
        // CRITICAL FIX 2: DO NOT set the GameObject inactive. 
        // We must keep the component active so StartCoroutine() can be called externally.
        // gameObject.SetActive(false); // <-- THIS LINE IS REMOVED
    }

    /// <summary>
    /// Shows the notification with the given message, sliding it in and out.
    /// </summary>
    public void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
        }
        
        // Since the GameObject is now always active, StartCoroutine works.
        StopAllCoroutines(); 
        StartCoroutine(AnimateNotification());
    }

    private IEnumerator AnimateNotification()
    {
        // Now that the panel is permanently active, we don't need to call SetActive(true) here.
        float startTime = Time.time;
        
        // Recalculate positions dynamically just before animation starts
        float panelWidth = rectTransform.rect.width;
        Vector2 currentHiddenPosition = new Vector2(panelWidth, 0);
        
        // --- 1. SLIDE IN ---
        while (Time.time < startTime + slideInDuration)
        {
            float t = (Time.time - startTime) / slideInDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(currentHiddenPosition, visiblePosition, t);
            yield return null;
        }
        rectTransform.anchoredPosition = visiblePosition; 
        
        // --- 2. DISPLAY DURATION ---
        yield return new WaitForSeconds(displayDuration);
        
        // --- 3. SLIDE OUT ---
        startTime = Time.time;
        while (Time.time < startTime + slideInDuration)
        {
            float t = (Time.time - startTime) / slideInDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(visiblePosition, currentHiddenPosition, t);
            yield return null;
        }
        
        // Hide the panel by moving it off-screen again.
        rectTransform.anchoredPosition = currentHiddenPosition;
    }
}