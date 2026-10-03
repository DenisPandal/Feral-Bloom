using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    Animator animator;
    public static UnityAction OnGoingNextLevel;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetTrigger("Open");
            SoundFXManager.Play("LevelProgress");
            OnGoingNextLevel!.Invoke();
            GameManager.levelProgressTrigger = true;
        }
    }
}
