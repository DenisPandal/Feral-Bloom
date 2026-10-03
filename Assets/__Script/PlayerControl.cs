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

    [SerializeField] ParticleSystem dustFX;

    bool gameBeginning = true;

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


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityBase = rb.gravityScale;
        animator = GetComponentInChildren<Animator>();
        animator.SetTrigger("isAppearing");
        Door.OnGoingNextLevel += GoNextLevel;
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
        //delay at the game start for the chracter creation effect
        if (gameBeginning)
        {
            t += Time.deltaTime;
            if (t > 0.5f)
            {
                gameBeginning = false;
            }
            return;
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
        if (context.performed && !isJumping && isGrounded)
        {
            isJumping = true;
            dustFX.Play();
            SoundFXManager.Play("Jump");
            rb.linearVelocityY = jumpForce;
            jumpStart = transform.position.y;
        }
        else if (context.canceled && isGoingUp)
        {
            rb.linearVelocityY *= 0.5f;
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
