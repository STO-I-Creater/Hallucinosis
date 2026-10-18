using UnityEngine;

public class WireDrag : MonoBehaviour
{
    public Transform leftEnd;     // draggable
    public Transform midSegment;  // scales in X only
    public Transform rightEnd;    // target end
    public string wireId;         // e.g., "Red" (same id on right target)
    public float snapDistance = 0.3f;
    public bool isConnected = false;

    Camera cam;
    bool dragging = false;

    void Start() => cam = Camera.main;

    // Called by TouchManager when left-end is touched
    public void OnDrag(Vector2 worldPos)
    {
        if (isConnected) return;
        // If touch began on this object, start dragging
        dragging = true;
        UpdateDrag(worldPos);
    }

    public void OnRelease(Vector2 worldPos)
    {
        if (!dragging) return;
        dragging = false;

        // check overlap with target
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.CompareTag("WireTarget"))
        {
            var target = hit.GetComponent<WireTarget>();
            if (target != null && target.wireId == wireId)
            {
                // Snap to target
                leftEnd.position = target.transform.position; // if you want left to snap; or snap mid/right
                rightEnd.position = target.transform.position;
                UpdateMid();
                isConnected = true;
                target.OnConnected();
                return;
            }
        }

        // if not correct, return leftEnd to start
        ResetWire();
    }

    void UpdateDrag(Vector2 worldPos)
    {
        // move left end to touch x,y but constrain to panel area if needed
        leftEnd.position = worldPos;
        UpdateMid();
    }

    void UpdateMid()
    {
        Vector2 a = leftEnd.position;
        Vector2 b = rightEnd.position;
        Vector2 dir = b - a;
        float dist = dir.magnitude;
        if (dist < 0.001f) dist = 0.001f;

        // position mid in the middle
        midSegment.position = a + dir * 0.5f;
        // rotate mid to match angle
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        midSegment.rotation = Quaternion.Euler(0, 0, angle);

        // scale midSegment in X to match distance (assuming original sprite width = 1 unit)
        // You may have to tweak baseWidth to match your sprite size
        float baseWidth = 1f;
        midSegment.localScale = new Vector3(dist / baseWidth, midSegment.localScale.y, 1);
    }

    void ResetWire()
    {
        // implement returning to original position (store initial positions if needed)
        // for brevity, just set leftEnd to its parent's original local pos
        leftEnd.localPosition = new Vector3(-1f, 0, 0); // replace with stored start pos
        UpdateMid();
    }
}
