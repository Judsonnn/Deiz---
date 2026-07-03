using UnityEngine;

public class FrogEnemyController : MonoBehaviour
{
    public enum FrogState { Patrolling, Charging, Jumping, Landing, Cooldown }

    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float patrolRadius = 4f;
    public float patrolPointWaitTime = 1.5f;

    [Header("Detection")]
    public float detectionRadius = 6f;

    [Header("Attack")]
    public float chargeTime = 1.5f;
    public float jumpForce = 14f;
    public float jumpHorizontalSpeed = 6f;
    public float landingCooldown = 0.8f;
    public float areaRadius = 2.5f;
    public int damage = 1;
    public LayerMask playerLayer;

    [Header("Indicator")]
    public GameObject areaIndicator;
    public float indicatorFadeDuration = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private FrogState currentState = FrogState.Patrolling;
    private Transform player;
    private Rigidbody2D rb;
    private Vector2 patrolCenter;
    private Vector2 currentPatrolTarget;
    private Vector2 lockedJumpTarget;

    private float chargeTimer;
    private float cooldownTimer;
    private float patrolWaitTimer;

    // FIX 1 — controla se já subiu para detectar pouso corretamente
    private bool hasReachedPeak = false;

    // FIX 2 — ignora colisão com o player durante o pulo
    private Collider2D myCollider;
    private Collider2D playerCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        patrolCenter = transform.position;
        PickNewPatrolPoint();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCollider = playerObj.GetComponent<Collider2D>();
        }

        if (areaIndicator != null)
            areaIndicator.SetActive(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case FrogState.Patrolling: HandlePatrol();  break;
            case FrogState.Charging:  HandleCharging(); break;
            case FrogState.Jumping:   HandleJumping();  break;
            case FrogState.Landing:   HandleLanding();  break;
            case FrogState.Cooldown:  HandleCooldown(); break;
        }

        HandleFlip();
    }

    private void HandlePatrol()
    {
        if (player != null &&
            Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            EnterCharging();
            return;
        }

        MoveTowards(currentPatrolTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, currentPatrolTarget) < 0.2f)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
                PickNewPatrolPoint();
        }
    }

    private void PickNewPatrolPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        currentPatrolTarget = patrolCenter + randomOffset;
        patrolWaitTimer = patrolPointWaitTime;
    }

    private void EnterCharging()
    {
        currentState = FrogState.Charging;
        chargeTimer = chargeTime;

        // FIX 3 — para completamente ao carregar
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (areaIndicator != null)
        {
            areaIndicator.SetActive(true);
            areaIndicator.transform.localScale = Vector3.one * (areaRadius * 2f);
        }
    }

    private void HandleCharging()
    {
        if (areaIndicator != null && player != null)
        {
            areaIndicator.transform.position = new Vector3(
                player.position.x,
                player.position.y - 0.5f,
                areaIndicator.transform.position.z
            );
        }

        chargeTimer -= Time.deltaTime;

        if (chargeTimer <= 0f)
        {
            if (player != null)
                lockedJumpTarget = player.position;

            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        currentState = FrogState.Jumping;
        hasReachedPeak = false;

        // FIX 4 — volta para Dynamic ao pular
        rb.bodyType = RigidbodyType2D.Dynamic;

        // FIX 5 — ignora colisão com player durante o pulo
        if (myCollider != null && playerCollider != null)
            Physics2D.IgnoreCollision(myCollider, playerCollider, true);

        if (areaIndicator != null)
            areaIndicator.SetActive(false);

        float direction = lockedJumpTarget.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * jumpHorizontalSpeed, jumpForce);
    }

    private void HandleJumping()
    {
        // Detecta o pico do pulo
        if (rb.linearVelocity.y < 0)
            hasReachedPeak = true;

        // Só pousa após atingir o pico
        if (hasReachedPeak && IsGrounded())
        {
            rb.linearVelocity = Vector2.zero;

            // Restaura colisão com player ao pousar
            if (myCollider != null && playerCollider != null)
                Physics2D.IgnoreCollision(myCollider, playerCollider, false);

            OnLand();
            currentState = FrogState.Landing;
        }
    }

    private void OnLand()
    {
        // FIX 6 — usa OverlapCircleAll com posição correta
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            areaRadius,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeDamage(damage, transform);
        }

        if (areaIndicator != null)
            StartCoroutine(ShowImpactEffect());
    }

    private System.Collections.IEnumerator ShowImpactEffect()
    {
        areaIndicator.SetActive(true);
        areaIndicator.transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            areaIndicator.transform.position.z
        );
        areaIndicator.transform.localScale = Vector3.one * (areaRadius * 2f);

        SpriteRenderer indicator = areaIndicator.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        Color startColor = new Color(1f, 0f, 0f, 0.6f);
        Color endColor = new Color(1f, 0f, 0f, 0f);

        while (elapsed < indicatorFadeDuration)
        {
            elapsed += Time.deltaTime;
            if (indicator != null)
                indicator.color = Color.Lerp(startColor, endColor, elapsed / indicatorFadeDuration);
            yield return null;
        }

        areaIndicator.SetActive(false);

        if (indicator != null)
            indicator.color = new Color(1f, 0f, 0f, 0.4f);
    }

    private void HandleLanding()
    {
        cooldownTimer = landingCooldown;
        currentState = FrogState.Cooldown;
    }

    private void HandleCooldown()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            if (player != null &&
                Vector2.Distance(transform.position, player.position) <= detectionRadius)
                EnterCharging();
            else
            {
                currentState = FrogState.Patrolling;
                PickNewPatrolPoint();
            }
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void HandleFlip()
    {
        if (spriteRenderer == null) return;

        Vector2 target = currentState == FrogState.Jumping
            ? lockedJumpTarget
            : (player != null && currentState == FrogState.Charging
                ? (Vector2)player.position
                : currentPatrolTarget);

        spriteRenderer.flipX = target.x < transform.position.x;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? (Vector3)patrolCenter : transform.position,
            detectionRadius
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            Application.isPlaying ? (Vector3)patrolCenter : transform.position,
            patrolRadius
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}