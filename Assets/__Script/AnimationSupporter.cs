using UnityEngine;

public class AnimationSupporter : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnHitAnimationEnded()
    {
        animator.SetBool("isHit", false);
    }
}
