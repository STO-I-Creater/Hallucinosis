using UnityEngine;

public class Cover : MonoBehaviour
{
    [Header("Cover Settings")]
    public Transform coverSpot;   // Where the player should move to when entering cover
    public float snapSpeed = 10f; // How fast the player snaps into cover
    public bool isInCover = false;

    private Transform player;

    void Start()
    {
        // Find your player in the scene
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isInCover)
        {
            // Smoothly pull the player to the cover spot
            player.position = Vector3.Lerp(player.position, coverSpot.position, Time.deltaTime * snapSpeed);
        }
    }

    public void EnterCover()
    {
        isInCover = true;
    }

    public void ExitCover()
    {
        isInCover = false;
    }

    void OnDrawGizmos()
    {
        if (coverSpot != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(coverSpot.position, 0.2f);
        }
    }
}
