using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script goes on the Shockwave prefab.
/// It EXPANDS, and when it hits the player, it applies the stutter-freeze effect.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Shockwave : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("How fast the shockwave expands its scale.")]
    public float expansionSpeed = 8f; // RENAMED from moveSpeed
    [Tooltip("How long (in seconds) the shockwave exists before disappearing.")]
    public float lifeTime = 3f;

    [Header("Effect Settings")]
    [Tooltip("Damage to deal on hit.")]
    public int damage = 10;
    [Tooltip("How long each freeze pulse lasts.")]
    public float freezeDuration = 0.5f;
    [Tooltip("How long the player is free between pulses.")]
    public float waitDuration = 1.0f;
    [Tooltip("How many freeze pulses to apply.")]
    public int stutterCount = 3;

    // NEW: List to track objects we've already hit
    private List<Collider2D> hitObjects;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        // NEW: Initialize the list
        hitObjects = new List<Collider2D>();
    }

    void Start()
    {
        // Automatically destroy this object after 'lifeTime' seconds
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // --- UPDATED LOGIC ---
        // We no longer move, we scale the object.
        // This makes the shockwave expand outwards from its spawn point.
        transform.localScale += new Vector3(expansionSpeed, expansionSpeed, 0) * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // --- UPDATED LOGIC ---
        // Check if we've already hit this object
        if (hitObjects.Contains(collision))
        {
            return; // Already hit, do nothing
        }

        if (collision.CompareTag("Player"))
        {
            // Find the player's effect script
            PlayerEffects playerEffects = collision.GetComponent<PlayerEffects>();

            if (playerEffects != null)
            {
                // NEW: Add player to the hit list so they don't get hit again
                hitObjects.Add(collision);

                // Apply the full, complex effect
                playerEffects.ApplyStutterFreeze(damage, freezeDuration, waitDuration, stutterCount);
            }

            // --- UPDATED LOGIC ---
            // We NO LONGER destroy the shockwave on hit.
            // It should continue expanding to hit other things (or just look cool).
            // It will be destroyed by its 'lifeTime' in the Start() function.
            // Destroy(gameObject); // REMOVED
        }
    }
}