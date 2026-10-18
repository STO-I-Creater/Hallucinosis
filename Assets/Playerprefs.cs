using UnityEngine;

public class PlayerPrefsTracker : MonoBehaviour
{
    int lastValue;

    void Start()
    {
        lastValue = PlayerPrefs.GetInt("PuzzleCompleted", 0);
        Debug.Log("START → PuzzleCompleted = " + lastValue);
    }

    void Update()
    {
        int current = PlayerPrefs.GetInt("PuzzleCompleted", 0);
        if (current != lastValue)
        {
            Debug.LogError("⚠️ PuzzleCompleted CHANGED! NEW VALUE = " + current);
            lastValue = current;
        }
    }
}
