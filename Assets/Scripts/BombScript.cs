using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int damage = 50;
    public float explosionForce = 5f;
    public GameObject explosionEffect;

    // --- Script References ---
    private PlayerHealth PlayerHealthScript;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Damage the player
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Optional: small knockback force
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            }

            // Optional: explosion VFX
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject); // destroy the bomb after triggering
        }
    }
}