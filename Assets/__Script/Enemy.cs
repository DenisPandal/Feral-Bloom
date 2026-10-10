using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected int health;
    protected int currentHealth;
    [SerializeField] protected int damage;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected bool isFacingRight;
    [SerializeField] protected GameObject head;
    [SerializeField] protected float headHeight;
    [SerializeField] protected float headWidth;
    [SerializeField] protected LayerMask playerMask;
    [SerializeField] protected LayerMask playerWeaponMask;
    [SerializeField] protected int enemyLayer = 11;
    [SerializeField] protected int enemyDeadLayer = 12;
    [SerializeField] protected int enemyTmpLayer = 13;
    [SerializeField] protected ParticleSystem crashFX;

    public static UnityAction<int, Vector3, bool> OnPlayerHit;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected Vector3 headSize;
    protected Vector3 headPosition;
    protected bool isAlive = true;
    [SerializeField] protected int score;

    public static UnityAction<Vector3, int> OnGettingPoint;


    protected virtual void Start()
    {
        isAlive = true;
        currentHealth = health;
        rb = GetComponent<Rigidbody2D>();
        animator = rb.GetComponentInChildren<Animator>();
        crashFX = rb.GetComponentInChildren<ParticleSystem>();

        headSize = new Vector3(headWidth, headHeight, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //enemy got hit (on the head)
        headPosition = head.transform.position;
        if (Physics2D.OverlapBox(headPosition, headSize, 0, playerWeaponMask))
        {
            TakeDamage(1);
        }
        //player got hit
        else if (collision.collider.CompareTag("Player"))
        {
            MakeUninteractable(1f);
            Vector3 position = transform.position;
            OnPlayerHit?.Invoke(damage, position, false);
        }
    }

    public void TakeDamage(int dmg)
    {
        MakeUninteractable(2f);
        SoundFXManager.Play("Hit"); // or StepOn
        animator.SetBool("isHit", true);
        
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            GameManager.ScoreAdd(score);
            OnGettingPoint?.Invoke(transform.position, score);
            
            if (crashFX != null) crashFX.Play();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.down * 2f, ForceMode2D.Impulse);
            rb.constraints = RigidbodyConstraints2D.None;
            rb.AddTorque(90);
            gameObject.layer = enemyDeadLayer;

            EnemyDead();
        }
    }

    void MakeUninteractable(float duration)
    {
        gameObject.layer = enemyTmpLayer;
        Invoke("MakeInteractable", duration);
    }
    void MakeInteractable()
    {
        gameObject.layer = enemyLayer;
    }

    void EnemyDead()
    {
        isAlive = false;
        Destroy(this.gameObject, 0.8f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = UnityEngine.Color.red;
        headPosition = head.transform.position;
        Gizmos.DrawWireCube(headPosition, headSize);
    }
}
