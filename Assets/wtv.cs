using UnityEngine;

public class wtv : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    
    {
    // Reset puzzle completion when entering the main menu
    PlayerPrefs.SetInt("UniversalPuzzleCompleted", 0);
    PlayerPrefs.SetInt("ReturnFromPuzzle", 0);
    PlayerPrefs.Save();
    Debug.Log("Reset UniversalPuzzleCompleted to 0 at Main Menu");
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
