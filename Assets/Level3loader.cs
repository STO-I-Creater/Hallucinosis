using UnityEngine;
using UnityEngine.SceneManagement;

public class Level3loader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Destroy(collision.gameObject);

            ResetAllCheckpoints();

            // Example puzzle logic
            PlayerPrefs.SetInt("UniversalPuzzleCompleted", 1);
            PlayerPrefs.SetInt("ReturnFromPuzzle", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene("ControlRoom");
        }
    }

    void ResetAllCheckpoints()
    {
        // Clear saved checkpoint
        GameManager.Instance.ClearCheckpoint();

        // Reset checkpoint objects visually/logic
        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.ResetCheckpoint();
        }

        Debug.Log("All checkpoints reset.");
    }
}
