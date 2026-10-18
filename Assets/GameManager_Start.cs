using UnityEngine;

public class GameManager_Start : MonoBehaviour
{
    void Start()
    {
        // Initialize puzzle state only the FIRST time
        if (!PlayerPrefs.HasKey("PuzzleCompleted"))
        {
            PlayerPrefs.SetInt("PuzzleCompleted", 0);
            PlayerPrefs.Save();
        }
    }
}
