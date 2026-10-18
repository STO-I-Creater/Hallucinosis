using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public SpriteRenderer wireEnd;   // Should be Tiled
    public GameObject lightOn;

    private Vector3 startPoint;
    private Vector3 startPosition;
    private bool connected = false;  // Prevent multiple connections

    void Start()
    {
        startPoint = transform.parent.position;
        startPosition = transform.position;

        if (lightOn != null)
            lightOn.SetActive(false);
    }

    private void OnMouseDrag()
    {
        if (connected) return; // Already connected

        // Mouse position to world point
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0;

        // Check for nearby connection points (only check objects on "WireEnd" layer if you want)
        Collider2D[] colliders = Physics2D.OverlapCircleAll(newPosition, 0.2f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.transform.parent != transform.parent)
            {
                // Only connect if parent names match (or set some ID)
                if (transform.parent.name == collider.transform.parent.name)
                {
                    // Snap wire to connection
                    UpdateWire(collider.transform.position);

                    // Count connection
                    if (Main.Instance != null)
                        Main.Instance.SwitchChange(1);

                    // Mark done
                    collider.GetComponent<Wire>()?.Done();
                    Done();
                    return;
                }
            }
        }

        // Update wire position normally
        UpdateWire(newPosition);
    }

    void Done()
    {
        connected = true;

        // Turn on light
        if (lightOn != null)
            lightOn.SetActive(true);

        // Keep wire in place, remove dragging script
        Destroy(this);
    }

    private void OnMouseUp()
    {
        if (!connected)
        {
            // Reset wire to start
            UpdateWire(startPosition);
        }
    }

    void UpdateWire(Vector3 newPosition)
    {
        transform.position = newPosition;

        Vector3 direction = newPosition - startPoint;
        transform.right = direction.normalized;

        float dist = Vector2.Distance(startPoint, newPosition);
        if (wireEnd != null)
            wireEnd.size = new Vector2(dist, wireEnd.size.y);  // WireEnd must be Tiled
    }
}
