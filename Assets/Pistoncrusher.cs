using UnityEngine;
using System.Collections;

public class PistonSmasher : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("How fast the piston slams down.")]
    public float slamSpeed = 20f;
    [Tooltip("How fast the piston returns to its base.")]
    public float retractSpeed = 2f;

    [Header("Timing Settings")]
    [Tooltip("Wait time at the top before slamming.")]
    public float waitAtTop = 2f;
    [Tooltip("Wait time at the bottom before retracting.")]
    public float waitAtBottom = 1f;

    [Header("Movement & Combat")]
    [Tooltip("How far the piston travels from its starting point.")]
    public float travelDistance = 4f; 
    [Tooltip("Damage dealt to the player.")]
    public int damageAmount = 25;

    private Vector3 topPos;
    private Vector3 bottomPos;

    void Start()
    {
        // Set the points relative to the Parent (The Base)
        topPos = transform.localPosition;
        bottomPos = topPos + (Vector3.down * travelDistance);
        
        // Start the infinite loop
        StartCoroutine(SmasherRoutine());
    }

    IEnumerator SmasherRoutine()
    {
        while (true)
        {
            // 1. Wait at the top (Warning phase)
            yield return new WaitForSeconds(waitAtTop);

            // 2. SLAM DOWN
            while (Vector3.Distance(transform.localPosition, bottomPos) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, bottomPos, slamSpeed * Time.deltaTime);
                yield return null;
            }

            // 3. Wait at the bottom (Crush phase)
            yield return new WaitForSeconds(waitAtBottom);

            // 4. RETRACT UP
            while (Vector3.Distance(transform.localPosition, topPos) > 0.01f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, topPos, retractSpeed * Time.deltaTime);
                yield return null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Checks if the object entering the trigger has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Attempt to get the Health script from the player
            // NOTE: Ensure your player health script is named 'PlayerHealth'
            // or rename it here to match your specific script name.
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damageAmount);
                Debug.Log("Smasher hit player for " + damageAmount + " damage.");
            }
            else
            {
                Debug.LogWarning("Player hit, but no PlayerHealth script found!");
            }
        }
    }
}