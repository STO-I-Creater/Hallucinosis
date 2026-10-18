using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayOnPlayerTouch : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator anim;
    public string animationStateName = "Activate"; // must match the state name in Animator
    public int animatorLayer = 0; // usually 0

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasPlayed) return; // animation already played

        if (collision.CompareTag("Player"))
        {
            if (anim != null)
            {
                anim.Play(animationStateName, animatorLayer, 0f); // play from start
                hasPlayed = true; // mark as played
            }
        }
    }
}
