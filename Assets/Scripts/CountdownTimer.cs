using UnityEngine;
using UnityEngine.Events;
using TMPro; 
using UnityEngine.UI; 

/// <summary>
/// A countdown timer that fires an event when it reaches zero.
/// Can optionally update a text element and a radial fill image.
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("The duration of the timer in seconds.")]
    public float timerDuration = 60f;

    [Tooltip("Check this if the timer should start automatically when the scene loads.")]
    public bool startOnAwake = true;

    [Header("Optional UI")]
    [Tooltip("Drag a TextMeshPro UI element here to display the time. (Optional)")]
    public TextMeshProUGUI timerText;

    [Tooltip("NEW: Drag a UI Image here. Set its Image Type to 'Filled' and Fill Method to 'Radial 360'.")]
    public Image radialFillImage; // NEW

    [Header("Event")]
    [Tooltip("This event will be fired when the timer reaches zero. You can leave this empty for now.")]
    public UnityEvent onTimerEnd; 

    // --- Private State ---
    private float currentTime;
    private bool isTimerRunning = false;

    void Awake()
    {
        if (startOnAwake)
        {
            StartTimer();
        }
        else
        {
            // If not starting on awake, set the initial text
            currentTime = timerDuration;
            UpdateTimerText(currentTime);
            UpdateRadialFill(currentTime); // NEW
        }
    }

    /// <summary>
    /// Public function to start the timer (or restart it).
    /// </summary>
    public void StartTimer()
    {
        currentTime = timerDuration;
        isTimerRunning = true;
        Debug.Log($"Timer started for {timerDuration} seconds.");
    }

    /// <summary>
    /// Public function to stop the timer.
    /// </summary>
    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("Timer stopped.");
    }

    void Update()
    {
        if (isTimerRunning)
        {
            // Count down
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                // --- TIMER ENDS ---
                currentTime = 0;
                isTimerRunning = false;
                
                Debug.Log("Timer has reached zero!");
                
                if (onTimerEnd != null)
                {
                    onTimerEnd.Invoke();
                }
            }

            // Update the UI text every frame
            UpdateTimerText(currentTime);
            UpdateRadialFill(currentTime); // NEW
        }
    }

    /// <summary>
    /// Formats and updates the optional UI text.
    /// </summary>
    private void UpdateTimerText(float time)
    {
        if (timerText != null)
        {
            // --- UPDATED LOGIC ---
            // Round up to the nearest whole number (e.g., 9.8s shows "10").
            int secondsRemaining = Mathf.CeilToInt(time);
            
            // Set the text to just the number, no "00:" formatting.
            timerText.text = secondsRemaining.ToString();
        }
    }

    /// <summary>
    /// NEW: Updates the fill of the radial image to show the 1-second "rotation".
    /// </summary>
    private void UpdateRadialFill(float time)
    {
        if (radialFillImage != null)
        {
            // "time % 1.0f" gets just the fractional part of the time.
            // e.g., 9.8s -> 0.8
            // e.g., 9.2s -> 0.2
            // This makes the circle "drain" from 100% to 0% every second.
            radialFillImage.fillAmount = time % 1.0f;
        }
    }
}