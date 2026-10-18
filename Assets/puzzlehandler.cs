using UnityEngine;

public class PuzzleEnabler : MonoBehaviour
{
    [Tooltip("The world trigger that will be allowed to start the puzzle.")]
    public PuzzleTrigger puzzleTrigger;

    public void EnablePuzzle()
    {
        if (puzzleTrigger == null)
        {
            Debug.LogWarning("PuzzleEnabler: puzzleTrigger not assigned.");
            return;
        }

        puzzleTrigger.EnableFromUI();
        Debug.Log("PuzzleEnabler: Called EnablePuzzle()");
    }
}
        