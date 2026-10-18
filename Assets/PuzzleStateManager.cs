using UnityEngine;

public static class PuzzleStateManager
{
    private const string PuzzleKey = "PuzzleCompleted";
    private const string ReturnKey = "ReturnFromPuzzle";

    /// <summary>
    /// Check if the puzzle is completed.
    /// </summary>
    public static bool IsPuzzleCompleted()
    {
        return PlayerPrefs.GetInt(PuzzleKey, 0) == 1;
    }

    /// <summary>
    /// Mark the puzzle as completed.
    /// </summary>
    public static void MarkPuzzleCompleted()
    {
        PlayerPrefs.SetInt(PuzzleKey, 1);
        PlayerPrefs.SetInt(ReturnKey, 1);
        PlayerPrefs.Save();
        Debug.Log("PuzzleStateManager: Puzzle marked as completed.");
    }

    /// <summary>
    /// Reset the puzzle state (for testing or retrying).
    /// </summary>
    public static void ResetPuzzle()
    {
        PlayerPrefs.DeleteKey(PuzzleKey);
        PlayerPrefs.DeleteKey(ReturnKey);
        PlayerPrefs.Save();
        Debug.Log("PuzzleStateManager: Puzzle state reset.");
    }

    /// <summary>
    /// Debug helper: print current puzzle state to console.
    /// </summary>
    public static void PrintPuzzleState()
    {
        bool completed = IsPuzzleCompleted();
        Debug.Log($"PuzzleStateManager: Puzzle completed? {completed}");
    }
}
