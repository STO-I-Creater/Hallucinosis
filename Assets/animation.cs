using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed for scene loading
using System.Collections;

public class Animation1 : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite clickedSprite;
    public float animationTime = 0.15f; // how long to show clicked sprite
    public string sceneToLoad; // name of the scene to load
    public float loadDelay = 0.5f; // delay before loading scene

    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        img.sprite = normalSprite;
    }

    public void OnClick()
    {
        StartCoroutine(ClickAnimationAndLoadScene());
    }

    private IEnumerator ClickAnimationAndLoadScene()
    {
        // Change to clicked sprite
        img.sprite = clickedSprite;

        // Wait in real time for the click animation
        yield return new WaitForSecondsRealtime(animationTime);

        // Revert to normal sprite
        img.sprite = normalSprite;

        // Wait a bit more before loading the scene
        yield return new WaitForSecondsRealtime(loadDelay);

        // Load the specified scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
