using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossController : MonoBehaviour
{
    public static UnityAction OnBossDefeated;

    [Header("--- Health ---")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    private bool isInvincible = false;
    private bool isDead = false;

    [Header("--- Movement & Bounds ---")]
    [SerializeField] private float arenaMinX = 94f;
    [SerializeField] private float arenaMaxX = 130f;
    [SerializeField] private float groundY = -8.2f;
    [SerializeField] private float hoverY = -5.0f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float chargeSpeed = 16f;

    [Header("--- Attacks & Projectiles ---")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("--- Visuals & Sprites ---")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] chargeFrames;
    [SerializeField] private Sprite[] attackFrames;
    [SerializeField] private Sprite[] hurtFrames;

    [Header("--- UI ---")]
    [SerializeField] private BossHealthBar healthBar;

    private Rigidbody2D rb;
    private Transform player;
    private Coroutine currentBehavior;
    private Coroutine animationCoroutine;
    private bool isFacingRight = false;
    private bool isCharging = false;
    private bool isActive = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (firePoint == null) firePoint = transform;
        if (healthBar == null) healthBar = FindFirstObjectByType<BossHealthBar>();
    }

    public void ActivateBoss()
    {
        if (isActive) return;
        isActive = true;
        currentHealth = maxHealth;
        isDead = false;

        // Notificar al controlador de arena para cerrar barreras
        var arena = FindFirstObjectByType<BossArenaController>();
        if (arena != null)
        {
            arena.StartBossFight();
        }

        if (healthBar == null) healthBar = FindFirstObjectByType<BossHealthBar>();
        if (healthBar != null) healthBar.Show("TITÁN DE HUMO PRIMORDIAL", currentHealth, maxHealth);
        else BossHealthBar.Instance?.Show("TITÁN DE HUMO PRIMORDIAL", currentHealth, maxHealth);

        currentBehavior = StartCoroutine(BossBehaviorRoutine());
        animationCoroutine = StartCoroutine(AnimateFrames(idleFrames, 0.12f, true));
    }

    void Update()
    {
        if (isDead) return;

        if (!isActive)
        {
            // Detectar si el jugador se acerca a la zona del jefe
            if (player == null)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
            }

            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                if (player.position.x >= BossArenaController.EntryX)
                {
                    ActivateBoss();
                }
            }
            return;
        }

        // Orientación hacia el jugador cuando no está embistiendo a toda velocidad
        if (!isCharging && player != null)
        {
            if (player.position.x > transform.position.x && !isFacingRight)
            {
                Flip(true);
            }
            else if (player.position.x < transform.position.x && isFacingRight)
            {
                Flip(false);
            }
        }
    }

    void Flip(bool faceRight)
    {
        isFacingRight = faceRight;
        Vector3 ls = transform.localScale;
        ls.x = Mathf.Abs(ls.x) * (faceRight ? -1f : 1f); // El sprite original mira hacia la izquierda
        transform.localScale = ls;
    }

    IEnumerator BossBehaviorRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        int attackPattern = 0;

        while (!isDead)
        {
            // 1. Estado flotante / acecho (Idle)
            SetAnimation(idleFrames, 0.12f, true);
            float waitTime = UnityEngine.Random.Range(2f, 3f);
            float timer = 0f;
            Vector3 startPos = transform.position;

            while (timer < waitTime)
            {
                timer += Time.deltaTime;
                // Pequeña ondulación sinodal
                float bob = Mathf.Sin(Time.time * 3f) * 0.4f;
                transform.position = new Vector3(transform.position.x, hoverY + bob, transform.position.z);
                yield return null;
            }

            // Alternar ataques: Envestida -> Proyectiles en Abanico -> Envestida -> Ráfaga de Proyectiles
            attackPattern = (attackPattern + 1) % 4;

            if (attackPattern == 1 || attackPattern == 3)
            {
                // ATAQUE DE ENVESTIDA (Charge)
                yield return StartCoroutine(ChargeAttackRoutine());
            }
            else if (attackPattern == 2)
            {
                // ATAQUE DE PROYECTILES CON PATRÓN: ABANICO (Spread)
                yield return StartCoroutine(SpreadShotRoutine());
            }
            else
            {
                // ATAQUE DE PROYECTILES CON PATRÓN: RÁFAGA CONTINUA (Burst)
                yield return StartCoroutine(BurstShotRoutine());
            }

            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator ChargeAttackRoutine()
    {
        // Descender al nivel del suelo
        while (Mathf.Abs(transform.position.y - groundY) > 0.1f)
        {
            float newY = Mathf.MoveTowards(transform.position.y, groundY, moveSpeed * 3f * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        // Telegrafiado / aviso de peligro (Aura roja y animación de carga)
        SetAnimation(chargeFrames, 0.08f, true);
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
        SoundFXManager.Play("Jump");

        yield return new WaitForSeconds(0.85f);

        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        // Dirección de la envestida hacia el jugador
        isCharging = true;
        float dir = (player != null && player.position.x > transform.position.x) ? 1f : -1f;
        float targetX = dir > 0 ? arenaMaxX - 1.5f : arenaMinX + 1.5f;

        // Envestida a gran velocidad
        while (Mathf.Abs(transform.position.x - targetX) > 0.5f)
        {
            float newX = Mathf.MoveTowards(transform.position.x, targetX, chargeSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, groundY, transform.position.z);
            yield return null;
        }

        isCharging = false;
        SoundFXManager.Play("Hit");

        // Periodo de vulnerabilidad / fatiga tras la envestida (Oportunidad de golpe para el jugador)
        SetAnimation(hurtFrames, 0.15f, true);
        if (spriteRenderer != null) spriteRenderer.color = new Color(0.8f, 0.8f, 1f, 0.9f);
        yield return new WaitForSeconds(1.8f);

        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        // Regresar a la altura normal flotante
        while (Mathf.Abs(transform.position.y - hoverY) > 0.1f)
        {
            float newY = Mathf.MoveTowards(transform.position.y, hoverY, moveSpeed * 2f * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
    }

    IEnumerator SpreadShotRoutine()
    {
        SetAnimation(attackFrames, 0.1f, true);
        SoundFXManager.Play("Jump");

        yield return new WaitForSeconds(0.5f);

        // Disparar 5 proyectiles en abanico
        if (projectilePrefab != null)
        {
            Vector3 origin = firePoint.position;
            float baseAngle = isFacingRight ? 0f : 180f;
            if (player != null)
            {
                Vector2 toPlayer = (player.position - origin).normalized;
                baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            }

            float[] angleOffsets = new float[] { -30f, -15f, 0f, 15f, 30f };
            foreach (float offset in angleOffsets)
            {
                float ang = (baseAngle + offset) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

                var projObj = Instantiate(projectilePrefab, origin, Quaternion.identity);
                var proj = projObj.GetComponent<BossProjectile>();
                if (proj != null)
                {
                    proj.Initialize(dir, 7.5f, 1);
                }
            }
            SoundFXManager.Play("Hit");
        }

        yield return new WaitForSeconds(0.6f);
    }

    IEnumerator BurstShotRoutine()
    {
        SetAnimation(attackFrames, 0.1f, true);

        // Disparar 3 proyectiles guiados consecutivos
        for (int i = 0; i < 3; i++)
        {
            if (isDead) yield break;

            if (projectilePrefab != null && player != null)
            {
                Vector3 origin = firePoint.position;
                Vector2 dir = ((Vector2)player.position - (Vector2)origin).normalized;

                var projObj = Instantiate(projectilePrefab, origin, Quaternion.identity);
                var proj = projObj.GetComponent<BossProjectile>();
                if (proj != null)
                {
                    proj.Initialize(dir, 8.5f, 1);
                }
                SoundFXManager.Play("Jump");
            }

            yield return new WaitForSeconds(0.45f);
        }

        yield return new WaitForSeconds(0.4f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision2D collision)
    {
        if (isDead) return;

        var playerControl = collision.collider.GetComponent<PlayerControl>() 
                         ?? collision.collider.GetComponentInParent<PlayerControl>()
                         ?? collision.collider.transform.root.GetComponent<PlayerControl>();

        if (playerControl != null)
        {
            var playerRb = playerControl.GetComponent<Rigidbody2D>();
            // Verificar si el jugador está cayendo sobre el jefe desde arriba
            bool isStomp = false;
            if (playerControl.transform.position.y > transform.position.y + 0.35f)
            {
                if (playerRb == null || playerRb.linearVelocityY <= 2.0f)
                {
                    isStomp = true;
                }
            }

            if (isStomp)
            {
                TakeStompDamage(playerControl);
            }
            else if (!isInvincible)
            {
                // Daño al jugador por contacto lateral o inferior
                Vector3 contactPos = collision.contactCount > 0 ? (Vector3)collision.GetContact(0).point : transform.position;
                Enemy.OnPlayerHit?.Invoke(1, contactPos, false);
            }
        }
    }

    public void TakeStompDamage(PlayerControl playerControl)
    {
        if (isInvincible || isDead) return;
        
        // Hacer rebotar al jugador hacia arriba limpiamente
        var playerRb = playerControl.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocityY = 12f;
        }

        TakeMeleeDamage(1);
    }

    public void TakeMeleeDamage(int dmg)
    {
        if (isInvincible || isDead) return;

        currentHealth -= dmg;
        isInvincible = true;

        SoundFXManager.Play("Hit");

        if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);
        else BossHealthBar.Instance?.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFeedback());
        }
    }

    IEnumerator DamageFeedback()
    {
        SetAnimation(hurtFrames, 0.1f, false);
        for (int i = 0; i < 4; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 0.7f);
            yield return new WaitForSeconds(0.12f);
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.12f);
        }
        isInvincible = false;
        SetAnimation(idleFrames, 0.12f, true);
    }

    void Die()
    {
        isDead = true;
        isActive = false;

        if (currentBehavior != null) StopCoroutine(currentBehavior);
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);

        SoundFXManager.Play("Death");
        GameManager.ScoreAdd(500);
        if (healthBar != null) healthBar.Hide();
        else BossHealthBar.Instance?.Hide();

        OnBossDefeated?.Invoke();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.1f, 0.1f, 1f);
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        float timer = 0f;
        while (timer < 1.2f)
        {
            timer += Time.deltaTime;
            transform.localScale *= 0.97f;
            transform.Rotate(0, 0, 180f * Time.deltaTime);
            yield return null;
        }

        // Spawn Dash item before dying
        GameObject drop = new GameObject("DashUpgradeDrop");
        drop.transform.position = transform.position + new Vector3(0, -1.5f, 0);
        var dropScript = drop.AddComponent<UpgradeItemDrop>();
        Sprite dashSprite = Resources.Load<Sprite>("Dash");
        dropScript.Setup(dashSprite, "Dash");

        Destroy(gameObject);
    }

    void SetAnimation(Sprite[] frames, float frameRate, bool loop)
    {
        if (frames == null || frames.Length == 0) return;
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateFrames(frames, frameRate, loop));
    }

    IEnumerator AnimateFrames(Sprite[] frames, float frameRate, bool loop)
    {
        if (frames == null || frames.Length == 0) yield break;

        int index = 0;
        while (true)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = frames[index];
            }

            yield return new WaitForSeconds(frameRate);
            index++;
            if (index >= frames.Length)
            {
                if (loop) index = 0;
                else yield break;
            }
        }
    }
}
