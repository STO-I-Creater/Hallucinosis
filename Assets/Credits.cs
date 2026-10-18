using UnityEngine;

public class CreditsScroll : MonoBehaviour
{
    public RectTransform creditsText;   // The credits text
    public float speed = 50f;           // Scrolling speed

    private Vector2 startPos;
    private Vector2 endPos;

    void OnEnable()
    {
        // Start above the visible area
        startPos = new Vector2(creditsText.anchoredPosition.x, 500f);

        // End below the mask
        endPos = new Vector2(creditsText.anchoredPosition.x, -1200f);

        creditsText.anchoredPosition = startPos;
    }

    void Update()
    {
        // Move DOWN only (top → bottom)
        creditsText.anchoredPosition = new Vector2(
            startPos.x,
            creditsText.anchoredPosition.y - speed * Time.deltaTime
        );
    }
}
