using UnityEngine;

/// <summary>
/// Place this on a "steam vent" or "wind" object.
/// It will apply a continuous force in a fixed direction (LEFT)
/// to any Rigidbody2D that enters its trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SteamVent : MonoBehaviour
{
    [Header("Vent Settings")]
    // Renamed for clarity, since it no longer pushes "upward"
    [Tooltip("The strength of the push. Start with 50-100.")]
    public float pushForce = 50f; 

    void Awake()
    {
        // Automatically set the collider to be a trigger
        GetComponent<Collider2D>().isTrigger = true;
    }

    /// <summary>
    // This function is called every physics frame for every collider
    // that is inside this trigger.
    /// </summary>
    private void OnTriggerStay2D(Collider2D collision)
    {
        // Find the Rigidbody of whatever entered the trigger.
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        // If the object has a Rigidbody (like the player or a box)...
        if (rb != null)
        {
            // --- FIX: Use Vector2.left for World-Space Left Push ---
            // Vector2.left is shorthand for new Vector2(-1, 0)
            rb.AddForce(Vector2.left * pushForce, ForceMode2D.Force);
        }
    }
}