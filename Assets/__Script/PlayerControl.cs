using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;

    [Header("--- Movement ---")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float moveSpeedOnAir = 4f;
    float moveX;
    bool isFacingRight = true;

    [Header("--- Bottom Check ---")]
    [SerializeField] Transform bottom;
    [SerializeField] Vector2 bottomSize = new Vector2(2f, 2f);
    [SerializeField] LayerMask bottomLayer;
    [SerializeField] bool isJumping = false;
    [SerializeField] float fallGravity = 2.0f;
    float gravityBase;

    [Header("--- Side Check ---")]
    [SerializeField] Transform side;
    [SerializeField] Vector2 sideSize = new Vector2(2f, 2f);
    [SerializeField] LayerMask sideLayer;
    [SerializeField] float slideSpeed = -1.5f;
    bool isOnWall;

    [Header("--- Jump ---")]
    [SerializeField] float jumpForce = 8f;
    [SerializeField] float jumpMax = 3.0f;
    [SerializeField] float jumpStart = 8f;
    [Tooltip("Cantidad máxima de saltos (2 para doble salto)")]
    [SerializeField] int maxJumps = 2;
    [Tooltip("Fuerza del salto en el aire / doble salto")]
    [SerializeField] float doubleJumpForce = 7.8f;

    [SerializeField] ParticleSystem dustFX;

    int jumpsRemaining = 2;

    [Header("--- Dash ---")]
    [SerializeField] float dashSpeed = 22f;
    [SerializeField] float dashDuration = 0.22f;
    [SerializeField] float dashCooldown = 0.8f;
    bool isDashing;
    bool canDash = true;

    bool gameBeginning = true;
    PlayerInput playerInput;
    InputAction moveAction;
    float lastJumpTime = -1f;

    bool isGoingUp {
        get {
            return rb.linearVelocityY > 0;
        }
    }
    bool isGoingDown {
        get {
            return rb.linearVelocityY < 0;
        }
    }

    void Awake()
    {
        Time.timeScale = 1f;
        GameManager.isGameOn = true;
        if (GameManager.health <= 0)
        {
            GameManager.health = 3;
        }

        playerInput = GetComponent<PlayerInput>();
        ReactivateInput();
    }

    void OnEnable()
    {
        ReactivateInput();
    }

    void ReactivateInput()
    {
        if (playerInput != null)
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
            playerInput.SwitchCurrentActionMap("Player");
            playerInput.ActivateInput();
            moveAction = playerInput.actions?.FindAction("Player/Move") ?? playerInput.actions?.FindAction("Move");
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityBase = rb.gravityScale;
        animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("isAppearing");
        Door.OnGoingNextLevel += GoNextLevel;
        jumpsRemaining = maxJumps;
        t = 0f;
        gameBeginning = true;
    }

    void OnDisable()
    {
        Door.OnGoingNextLevel -= GoNextLevel;
    }

    void GoNextLevel()
    {
        animator.SetTrigger("isNextLevel");
    }

    float t = 0;
    void Update()
    {
        if (isDashing) return;

        //delay at the game start for the chracter creation effect
        if (gameBeginning)
        {
            t += Time.unscaledDeltaTime;
            if (t > 0.5f)
            {
                gameBeginning = false;
            }
            return;
        }

        // Direct input read & fallback
        float currentInputX = 0f;
        if (moveAction != null && moveAction.enabled)
        {
            float actX = moveAction.ReadValue<Vector2>().x;
            if (Mathf.Abs(actX) > 0.01f)
            {
                currentInputX = actX;
            }
        }

        if (Mathf.Abs(currentInputX) < 0.01f && Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                currentInputX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                currentInputX += 1f;
        }

        if (Mathf.Abs(currentInputX) < 0.01f && Gamepad.current != null)
        {
            float stickX = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stickX) > 0.1f)
                currentInputX = stickX;
            else if (Gamepad.current.dpad.left.isPressed)
                currentInputX -= 1f;
            else if (Gamepad.current.dpad.right.isPressed)
                currentInputX += 1f;
        }

        moveX = currentInputX;

        // Jump direct input fallback
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                TryPerformJump();
            else if (Keyboard.current.spaceKey.wasReleasedThisFrame && isGoingUp)
                rb.linearVelocityY *= 0.5f;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                TryPerformJump();
            else if (Gamepad.current.buttonSouth.wasReleasedThisFrame && isGoingUp)
                rb.linearVelocityY *= 0.5f;
        }

        if (!isJumping)
        {
            rb.linearVelocityX = moveX * moveSpeed;
        }
        else
        {
            rb.linearVelocityX = moveX * moveSpeedOnAir;
        }

        DirectionCheck();
        BottomCheck();
        SideCheck();
        FallingAccel();
        AnimationHandle();

        if (transform.position.y - jumpStart > jumpMax)
        {
            jumpMax = transform.position.y - jumpStart;
        }

        bool dashPressed = false;
        if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame) dashPressed = true;
        if (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame) dashPressed = true;

        if (dashPressed && GameManager.hasDash && canDash)
        {
            StartCoroutine(PerformDash());
            return;
        }

        // Melee attack input
        bool meleePressed = false;
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) meleePressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) meleePressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame) meleePressed = true;

        if (meleePressed)
        {
            TryMeleeAttack();
        }
    }

    IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        
        // Animación de dash
        if (animator != null) animator.SetTrigger("isAttack"); // Puedes usar otra si tienes una animación de dash específica
        
        rb.linearVelocity = new Vector2(isFacingRight ? dashSpeed : -dashSpeed, 0f);
        
        StartCoroutine(DashGhostRoutine());

        yield return new WaitForSeconds(dashDuration);
        
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);
        isDashing = false;
        
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    IEnumerator DashGhostRoutine()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) yield break;
        
        while (isDashing)
        {
            GameObject ghost = new GameObject("DashGhost");
            ghost.transform.position = sr.transform.position;
            ghost.transform.localScale = sr.transform.lossyScale;
            
            SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = sr.sprite;
            ghostSr.sortingLayerName = sr.sortingLayerName;
            ghostSr.sortingOrder = sr.sortingOrder - 1;
            ghostSr.color = new Color(0.3f, 1f, 0.8f, 0.6f); // Cyan translúcido
            
            Destroy(ghost, 0.35f);
            yield return new WaitForSeconds(0.04f); // Crea un fantasma cada pocos frames
        }
    }

    [Header("--- Melee Attack ---")]
    [SerializeField] float meleeCooldown = 0.35f;
    [SerializeField] float meleeRange = 1.3f;
    [SerializeField] int meleeDamage = 1;
    float lastMeleeTime = -1f;
    private static Sprite slashSprite;

    void TryMeleeAttack()
    {
        if (Time.unscaledTime - lastMeleeTime < meleeCooldown) return;
        lastMeleeTime = Time.unscaledTime;
        
        animator.SetTrigger("isAttack");
        
        SoundFXManager.Play("Hit"); // Sonido para espadazo
        
        // Detectar enemigos
        Vector2 attackPoint = (Vector2)transform.position + new Vector2(isFacingRight ? 1f : -1f, 0.2f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, meleeRange);
        
        foreach(var hit in hitEnemies)
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(meleeDamage);
            }
            var boss = hit.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeMeleeDamage(meleeDamage);
            }
        }
    }

    void AnimationHandle()
    {

        animator.SetBool("isWalking", false);
        if (!isJumping && Mathf.Abs(moveX)>0.1f)
        {
            animator.SetBool("isWalking", true);
        }
        animator.SetBool("isOnWall", isOnWall);
        animator.SetFloat("velocityY", rb.linearVelocityY);
    }

    void FallingAccel()
    {
        if (isJumping)
        {
            if (transform.position.y - jumpStart > jumpMax)
            {
                rb.linearVelocityY = 0;
            }
        }
        if (!isJumping && rb.linearVelocityY > 0)
        {
            rb.linearVelocityY = 0;
        }

            if (rb.linearVelocityY < 0)
        {
            rb.gravityScale = gravityBase * fallGravity;
        }
        else
        {
            rb.gravityScale = gravityBase;
        }
    }

    bool IsTouchingSide()
    {
        return Physics2D.OverlapBox(side.position, sideSize, 0, sideLayer);
    }    

    void SideCheck()
    {
        if (IsTouchingSide() && isJumping)
        {
            isOnWall = true;
            rb.linearVelocityY = Mathf.Max(rb.linearVelocityY, slideSpeed);
            
        }
        else
        {
            isOnWall = false;
            
        }
    }

    void BottomCheck()
    {
        if (isGoingUp)
        {
            return;
        }
        if (Physics2D.OverlapBox(bottom.position, bottomSize, 0, bottomLayer))
        {
            isJumping = false;
            jumpsRemaining = maxJumps;
        }
    }

    void DirectionChange()
    {
        if (isGrounded)
        {
            dustFX.Play();
        }
        isFacingRight = !isFacingRight;
        Vector3 ls = rb.transform.localScale;
        ls.x *= -1f;
        rb.transform.localScale = ls;
    }

    void DirectionCheck()
    {
        if (isFacingRight && moveX < 0 || !isFacingRight && moveX > 0)
        {
            DirectionChange();
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveX = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryPerformJump();
        }
        else if (context.canceled && isGoingUp)
        {
            rb.linearVelocityY *= 0.5f;
        }
    }

    void TryPerformJump()
    {
        if (Time.unscaledTime - lastJumpTime < 0.08f)
        {
            return;
        }
        lastJumpTime = Time.unscaledTime;

        bool canGroundJump = isGrounded && (!isJumping || rb.linearVelocityY <= 0);

        if (canGroundJump)
        {
            ExecuteJump(jumpForce);
            jumpsRemaining = maxJumps - 1;
        }
        else if (jumpsRemaining > 0)
        {
            // Si el jugador cae de una plataforma sin saltar primero, consume el salto base
            if (jumpsRemaining == maxJumps)
            {
                jumpsRemaining = maxJumps - 1;
            }

            ExecuteJump(doubleJumpForce);
            jumpsRemaining--;
        }
    }

    void ExecuteJump(float force)
    {
        isJumping = true;
        if (dustFX != null)
        {
            dustFX.Play();
        }
        SoundFXManager.Play("Jump");
        rb.linearVelocityY = force;
        jumpStart = transform.position.y;
        if (animator != null)
        {
            animator.Play("Jump", 0, 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bottom.transform.position, bottomSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(side.transform.position, sideSize);
    }

    bool isGrounded {
        get { return Physics2D.OverlapBox(bottom.position, bottomSize, 0, bottomLayer); }
    }

}
