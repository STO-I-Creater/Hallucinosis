using UnityEngine;

public class OpeningCutscene : MonoBehaviour
{
    [Header("References")]
    public Animator doorAnimator;          // Door's Animator
    public Transform player;               // Player transform
    public SpriteRenderer playerRenderer;  // Player SpriteRenderer
    public PlayerMovement playerMovement;  // PlayerMovement script

    [Header("Sorting Layers")]
    public string startSortingLayer = "Background";
    public string endSortingLayer = "Player";

    [Header("Movement Settings")]
    public float moveSpeed = 2f;           // Player movement speed
    public float moveDistance = 2f;        // Distance to move forward

    private bool moving = false;
    private float targetX;

    void Start()
    {
        // Set initial player sorting layer
        if (playerRenderer != null)
            playerRenderer.sortingLayerName = startSortingLayer;

        // Disable player control during cutscene
        SetControl(false);

        // Play door opening animation
        if (doorAnimator != null)
            doorAnimator.Play("DoorOpen");

        // Start walking out after 0.5 seconds
        Invoke(nameof(StartWalking), 0.5f);
    }

    void StartWalking()
    {
        targetX = player.position.x + moveDistance;
        moving = true;

        // Flip player to face right (adjust if walking left)
        if (playerRenderer != null)
            playerRenderer.flipX = false; // false = face right, true = face left
    }

    void Update()
    {
        if (!moving) return;

        // Move player horizontally
        Vector3 pos = player.position;
        pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeed * Time.deltaTime);
        player.position = pos;

        // Reached target
        if (Mathf.Abs(player.position.x - targetX) < 0.01f)
        {
            moving = false;

            // Change sorting layer to "Player"
            if (playerRenderer != null)
                playerRenderer.sortingLayerName = endSortingLayer;

            // Play door closing animation
            if (doorAnimator != null)
                doorAnimator.Play("DoorClose");

            // Enable player control after door closes
            Invoke(nameof(EnableControl), 0.3f);
        }
    }

    void EnableControl()
    {
        SetControl(true);
    }

    // Access private "canControl" in PlayerMovement via reflection
    void SetControl(bool state)
    {
        var field = typeof(PlayerMovement).GetField(
            "canControl",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field != null)
            field.SetValue(playerMovement, state);
    }
}
