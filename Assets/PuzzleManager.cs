using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [Header("Puzzle Settings")]
    public int switchCount;          // Total switches needed
    public GameObject winText;       // UI text to show when puzzle is complete
    public AudioSource winSound;     // Sound to play when player wins

    [Header("Scene Settings")]
    public string returnSceneName = "MainScene"; // Your main scene

    private int onCount = 0;         // Current switches activated

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Call this from your switches when their state changes
    /// </summary>
    public void SwitchChange(int points)
    {
        onCount += points;

        if (onCount >= switchCount)
        {
            if (winText != null)
                winText.SetActive(true);

            if (winSound != null)
                winSound.Play(); // Play sound when "You Win" text appears

            CompletePuzzle();
            StartCoroutine(ReturnToMainScene());
        }
    }

    public void CompletePuzzle()
    {
        // Mark puzzle as completed permanently
        PlayerPrefs.SetInt("PuzzleCompleted", 1);
        PlayerPrefs.Save();
    }

    private IEnumerator ReturnToMainScene()
    {
        yield return new WaitForSeconds(1.5f); // Optional delay before returning
        SceneManager.LoadScene(returnSceneName);
    }
}
    