using UnityEngine;

public class QuitButton : MonoBehaviour
{
    // Call this method on button click
    public void OnClickQuit()
    {
        #if UNITY_EDITOR
            // Stop play mode if in the editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Quit the application
            Application.Quit();
        #endif
    }
}
