using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    [Header("Player Tag / Name")]
    public string playerTag = "Player"; // Make sure your Player has this tag
    public string playerName = "Player"; // Or use the object name if you prefer

    private GameObject player;
    
    void Start()
    {
        CheckPlayerStatus();
    }

    void CheckPlayerStatus()
    {
        // Find player by tag
        player = GameObject.FindWithTag(playerTag);

        if (player == null)
        {
            Debug.LogError("[PLAYER DEBUG] Player not found in scene!");
            return;
        }

        Debug.Log("[PLAYER DEBUG] Player found: " + player.name);

        // Check if active
        Debug.Log("[PLAYER DEBUG] ActiveSelf = " + player.activeSelf);
        Debug.Log("[PLAYER DEBUG] ActiveInHierarchy = " + player.activeInHierarchy);

        // Check SpriteRenderer
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        Debug.Log("[PLAYER DEBUG] SpriteRenderer exists = " + (sr != null));
        if (sr != null) Debug.Log("[PLAYER DEBUG] SpriteRenderer enabled = " + sr.enabled);

        // Check Rigidbody2D
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Debug.Log("[PLAYER DEBUG] Rigidbody2D exists = " + (rb != null));
        if (rb != null) Debug.Log("[PLAYER DEBUG] Rigidbody2D position = " + rb.position);

        // Check Collider2D
        Collider2D col = player.GetComponent<Collider2D>();
        Debug.Log("[PLAYER DEBUG] Collider2D exists = " + (col != null));
        if (col != null) Debug.Log("[PLAYER DEBUG] Collider2D enabled = " + col.enabled);

        // Check PlayerHealth
        var health = player.GetComponent<PlayerHealth>();
        Debug.Log("[PLAYER DEBUG] PlayerHealth exists = " + (health != null));
        if (health != null)
        {
            Debug.Log("[PLAYER DEBUG] currentHealth = " + health.currentHealth);
            Debug.Log("[PLAYER DEBUG] maxHealth = " + health.maxHealth);
        }

        // Check Animator
        Animator anim = player.GetComponent<Animator>();
        Debug.Log("[PLAYER DEBUG] Animator exists = " + (anim != null));
        if (anim != null) Debug.Log("[PLAYER DEBUG] Animator enabled = " + anim.enabled);

        Debug.Log("[PLAYER DEBUG] --- CHECK COMPLETE ---");
    }
}
