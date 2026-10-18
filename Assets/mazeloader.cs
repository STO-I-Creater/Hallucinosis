using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMazeOnTouch2D : MonoBehaviour
{
    public GameObject playerPrefab;      // Player prefab to spawn in Maze
    public Vector3 spawnPosition = Vector3.zero;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Destroy the current player before loading new scene
            Destroy(collision.gameObject);

            // Reset all checkpoints in this scene
            ResetAllCheckpoints();

            // Reset puzzle progress if needed
            PlayerPrefs.SetInt("UniversalPuzzleCompleted", 1);
            PlayerPrefs.SetInt("ReturnFromPuzzle", 0);
            PlayerPrefs.Save();

            // Load Maze scene
            SceneManager.LoadScene("Maze");

            // Spawn player after scene load
            SceneManager.sceneLoaded += SpawnPlayerOnLoad;
        }
    }

    private void SpawnPlayerOnLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Maze" && playerPrefab != null)
        {
            Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        }

        SceneManager.sceneLoaded -= SpawnPlayerOnLoad;
    }

    private void ResetAllCheckpoints()
    {
        // Find all GameManagers in the current scene and clear them
        GameManager[] managers = FindObjectsOfType<GameManager>();
        foreach (var gm in managers)
        {
            gm.ClearCheckpoint();
        }

        // Reset all Checkpoint objects
        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();
        foreach (Checkpoint cp in allCheckpoints)
        {
            cp.ResetCheckpoint();
        }

        Debug.Log("All checkpoints cleared in this scene!");
    }
}
