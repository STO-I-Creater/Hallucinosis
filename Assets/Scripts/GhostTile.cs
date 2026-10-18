using System.Collections;
using UnityEngine;

public class GhostPlatform : MonoBehaviour
{
    [Tooltip("How long the platform is solid.")]
    public float visibleTime = 2f;
    [Tooltip("How long the platform is invisible.")]
    public float invisibleTime = 2f;
    [Tooltip("Random start delay so multiple platforms aren't perfectly synced.")]
    public float startOffset = 0f;

    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        StartCoroutine(GhostCycle());
    }

    private IEnumerator GhostCycle()
    {
        yield return new WaitForSeconds(startOffset);

        while (true)
        {
            // Visible
            sr.enabled = true;
            col.enabled = true;
            yield return new WaitForSeconds(visibleTime);

            // Invisible
            sr.enabled = false;
            col.enabled = false;
            yield return new WaitForSeconds(invisibleTime);
        }
    }
}