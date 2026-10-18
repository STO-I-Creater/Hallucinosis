using UnityEngine;

/// <summary>
/// Creates a wall that is solid for jumping (WallSlide) but allows
/// the player to walk through if they are grounded (Hidden Path).
/// </summary>
public class IntangibleWall : MonoBehaviour
{
    [Header("Visibility")]
    [Tooltip("The Renderers of the wall and room elements that should disappear.")]
    public Renderer[] hiddenRenderers; 

    [Header("Behavior Thresholds")]
    [Tooltip("If player's Y velocity is below this, the wall becomes solid for sliding.")]
    public float solidFallThreshold = -0.1f;
    [Tooltip("If player's Y velocity is above this, the wall becomes solid for jumping.")]
    public float solidJumpThreshold = 0.5f;
    
    [Header("Overlap Check (New System)")]
    [Tooltip("The point in world space where the player must be located to reveal the room.")]
    public Transform revealPoint;
    [Tooltip("The range around the Reveal Point to check for the player.")]
    public float revealRadius = 1f;
    
    [Header("Feedback")]
    public AudioClip solidSound;

    // --- Private References ---
    private Collider2D wallCollider;
    private PlayerMovement playerMovement;
    private bool roomIsVisible = false;

    void Awake()
    {
        wallCollider = GetComponent<Collider2D>();
        
        // Wall starts solid and hidden
        if (wallCollider != null)
        {
            wallCollider.isTrigger = false;
        }

        SetVisibility(false);
    }

    private void Update()
    {
        // 1. Get Player Reference
        if (playerMovement == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerMovement = playerObj.GetComponent<PlayerMovement>();
            }
            return;
        }
        
        // 2. Control Wall Solidity
        if (wallCollider != null)
        {
            ControlWallSolidity();
        }

        // 3. Control Room Visibility
        ControlRoomVisibility();
    }
    
    /// <summary>
    /// Checks player velocity/ground status and toggles the wall's solidity.
    /// </summary>
    private void ControlWallSolidity()
    {
        float velocityY = playerMovement.rb.linearVelocity.y;
        
        if (playerMovement.IsGrounded && velocityY > solidFallThreshold && velocityY < solidJumpThreshold)
        {
            // Intent: WALKING (Wall must be pass-through)
            if (wallCollider.isTrigger == false)
            {
                wallCollider.isTrigger = true; // Make it intangible
                Debug.Log("Wall: INTANGIBLE (Walk Through)");
            }
        }
        else if (!playerMovement.IsGrounded && (velocityY < solidFallThreshold || velocityY > solidJumpThreshold))
        {
            // Intent: JUMPING UP or FALLING/SLIDING DOWN (Wall must be solid)
            if (wallCollider.isTrigger == true)
            {
                wallCollider.isTrigger = false; // Make it SOLID
                if (solidSound != null) AudioSource.PlayClipAtPoint(solidSound, transform.position);
                Debug.Log("Wall: SOLID (Wall Jump Ready)");
            }
        }
    }
    
    /// <summary>
    /// Checks the new Reveal Point to show/hide the room.
    /// </summary>
    private void ControlRoomVisibility()
    {
        if (revealPoint == null) return;
        
        // Check if the Player's Collider overlaps the defined point/radius
        // Note: The player must be on the 'Player' layer for this to work.
        Collider2D playerOverlap = Physics2D.OverlapCircle(revealPoint.position, revealRadius, LayerMask.GetMask("Player"));

        if (playerOverlap != null && !roomIsVisible)
        {
            // Player entered the reveal zone
            SetVisibility(true);
            roomIsVisible = true;
        }
        else if (playerOverlap == null && roomIsVisible)
        {
            // Player left the reveal zone
            SetVisibility(false);
            roomIsVisible = false;
        }
    }

    /// <summary>
    /// Toggles the visibility of all assigned renderers (the wall and room).
    /// </summary>
    private void SetVisibility(bool visible)
    {
        foreach (Renderer r in hiddenRenderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }
}
