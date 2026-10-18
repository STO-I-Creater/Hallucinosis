using UnityEngine;

public class WireTarget : MonoBehaviour
{
    public string wireId;

    public void OnConnected()
    {
        // play effect, mark as connected
        GetComponent<SpriteRenderer>().enabled = true;
        // optional: disable collider so it can't be used again
        GetComponent<Collider2D>().enabled = false;
    }
}
