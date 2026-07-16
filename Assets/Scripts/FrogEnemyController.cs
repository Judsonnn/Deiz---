using UnityEngine;
using System.Collections;

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
    public float jumpForce = 18f;
    public float jumpHorizontalSpeed = 10f;
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
    private EnemyHealth enemyHealth;
    private Vector2 patrolCenter;
    private Vector2 currentPatrolTarget;
    private Vector2 lockedJumpTarget;

    private float chargeTimer;
    private float cooldownTimer;
    private float patrolWaitTimer;

    private bool hasReachedPeak = false;
    private float jumpGroundCheckDelay = 0.3f;
    private float jumpGroundCheckTimer;

    private bool shouldJump = false;
    private float jumpDirection = 1f;

    private Collider2D myCollider;
    private Collider2D playerCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        enemyHealth = GetComponent<EnemyHealth>();
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
            case FrogState.Landing:   HandleLanding();  break;
            case FrogState.Cooldown:  HandleCooldown(); break;
        }

        HandleFlip();
    }

    void FixedUpdate()
    {
        // Aplica a força no FixedUpdate
        if (shouldJump)
        {
            shouldJump = false;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(jumpDirection * jumpHorizontalSpeed, jumpForce), ForceMode2D.Impulse);
            Debug.Log("Força aplicada: " + rb.linearVelocity);
        }

        if (currentState == FrogState.Jumping)
            HandleJumping();
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
        chargeTimer -= Time.deltaTime;

        if (areaIndicator != null)
        {
            // Raio para achar o chão abaixo do SAPO
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                20f,
                groundLayer
            );

            if (hit.collider != null)
            {
                areaIndicator.SetActive(true);
                areaIndicator.transform.position = new Vector3(
                    transform.position.x,
                    hit.point.y + 0.05f,
                    areaIndicator.transform.position.z
                );
                areaIndicator.transform.localScale = new Vector3(areaRadius * 2f, 0.3f, 1f);
            }
        }

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
        jumpGroundCheckTimer = jumpGroundCheckDelay;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;

        if (areaIndicator != null)
            areaIndicator.SetActive(false);

        jumpDirection = lockedJumpTarget.x > transform.position.x ? 1f : -1f;
        shouldJump = true;
    }

    private void HandleJumping()
    {
        jumpGroundCheckTimer -= Time.deltaTime;

        if (rb.linearVelocity.y < 0)
            hasReachedPeak = true;

        // Atualiza posição da área no chão abaixo do sapo durante a queda
        if (areaIndicator != null && hasReachedPeak)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.down,
                20f,
                groundLayer
            );

            if (hit.collider != null)
            {
                areaIndicator.SetActive(true);
                areaIndicator.transform.position = new Vector3(
                    transform.position.x,
                    hit.point.y + 0.05f,
                    areaIndicator.transform.position.z
                );
                areaIndicator.transform.localScale = new Vector3(areaRadius * 2f, 0.3f, 1f);
            }
        }

        if (jumpGroundCheckTimer <= 0f && hasReachedPeak && IsGrounded())
        {
            rb.linearVelocity = Vector2.zero;
            OnLand();
            currentState = FrogState.Landing;
        }
    }

    private void OnLand()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            areaRadius,
            playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            // Busca o PlayerController no objeto ou em qualquer pai
            PlayerController pc = hit.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(damage, transform);
                Debug.Log("Dano aplicado!");
            }
            else
            {
                Debug.Log("PlayerController não encontrado em: " + hit.gameObject.name);
            }
        }

        if (areaIndicator != null)
            StartCoroutine(ShowImpactEffect());
    }

    private IEnumerator ShowImpactEffect()
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
            float dist = Vector2.Distance(transform.position, player.position);
            Debug.Log("Distância: " + dist + " | Radius: " + detectionRadius);

            if (player != null && dist <= detectionRadius)
                EnterCharging();
            else
            {
                currentState = FrogState.Patrolling;
                PickNewPatrolPoint();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != FrogState.Jumping) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        if (pc != null)
            pc.TakeDamage(damage, transform);
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
        bool grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
        Debug.Log("GroundCheck pos: " + groundCheck.position + " | grounded: " + grounded);
        return grounded;
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