using UnityEngine;
using UnityEngine.Audio;

public class AutoAssignMixer : MonoBehaviour
{
    public AudioMixerGroup masterGroup;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        AudioSource[] sources = FindObjectsOfType<AudioSource>();

        foreach (var src in sources)
        {
            src.outputAudioMixerGroup = masterGroup;
        }
    }
}
