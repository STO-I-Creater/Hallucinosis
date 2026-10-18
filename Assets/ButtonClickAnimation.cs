using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Needed for scene loading

public class ButtonClickWithSceneLoad : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite clickedSprite;
    public float animationTime = 0.15f; // How long to show clicked sprite
    public string sceneToLoad; // Name of the scene to load

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

        // Wait in real time so it works even if Time.timeScale = 0
        yield return new WaitForSecondsRealtime(animationTime);

        // Revert sprite
        img.sprite = normalSprite;

        // Load the chosen scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Time.timeScale = 1f; // Ensure game is running normally before loading
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No scene specified to load!");
        }
    }
}
