using System.Collections;
using UnityEngine;

public class UpgradeItemDrop : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private string upgradeName = "Dash";
    
    private Vector3 startPos;
    private bool isLanded = false;
    private SpriteRenderer sr;
    private GameObject glowAura;
    
    public void Setup(Sprite itemSprite, string name)
    {
        upgradeName = name;

        // Main Sprite
        transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = itemSprite;
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;

        // Glow Aura
        glowAura = new GameObject("Glow");
        glowAura.transform.SetParent(transform);
        glowAura.transform.localPosition = Vector3.zero;
        var auraSr = glowAura.AddComponent<SpriteRenderer>();
        auraSr.sprite = itemSprite;
        auraSr.sortingLayerName = "Player";
        auraSr.sortingOrder = 9;
        auraSr.color = new Color(0f, 1f, 0.8f, 0.4f); // Cyan glow
        glowAura.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        
        // Physics for the drop arc
        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 2f;
        rb.linearVelocity = new Vector2(0f, 3f); // Toss straight up a bit
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Collider for landing
        var col = gameObject.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        // Particle System for flashy effects
        CreateSparkles();

        StartCoroutine(LandingCheck(rb, col));
    }

    private void CreateSparkles()
    {
        GameObject particles = new GameObject("Sparkles");
        particles.transform.SetParent(transform);
        particles.transform.localPosition = Vector3.zero;

        var ps = particles.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        
        var main = ps.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.2f, 1f, 0.8f, 1f);
        
        var emission = ps.emission;
        emission.rateOverTime = 15f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        ps.Play();
    }

    IEnumerator LandingCheck(Rigidbody2D rb, Collider2D col)
    {
        // Wait until it starts falling
        yield return new WaitUntil(() => rb.linearVelocityY <= 0.1f);

        // Ground detection to stop falling
        while (!isLanded)
        {
            var hits = Physics2D.RaycastAll(transform.position, Vector2.down, 0.8f);
            foreach (var hit in hits)
            {
                if (hit.collider != col && !hit.collider.isTrigger && 
                    !hit.collider.CompareTag("Player") && 
                    !hit.collider.CompareTag("Enemy") && 
                    !hit.collider.CompareTag("Boss"))
                {
                    isLanded = true;
                    rb.linearVelocity = Vector2.zero;
                    rb.isKinematic = true;
                    startPos = transform.position;
                    
                    // Become trigger for collection
                    col.isTrigger = true;
                    break;
                }
            }
            yield return null;
        }
    }

    void Update()
    {
        if (isLanded)
        {
            // Floating animation
            transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * floatSpeed) * floatAmplitude, 0f);

            // Pulse glow
            if (glowAura != null)
            {
                float pulse = 1.3f + Mathf.Sin(Time.time * 6f) * 0.2f;
                glowAura.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isLanded && other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void Collect()
    {
        // Add effect on collection
        SoundFXManager.Play("PickUp");
        
        // Desbloquear Dash en GameManager y el Menu
        GameManager.hasDash = true;
        
        // Destruir
        Destroy(gameObject);
    }
}
