using UnityEngine;

public class ButtonAnimationTrigger : MonoBehaviour
{
    public Animator animator;
    public string animationName;

    public void PlayAnimation()
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }
}
