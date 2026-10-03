using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public static UnityAction<int> OnHealthChanged;
    public static UnityAction OnDead;

    bool isInvincible;

    Rigidbody2D rb;
    Animator animator;
   

    void Start()
    {
        OnHealthChanged?.Invoke(GameManager.health);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        Trap.OnPlayerHit += TakeDamage;
        Enemy.OnPlayerHit += TakeDamage;
        DeathZone.OnPlayerHit += TakeDamage;
    }

    void OnDisable()
    {
        Trap.OnPlayerHit -= TakeDamage;
        Enemy.OnPlayerHit -= TakeDamage;
        DeathZone.OnPlayerHit -= TakeDamage;
    }

    void TakeDamage(int damage, Vector3 hitPosition, bool instantDeath)
    {
        float force = 5f;
        
        if (isInvincible && !instantDeath)
        {
            return;
        }
        if (!GameManager.isGameOn)
        {
            return;
        }

        GameManager.health -= damage;
        if (GameManager.health < 0)
        {
            GameManager.health = 0;
        }

        if (damage > 0)
        {
            SoundFXManager.Play("Hit");
            OnHealthChanged?.Invoke(GameManager.health);
            animator.SetTrigger("isHit");
        }
        rb.linearVelocity = Vector3.zero;
        //rb.linearVelocityY = 1.0f * force;
        rb.AddForceY(force, ForceMode2D.Impulse);

        if (GameManager.health <= 0 || instantDeath)
        {
            PlayerDie();
        }
        else if (damage > 0)
        {
            StartCoroutine(MakeInvincible(1));
        }
    }

    IEnumerator MakeInvincible(float timer)
    {
        isInvincible = true;
        Renderer renderer = GetComponentInChildren<Renderer>();

        float blinkDuration = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < timer)
        {
            renderer.enabled = !renderer.enabled;
            yield return new WaitForSeconds(blinkDuration);
            elapsedTime += blinkDuration;
        }

        renderer.enabled = true;
        isInvincible = false;
    }

    void PlayerDie()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GameManager.isGameOn = false;
        SoundFXManager.Play("Death");
        animator.SetTrigger("isDead");
        StartCoroutine(Delay(1.3f));
    }

    IEnumerator Delay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnDead!.Invoke();
    }
}
