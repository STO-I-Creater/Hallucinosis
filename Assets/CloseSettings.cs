using UnityEngine;

public class ClosePanelButton : MonoBehaviour
{
    public GameObject panelToClose; // The panel to close

    // Call this on button click
    public void OnClickClose()
    {
        if(panelToClose != null)
            panelToClose.SetActive(false);
        else
            Debug.LogWarning("Panel not assigned!");
    }
}
