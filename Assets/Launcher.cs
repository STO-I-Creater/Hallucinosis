using UnityEngine;
using System.Collections;

public class LaunchPiston : MonoBehaviour
{
    [Header("Speed Settings")]
    public float launchSpeed = 30f; 
    public float retractSpeed = 2f;

    [Header("Timing Settings")]
    public float waitAtBottom = 2f;
    public float waitAtTop = 0.5f;

    [Header("Movement & Launch")]
    public float travelDistance = 2f; 
    public float launchForce = 20f; 

    private Vector3 bottomPos;
    private Vector3 topPos;
    
    // This tracks if we are currently in the "Slamming Up" phase
    private bool movingUp = false;

    void Start()
    {
        bottomPos = transform.localPosition;
        topPos = bottomPos + (Vector3.up * travelDistance);
        
        StartCoroutine(PistonRoutine());
    }

    IEnumerator PistonRoutine()
    {
        while (true)
        {
            movingUp = false;
            yield return new WaitForSeconds(waitAtBottom);

            // PHASE: MOVING UP
            movingUp = true; 
            while (Vector3.Distance(transform.localPosition, topPos) > 0.05f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, topPos, launchSpeed * Time.deltaTime);
                yield return null;
            }
            transform.localPosition = topPos; // Snap to top
            movingUp = false; 

            yield return new WaitForSeconds(waitAtTop);

            // PHASE: RETRACTING
            while (Vector3.Distance(transform.localPosition, bottomPos) > 0.05f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, bottomPos, retractSpeed * Time.deltaTime);
                yield return null;
            }
            transform.localPosition = bottomPos; // Snap to bottom
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only launch if the player enters the trigger WHILE we are moving up
        if (movingUp && other.CompareTag("Player"))
        {
            ApplyLaunch(other.gameObject);
        }
    }

    // This handles the case where the player is ALREADY standing on it when it starts moving
    private void OnTriggerStay2D(Collider2D other)
    {
        if (movingUp && other.CompareTag("Player"))
        {
            ApplyLaunch(other.gameObject);
        }
    }

    void ApplyLaunch(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Reset velocity for a clean launch
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * launchForce, ForceMode2D.Impulse);
            
            // Turn off movingUp for this specific hit so it doesn't double-launch
            movingUp = false; 
            Debug.Log("Piston Launched Player!");
        }
    }
}