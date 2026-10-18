using System.Collections;
using UnityEngine;

/// <summary>
/// Manages status effects on the player, like poison, slows, and freezes.
/// Place this on the Player GameObject.
/// </summary>
public class PlayerEffects : MonoBehaviour
{
    // --- Script References ---
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerWallSlide playerWallSlide;
    private Rigidbody2D rb;

    // --- State ---
    private float originalMoveSpeed;
    private Coroutine activeAcidCoroutine; // Tracks the lingering (fire-and-forget) damage
    private Coroutine activeContinuousAcidCoroutine; // Tracks the continuous damage
    private Coroutine activeFreezeCoroutine;
    private Coroutine activeStutterCoroutine;

    void Awake()
    {
        // Get references to the other scripts on this player
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerWallSlide = GetComponent<PlayerWallSlide>();
        rb = GetComponent<Rigidbody2D>();

        if (playerMovement != null)
        {
            originalMoveSpeed = playerMovement.moveSpeed;
        }
    }

    /// <summary>
    /// This is the OLD "fire-and-forget" poison. Used for lingering effect.
    /// </summary>
    public void ApplyAcidEffect(int dps, float slowMultiplier, float duration)
    {
        // If the player is already poisoned, stop the old poison effect.
        if (activeAcidCoroutine != null)
        {
            StopCoroutine(activeAcidCoroutine);
            if (playerMovement != null) // Safety check
            {
                playerMovement.moveSpeed = originalMoveSpeed;
            }
        }

        activeAcidCoroutine = StartCoroutine(AcidEffectCoroutine(dps, slowMultiplier, duration));
    }

    /// <summary>
    /// Called by AcidHazard when the player ENTERS the trigger.
    /// Starts the slow and the damage-over-time coroutine.
    /// </summary>
    public void StartContinuousAcid(int dps, float slowMultiplier)
    {
        // If already in the continuous acid zone, don't restart.
        if (activeContinuousAcidCoroutine != null) return;

        Debug.Log("Player entered acid zone. Applying continuous effect.");

        // 1. Apply slow (but only if we aren't already poisoned by lingering effect)
        if (playerMovement != null && activeAcidCoroutine == null)
        {
            playerMovement.moveSpeed = originalMoveSpeed * slowMultiplier;
        }

        // 2. Start the continuous damage coroutine
        activeContinuousAcidCoroutine = StartCoroutine(ContinuousAcidCoroutine(dps));
    }

    /// <summary>
    /// Called by AcidHazard when the player EXITS the trigger.
    /// Stops the continuous damage, but starts the 5-second lingering poison.
    /// </summary>
    public void StopContinuousAcid(int dps, float slowMultiplier, float duration)
    {
        // If not in acid, do nothing.
        if (activeContinuousAcidCoroutine == null) return;

        Debug.Log("Player left acid zone. Starting 5-second lingering poison.");

        // 1. Stop the continuous damage coroutine
        StopCoroutine(activeContinuousAcidCoroutine);
        activeContinuousAcidCoroutine = null;

        // 2. Start the lingering (fire-and-forget) effect
        ApplyAcidEffect(dps, slowMultiplier, duration);
    }

    /// <summary>
    /// NEW: Called by PlayerHealth.Respawn() to immediately stop ALL running effects and restore player speed.
    /// </summary>
    public void ForceCleanse()
    {
        // 1. Stop Lingering Acid
        if (activeAcidCoroutine != null)
        {
            StopCoroutine(activeAcidCoroutine);
            activeAcidCoroutine = null;
        }

        // 2. Stop Continuous Acid (if we died in the puddle)
        if (activeContinuousAcidCoroutine != null)
        {
            StopCoroutine(activeContinuousAcidCoroutine);
            activeContinuousAcidCoroutine = null;
        }
        
        // 3. Stop Freeze
        if (activeFreezeCoroutine != null)
        {
            StopCoroutine(activeFreezeCoroutine);
            activeFreezeCoroutine = null;
        }

        // 4. Stop Stutter Freeze
        if (activeStutterCoroutine != null)
        {
            StopCoroutine(activeStutterCoroutine);
            activeStutterCoroutine = null;
        }

        // 5. Restore Speed (Must be done manually, as the coroutines didn't finish normally)
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = originalMoveSpeed;
        }
    }

    /// <summary>
    /// Coroutine for the OLD "fire-and-forget" poison.
    /// </summary>
    private IEnumerator AcidEffectCoroutine(int dps, float slowMultiplier, float duration)
    {
        Debug.Log("Acid effect started!");

        // Only slow the player if they aren't already being slowed by the continuous zone
        if (playerMovement != null && activeContinuousAcidCoroutine == null) 
        {
            playerMovement.moveSpeed = originalMoveSpeed * slowMultiplier;
        }

        float timer = 0f;
        float damageTick = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            damageTick += Time.deltaTime;

            if (damageTick >= 1.0f)
            {
                damageTick -= 1.0f;
                if (playerHealth != null)
                {
                    Debug.Log("Player takes " + dps + " lingering acid damage.");
                    playerHealth.TakeDamage(dps);
                }
            }

            yield return null;
        }

        Debug.Log("Acid effect wore off.");
        // Only restore speed if we are not currently in the continuous acid zone
        if (playerMovement != null && activeContinuousAcidCoroutine == null)
        {
            playerMovement.moveSpeed = originalMoveSpeed;
        }
        activeAcidCoroutine = null;
    }

    /// <summary>
    /// NEW: Coroutine that runs FOREVER while the player is in the acid.
    /// It gets stopped by the StopContinuousAcid() function.
    /// </summary>
    private IEnumerator ContinuousAcidCoroutine(int dps)
    {
        float damageTick = 1.0f; // Start at 1 so we get damaged immediately

        while (true)
        {
            damageTick += Time.deltaTime;

            if (damageTick >= 1.0f)
            {
                damageTick -= 1.0f;
                if (playerHealth != null)
                {
                    Debug.Log("Player takes " + dps + " continuous acid damage.");
                    playerHealth.TakeDamage(dps);
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Called by hazards (like Tesla Coils) to freeze the player.
    /// </summary>
    public void ApplyFreeze(float duration)
    {
        // If already frozen, refresh the freeze.
        if (activeFreezeCoroutine != null)
        {
            StopCoroutine(activeFreezeCoroutine);
        }

        activeFreezeCoroutine = StartCoroutine(FreezeCoroutine(duration));
    }

    /// <summary>
    /// Called by the Shockwave Tower to apply damage and a pulsing freeze effect.
    /// </summary>
    public void ApplyStutterFreeze(int damage, float freezeDuration, float waitDuration, int stutterCount)
    {
        // If already stuttering, stop the old one
        if (activeStutterCoroutine != null)
        {
            StopCoroutine(activeStutterCoroutine);
        }

        // Also stop any regular freezes
        if (activeFreezeCoroutine != null)
        {
            StopCoroutine(activeFreezeCoroutine);
        }

        activeStutterCoroutine = StartCoroutine(StutterFreezeCoroutine(damage, freezeDuration, waitDuration, stutterCount));
    }

    /// <summary>
    /// Coroutine that handles the simple freeze effect.
    /// </summary>
    private IEnumerator FreezeCoroutine(float duration)
    {
        Debug.Log("Player frozen!");

        // 1. Stop all momentum
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 2. Disable movement scripts
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerWallSlide != null) playerWallSlide.enabled = false;

        // 3. Wait for the duration
        yield return new WaitForSeconds(duration);

        Debug.Log("Freeze wore off.");

        // 4. Re-enable scripts ONLY IF ALIVE
        if (playerHealth != null && playerHealth.currentHealth > 0)
        {
            if (playerMovement != null) playerMovement.enabled = true;
            if (playerWallSlide != null) playerWallSlide.enabled = true;
        }
        else
        {
            Debug.Log("Freeze wore off, but player is dead. Not re-enabling controls.");
        }

        activeFreezeCoroutine = null; // Mark as no longer frozen
    }

    /// <summary>
    /// Coroutine that handles the pulsing stutter-freeze effect.
    /// </summary>
    private IEnumerator StutterFreezeCoroutine(int damage, float freezeDuration, float waitDuration, int stutterCount)
    {
        Debug.Log("Stutter-freeze effect started!");

        // 1. Apply initial damage
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        // 2. Loop for the number of stutters
        for (int i = 0; i < stutterCount; i++)
        {
            // --- FREEZE ---
            Debug.Log($"Stutter freeze pulse {i + 1} START");
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (playerMovement != null) playerMovement.enabled = false;
            if (playerWallSlide != null) playerWallSlide.enabled = false;

            yield return new WaitForSeconds(freezeDuration); // e.g., 0.5 seconds

            // --- UNFREEZE ---
            Debug.Log($"Stutter freeze pulse {i + 1} END");
            if (playerHealth != null && playerHealth.currentHealth > 0)
            {
                if (playerMovement != null) playerMovement.enabled = true;
                if (playerWallSlide != null) playerWallSlide.enabled = true;
            }

            // --- WAIT (if not the last pulse) ---
            if (i < stutterCount - 1)
            {
                yield return new WaitForSeconds(waitDuration); // e.g., 1.0 second
            }
        }

        Debug.Log("Stutter-freeze effect finished.");
        activeStutterCoroutine = null;
    }
}