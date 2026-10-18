using UnityEngine;
using System.Collections;

/// <summary>
/// A fast-moving plasma orb that travels along a set path once.
/// Deals damage and freezes the player on contact.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlasmaOrb : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("How fast the orb moves between points.")]
    public float moveSpeed = 5f;
    [Tooltip("Damage dealt to the player on hit.")]
    public int damage = 15;
    [Tooltip("How long the player is frozen on hit.")]
    public float freezeDuration = 0.5f;

    [Header("Visuals & Audio")]
    [Tooltip("How fast the orb rotates (degrees per second). 180 is a good start.")]
    public float rotationSpeed = 180f; 
    [Tooltip("Sound to play on contact with the player.")]
    public AudioClip zapSound;
    
    [Tooltip("Sound to play when the orb reaches the end of its path.")]
    public AudioClip despawnSound;
    [Tooltip("Volume for the despawn sound.")]
    [Range(0f, 1f)]
    public float despawnVolume = 0.8f;
    
    [Header("Proximity Warning (NEW)")]
    [Tooltip("Sound clip for the looping proximity warning (e.g., high-pitched whine).")]
    public AudioClip warningSound;
    [Tooltip("Maximum distance the warning sound can be heard from.")]
    public float maxHearingDistance = 15f; 
    [Tooltip("Maximum volume for the warning sound.")]
    [Range(0f, 1f)]
    public float warningVolume = 0.4f;

    // --- Private State ---
    private Transform[] path;
    private int currentWaypointIndex = 0;
    private bool isPlayerHit = false;
    private AudioSource warningSource; // Dedicated Audio Source for the loop

    void Awake()
    {
        // Orb must be a trigger to hit the player without pushing them
        GetComponent<Collider2D>().isTrigger = true;
        
        // --- Setup Dedicated Audio Source for Proximity ---
        warningSource = gameObject.AddComponent<AudioSource>();
        warningSource.playOnAwake = false;
        warningSource.loop = true; // The warning sound should loop
        warningSource.spatialBlend = 1.0f; // Enable 3D sound (volume based on distance)
        warningSource.volume = warningVolume;
        
        // Initial setup for 3D sound attenuation (Rolloff)
        warningSource.maxDistance = maxHearingDistance;
        warningSource.rolloffMode = AudioRolloffMode.Linear; // Linear fade is best for a predictable hazard
    }
    
    /// <summary>
    /// Called by the PlasmaSpawner to assign the path.
    /// </summary>
    public void SetPath(Transform[] newPath)
    {
        path = newPath;
        currentWaypointIndex = 1; 
        
        // --- Start the looping warning sound immediately ---
        if (warningSource != null && warningSound != null)
        {
            warningSource.clip = warningSound;
            warningSource.Play();
        }
    }

    void Update()
    {
        // 1. Handle Movement
        if (path != null && path.Length > currentWaypointIndex)
        {
            MoveToNextWaypoint();
        }

        // 2. Handle Rotation
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    private void MoveToNextWaypoint()
    {
        // The target is the position of the current index waypoint
        Vector3 targetPosition = path[currentWaypointIndex].position;

        // Move towards the target position smoothly
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );

        // Check if the orb has arrived at the waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // If we hit the last point in the array (the Despawn Point)...
            if (currentWaypointIndex >= path.Length - 1)
            {
                // --- DESPAWN LOGIC ---
                DespawnOrb(); // Call the unified despawn method
                return;
            }
            
            // Move to the next waypoint
            currentWaypointIndex++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isPlayerHit)
        {
            isPlayerHit = true; 

            // Get player scripts
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            PlayerEffects playerEffects = collision.GetComponent<PlayerEffects>();

            // Apply hazard effects
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            if (playerEffects != null)
            {
                playerEffects.ApplyFreeze(freezeDuration);
            }

            // Play zap sound effect (one-shot, high priority)
            if(zapSound != null)
            {
                AudioSource.PlayClipAtPoint(zapSound, transform.position, 1.0f);
            }
            
            // Stop the warning loop immediately
            if (warningSource != null) warningSource.Stop();

            // Destroy the orb on hit (we rely on zapSound for feedback)
            Destroy(gameObject); 
        }
    }
    
    /// <summary>
    /// Handles playing the despawn sound and destroying the orb.
    /// Called when the orb reaches the final waypoint.
    /// </summary>
    private void DespawnOrb()
    {
        // 1. Play the despawn sound as a one-shot, since the object is about to be destroyed.
        if (despawnSound != null)
        {
            AudioSource.PlayClipAtPoint(despawnSound, transform.position, despawnVolume);
        }
        
        // 2. Stop the looping warning sound immediately if it was still playing.
        if (warningSource != null) 
        {
            warningSource.Stop();
        }
        
        // 3. Destroy the orb object
        Destroy(gameObject);
    }
}