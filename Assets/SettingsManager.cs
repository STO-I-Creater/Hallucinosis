using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider volumeSlider;

    private const string VolumeKey = "MasterVolumeValue";

    void Start()
    {
        // Load volume if saved
        if (PlayerPrefs.HasKey(VolumeKey))
        {
            float savedValue = PlayerPrefs.GetFloat(VolumeKey);
            volumeSlider.value = savedValue;
            SetVolume(savedValue);
        }
        else
        {
            // First time playing
            volumeSlider.value = 1f;
            SetVolume(1f);
        }

        // Add listener
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        // Prevent volume from going to -∞ dB
        if (value <= 0.0001f) value = 0.0001f;

        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);

        // Save volume
        PlayerPrefs.SetFloat(VolumeKey, value);
    }
}
