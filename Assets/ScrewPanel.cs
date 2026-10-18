using UnityEngine;

public class VentPuzzle : MonoBehaviour
{
    public GameObject ventPanel;     // UI vent cover
    public Animator anim1;
    public Animator anim2;
    public Animator anim3;

    private int removedScrews = 0;
    public int totalScrews = 4;

    public void ScrewRemoved()
    {
        removedScrews++;

        if (removedScrews >= totalScrews)
        {
            OpenVent();
        }
    }

    void OpenVent()
    {
        ventPanel.SetActive(false);

        anim1.Play("Anim1");
        anim2.Play("Anim2");
        anim3.Play("Anim3");
    }
}
