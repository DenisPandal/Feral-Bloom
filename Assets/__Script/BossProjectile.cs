using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 7.5f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifeTime = 5f;

    private Vector2 moveDirection;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float projSpeed = 7.5f, int projDamage = 1)
    {
        moveDirection = direction.normalized;
        speed = projSpeed;
        damage = projDamage;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (rb == null)
        {
            transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Enemy.OnPlayerHit?.Invoke(damage, transform.position, false);
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == 0 && !collision.isTrigger) // Suelo o plataformas sólidas
        {
            Destroy(gameObject);
        }
    }
}
