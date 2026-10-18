using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance.savedCheckpoint != Vector2.zero)
        {
            // Place player at last checkpoint
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = GameManager.Instance.savedCheckpoint;
                Debug.Log("Player placed at checkpoint: " + GameManager.Instance.savedCheckpoint);
            }
        }
        else
        {
            Debug.Log("Player spawned at scene default spawn.");
        }
    }
}
