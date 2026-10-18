using UnityEngine;
using UnityEngine.UI; // Required for UI components

/// <summary>
/// This script goes on the Slider object itself.
/// It listens to events from PlayerHealth to update the bar.
/// </summary>
[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        // Get the Slider component on this same GameObject
        slider = GetComponent<Slider>();
    }

    /// <summary>
    /// A public function that sets the slider's max value.
    /// This will be connected to the PlayerHealth 'OnMaxHealthSet' event.
    /// </summary>
    public void SetMaxHealth(int health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health; // Start at full health
        }
    }

    /// <summary>
    /// A public function that updates the slider's current value.
    /// This will be connected to the PlayerHealth 'OnHealthChanged' event.
    /// </summary>
    public void SetHealth(int health)
    {
        if (slider != null)
        {
            slider.value = health;
        }
    }
}