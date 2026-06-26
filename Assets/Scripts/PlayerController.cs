using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;

    [Header("Jump")]
    public float firstJumpForce = 14f;
    public float secondJumpForce = 12f;
    public int maxJumps = 2;

    [Header("Jump Gravity")]
    public float normalGravity = 3f;
    public float fallGravity = 7f;
    public float lowJumpGravity = 6f;
    public float maxFallSpeed = 18f;

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

    // Saúde — usada pelo HeartSystem e EnemyController
    public int health = 3;

    // Privados
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    private bool takingDamage = false;
    private Vector3 cameraTargetPosition;

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

    // ──────────────────────────────────────
    // Movimento
    // ──────────────────────────────────────
    private void HandleMovement()
    {
        float move = Input.GetAxisRaw("Horizontal");

        if (spriteRenderer != null)
        {
            if (move > 0) spriteRenderer.flipX = false;
            else if (move < 0) spriteRenderer.flipX = true;
        }

        if (!takingDamage)
        {
            rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);
        }
    }

    // ──────────────────────────────────────
    // Chão
    // ──────────────────────────────────────
    private void HandleGroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0,
            groundLayer
        );

        if (isGrounded)
            jumpCount = 0;
    }

    // ──────────────────────────────────────
    // Pulo — normal, duplo e variável (segurar = mais alto)
    // ──────────────────────────────────────
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            float force = jumpCount == 0 ? firstJumpForce : secondJumpForce;

            // Zera velocidade vertical para pulos consistentes
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);

            jumpCount++;
        }
    }

    // ──────────────────────────────────────
    // Gravidade dinâmica
    // Caindo     → pesado (queda rápida)
    // Subindo    → normal se segurar Space, lowJump se soltar
    // ──────────────────────────────────────
    private void HandleGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravity;

            // Limita velocidade de queda
            if (rb.linearVelocity.y < -maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            // Soltou o Space — corta o pulo (pulo curto)
            rb.gravityScale = lowJumpGravity;
        }
        else
        {
            // Subindo com Space pressionado — pulo cheio
            rb.gravityScale = normalGravity;
        }
    }

    // ──────────────────────────────────────
    // Camera look-ahead
    // ──────────────────────────────────────
    private void HandleCameraLookAhead()
    {
        if (cameraTarget == null) return;

        float move = Input.GetAxisRaw("Horizontal");

        if (move > 0)
            cameraTargetPosition = new Vector3(
                lookAheadDistance,
                0f,  // << sempre 0, não acumula altura
                cameraTarget.localPosition.z
            );
        else if (move < 0)
            cameraTargetPosition = new Vector3(
                -lookAheadDistance,
                0f,  // << sempre 0
                cameraTarget.localPosition.z
            );
        else
            cameraTargetPosition = new Vector3(
                0f,
                0f,
                cameraTarget.localPosition.z
            );

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            cameraTargetPosition,
            cameraSpeed * Time.deltaTime
        );
    }

    // ──────────────────────────────────────
    // Dano — chamado pelo EnemyController e TriggerDamage
    // ──────────────────────────────────────
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

    // Sobrecarga sem knockback — compatível com TriggerDamage e HeartSystem
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Debug.Log("Morreu");
    }

    private void StopTakingDamage()
    {
        takingDamage = false;
    }

    // ──────────────────────────────────────
    // Gizmos
    // ──────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}