using UnityEngine;

public class FlyAnimationHelper : MonoBehaviour
{
    Animator animator;
    [SerializeField] ParticleSystem dustFX;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    public void PlayDustFX()
    {
        dustFX.Play();
    }
}
