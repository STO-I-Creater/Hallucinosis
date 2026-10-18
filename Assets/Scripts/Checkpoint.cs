using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.SetCheckpoint(transform.position);
            isActivated = true;

            Debug.Log("Checkpoint activated at: " + transform.position);

            // Optional – disable collider so it can’t trigger again
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }

    // RESET FUNCTION (Fixes your error)
    public void ResetCheckpoint()
    {
        isActivated = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = true;

        Debug.Log("Checkpoint reset: " + name);
    }
}
