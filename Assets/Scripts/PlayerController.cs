using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;

    [Header("Jump")]
    public float firstJumpForce = 13f;
    public float secondJumpForce = 11f;
    public int maxJumps = 2;

    [Header("Jump Gravity")]
    public float normalGravity = 2f;
    public float fallGravity = 5f;
    public float lowJumpGravity = 4f;
    public float maxFallSpeed = 14f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    [Header("Camera Look-Ahead")]
    public Transform cameraTarget;
    public float cameraSpeed = 5f;
    public float lookAheadDistance = 1.7f;

    [Header("Knockback")]
    public float knockbackForce = 10f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    public int health = 3;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;        // << estado do frame anterior
    private int jumpCount;
    private bool takingDamage = false;
    private Vector3 cameraTargetPosition;

    private float coyoteTimeCounter;
    public float coyoteTime = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = normalGravity;
        transform.localScale = Vector3.one;

        if (cameraTarget != null)
            cameraTargetPosition = cameraTarget.localPosition;
    }

    void Update()
    {
        HandleMovement();
        HandleGroundCheck();
        HandleJump();
        HandleCameraLookAhead();
    }

    void FixedUpdate()
    {
        HandleGravity();
    }

    private void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");

        if (spriteRenderer != null)
        {
            if (move > 0) spriteRenderer.flipX = false;
            else if (move < 0) spriteRenderer.flipX = true;
        }

        if (!takingDamage)
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
    }

    private void HandleGroundCheck()
    {
        wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0,
            groundLayer
        );

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;

            // Só reseta o jumpCount quando ACABOU de pousar
            // evita resetar enquanto caminha na borda
            if (!wasGrounded || jumpCount > 1)
                jumpCount = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            // Saiu do chão sem pular — conta como primeiro pulo gasto
            // para não ganhar pulo extra ao cair de plataforma
            if (wasGrounded && jumpCount == 0)
                jumpCount = 1;
        }
    }

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        // Primeiro pulo — chão ou coyote time
        if (coyoteTimeCounter > 0f && jumpCount <= 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, firstJumpForce);
            jumpCount = 1;
            coyoteTimeCounter = 0f;
        }
        // Double jump
        else if (jumpCount == 1)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, secondJumpForce);
            jumpCount = 2;
        }
    }

    private void HandleGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravity;

            if (rb.linearVelocity.y < -maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.gravityScale = lowJumpGravity;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }
    }

    private void HandleCameraLookAhead()
    {
        if (cameraTarget == null) return;

        float move = Input.GetAxisRaw("Horizontal");

        if (move > 0)
            cameraTargetPosition = new Vector3(lookAheadDistance, 0f, cameraTarget.localPosition.z);
        else if (move < 0)
            cameraTargetPosition = new Vector3(-lookAheadDistance, 0f, cameraTarget.localPosition.z);
        else
            cameraTargetPosition = new Vector3(0f, 0f, cameraTarget.localPosition.z);

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            cameraTargetPosition,
            cameraSpeed * Time.deltaTime
        );
    }

    public void TakeDamage(int damage, Transform enemy)
    {
        HeartSystem heart = GetComponent<HeartSystem>();
        if (heart != null)
            heart.vida -= damage;

        takingDamage = true;

        float direction = transform.position.x > enemy.position.x ? 1f : -1f;
        Vector2 knockbackDirection = new Vector2(direction, 1f).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        Invoke(nameof(StopTakingDamage), 0.3f);
    }

    public void TakeDamage(int damage)
    {
        HeartSystem heart = GetComponent<HeartSystem>();
        if (heart != null)
            heart.vida -= damage;
        else
            health -= damage;
    }

    private void StopTakingDamage()
    {
        takingDamage = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}