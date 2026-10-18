using UnityEngine;

/// <summary>
/// This script goes on the ArcVisual object (the one with the collider).
/// It detects the player and tells the parent TeslaHazard to apply damage.
/// </summary>
public class TeslaArc : MonoBehaviour
{
    private TeslaHazard parentHazard;

    void Awake()
    {
        // Get the main script from the parent object
        parentHazard = GetComponentInParent<TeslaHazard>();
        if (parentHazard == null)
        {
            Debug.LogError("TeslaArc script can't find its parent TeslaHazard script!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Safety check and make sure we are tagged "Player"
        if (parentHazard == null || !collision.CompareTag("Player"))
        {
            return;
        }

        Debug.Log("Player zapped!");

        // Get the player's scripts
        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        PlayerEffects playerEffects = collision.GetComponent<PlayerEffects>();

        // --- UPDATED LOGIC ---
        // Instead of applying effects here, we just tell the parent
        // script to handle everything. The parent will do the time calculation.
        parentHazard.ApplyDamageAndFreeze(playerHealth, playerEffects);
    }
}