using System.Collections;
using UnityEngine;

/// <summary>
/// This is the "brain" that spawns a shockwave prefab at a regular interval.
/// Place this on a 'ShockwaveTower' GameObject.
/// </summary>
public class ShockwaveTower : MonoBehaviour
{
    [Header("Shockwave Settings")]
    [Tooltip("The shockwave prefab to spawn.")]
    public GameObject shockwavePrefab;

    [Tooltip("The point where the shockwave will be spawned.")]
    public Transform spawnPoint;

    [Tooltip("The time (in seconds) between each shockwave.")]
    public float fireInterval = 5f;

    void Start()
    {
        if (shockwavePrefab == null)
        {
            Debug.LogError("Shockwave Tower has no shockwave prefab assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Shockwave Tower has no spawn point assigned!");
            return;
        }

        // Start the firing loop
        StartCoroutine(FireCoroutine());
    }

    private IEnumerator FireCoroutine()
    {
        // Loop forever
        while (true)
        {
            // Wait for the interval (e.g., 5 seconds)
            yield return new WaitForSeconds(fireInterval);

            // TODO: You could play a "power up" sound or animation here

            // Spawn the shockwave
            Instantiate(shockwavePrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
