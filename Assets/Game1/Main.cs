using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public static Main Instance;

    [Header("Puzzle Settings")]
    public int switchCount = 3;          
    public string returnSceneName = "MainScene"; 

    [Header("UI & Feedback")]
    public GameObject winText;
    public AudioClip winSound;           
    public float winDisplayTime = 1.5f;

    private int onCount = 0;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;

        // Add an AudioSource for playing the win sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Hide the win text at start
        if (winText != null)
            winText.SetActive(false);
    }

    /// <summary>
    /// Call this when a switch is correctly connected.
    /// </summary>
    /// <param name="points">Points to add (usually 1 per correct switch)</param>
    public void SwitchChange(int points)
    {
        onCount += points;
        Debug.Log($"SwitchChange: onCount = {onCount}/{switchCount}");

        if (onCount >= switchCount)
        {
            StartCoroutine(PuzzleCompleted());
        }
    }

    private IEnumerator PuzzleCompleted()
    {
        Debug.Log("Puzzle completed!");

        // Show "win" text
        if (winText != null)
            winText.SetActive(true);

        // Play win sound
        if (winSound != null && audioSource != null)
        {
            audioSource.clip = winSound;
            audioSource.Play();
        }

        // Wait for feedback duration
        yield return new WaitForSeconds(winDisplayTime);

        // Set universal key for puzzle completion
        PlayerPrefs.SetInt("UniversalPuzzleCompleted", 1);
        PlayerPrefs.SetInt("ReturnFromPuzzle", 1); // optional: to trigger return message
        PlayerPrefs.Save();

        // Load the return scene
        SceneManager.LoadScene(returnSceneName);
    }
}
