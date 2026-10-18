using UnityEngine;

public class TerminalOption : MonoBehaviour
{
    // Called when the player selects an option on the tablet
    public void OnOptionSelected()
    {
        Debug.Log($"Option '{gameObject.name}' selected!");

        TerminalTrigger terminal = FindObjectOfType<TerminalTrigger>();
        if (terminal != null)
        {
            terminal.CloseTablet(); // Close tablet after clicking an option
        }
        else
        {
            Debug.LogWarning("⚠️ No TerminalTrigger found in !");
        }
    }
}
