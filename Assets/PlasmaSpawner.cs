using UnityEngine;
using System.Collections;

/// <summary>
/// This script manages the continuous spawning of PlasmaOrbs along a predefined path,
/// synchronized with the spawner's animation cycle.
///
/// NOTE: The Animator component must be on THIS GameObject (the parent).
/// </summary>
public class PlasmaSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The PlasmaOrb Prefab to spawn.")]
    public PlasmaOrb plasmaOrbPrefab;
    [Tooltip("The time (in seconds) between each orb spawn (This should match animation length).")]
    public float spawnRate = 3f; // Now used for initial timing if no animation is set

    [Header("Movement Path (Max 7 Waypoints)")]
    [Tooltip("The path the spawned orb will follow (Waypoint 0 is the Spawn Point).")]
    public Transform[] pathWaypoints = new Transform[7]; 
    
    // --- Private References ---
    private Animator spawnerAnimator;
    private AudioSource audioSource;
    
    void Awake()
    {
        // --- FIX: Get Animator and Sprite Renderer directly from THIS Parent Object ---
        spawnerAnimator = GetComponent<Animator>();
        
        if (spawnerAnimator == null)
        {
            Debug.LogError("Plasma Spawner requires an Animator component on the same GameObject!");
        }

        // Setup AudioSource for charging sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        if (plasmaOrbPrefab == null || pathWaypoints.Length == 0 || pathWaypoints[0] == null)
        {
            Debug.LogError("Plasma Spawner is missing required references!");
            return;
        }
        
        // --- Start the cycle immediately ---
        StartCoroutine(SpawnCycleCoroutine());
    }
    
    /// <summary>
    /// The coroutine that manages the animation states and timing.
    /// </summary>
    private IEnumerator SpawnCycleCoroutine()
    {
        while (true)
        {
            // --- 1. IDLE STATE ---
            if (spawnerAnimator != null) spawnerAnimator.SetBool("IsCharging", false);
            
            // Wait for the duration of the 'TeslaOff' or 'Idle' animation clip.
            // FIX: Use WaitForSecondsRealtime
            yield return new WaitForSecondsRealtime(spawnRate); 
            
            // --- 2. CHARGING STATE ---
            if (spawnerAnimator != null) spawnerAnimator.SetBool("IsCharging", true);
            
            // Wait for the animation to play once.
            float clipLength = GetClipLength(spawnerAnimator, "ChargeUp"); 
            
            if (clipLength > 0.01f)
            {
                 // We wait for the animation duration. The animation event fires right at the end of this.
                 // FIX: Use WaitForSecondsRealtime
                 yield return new WaitForSecondsRealtime(clipLength + 0.05f);
            }
            else
            {
                // Fallback if we can't find the clip length (use timer instead)
                // FIX: Use WaitForSecondsRealtime
                yield return new WaitForSecondsRealtime(spawnRate / 2f);
            }
        }
    }
    
    /// <summary>
    /// Finds the length of a clip by name (essential for accurate timing).
    /// </summary>
    private float GetClipLength(Animator animator, string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0f;
        
        // Ensure the Animator Controller has clips loaded
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        
        foreach (var clip in clips)
        {
            // Note: This relies on the clip name in the PROJECT, not the state name in the Animator
            if (clip.name == clipName) 
            {
                return clip.length;
            }
        }
        return 0f;
    }


    /// <summary>
    /// CRITICAL FUNCTION: This is called directly by the Animation Event.
    /// </summary>
    public void AnimationEvent_ReleaseOrb()
    {
        if (pathWaypoints.Length == 0 || pathWaypoints[0] == null) return;
        
        // 1. Spawn the Orb at the first waypoint (Spawn Point)
        PlasmaOrb newOrb = Instantiate(plasmaOrbPrefab, pathWaypoints[0].position, Quaternion.identity);

        // 2. Tell the new Orb which path to follow
        newOrb.SetPath(pathWaypoints);
        
        // 3. Immediately switch the animator back to IDLE state to restart the cycle wait.
        if (spawnerAnimator != null) spawnerAnimator.SetBool("IsCharging", false);
    }
}