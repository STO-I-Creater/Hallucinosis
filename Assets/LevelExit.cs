using UnityEngine;
using System.Collections; // Required for IEnumerator and StartCoroutine
using UnityEngine.SceneManagement;

/// <summary>
/// Place this on the final trigger zone (door, portal, etc.) to signal stage completion.
/// Plays a sound when the player enters the zone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    [Header("End of Stage")]
    [Tooltip("The sound clip to play upon completion.")]
    public AudioClip celebrationSound;
    [Tooltip("Time delay before re-enabling player control.")]
    public float loadDelay = 2.0f; // Kept as a buffer, but not used for pausing

    private bool hasExited = false;
    
    void Awake()
    {
        // Ensures the collider acts as a trigger zone, not a solid wall.
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !hasExited)
        {
            hasExited = true;
            
            // 1. Play Sound (Only action needed)
            if (celebrationSound != null)
            {
                // Play sound as a one-shot so it finishes even if the player leaves the area
                AudioSource.PlayClipAtPoint(celebrationSound, transform.position);
            }

            // 2. We remove all movement stopping logic (playerMovementRef.enabled = false, rb.velocity = 0)
            //    The player continues moving through the exit.
        }
    }
    
    // REMOVED CleanupExit() coroutine entirely.
}