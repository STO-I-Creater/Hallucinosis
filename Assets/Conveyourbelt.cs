using UnityEngine;

public class ConveyorBelt2D : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 2.0f; // The magnitude of the force
    
    // We use forceMagnitude instead of "speed" for the Effector
    private AreaEffector2D effector;
    private Material material;

    void Start()
    {
        effector = GetComponent<AreaEffector2D>();
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
    }

    void Update()
    {
        // Update the physics speed from the public variable
        if (effector != null)
        {
            // forceMagnitude is the correct property name
            effector.forceMagnitude = speed;
        }

        // Visual scrolling (adjust the 0.5f to match your texture scale)
        if (material != null)
        {
            float offset = Time.time * speed * 0.2f;
            material.mainTextureOffset = new Vector2(offset, 0);
        }
    }
}