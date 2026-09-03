using UnityEngine;

public class AerialEnemyController : MonoBehaviour
{
    public enum EnemyState { Patrolling, Positioning, Telegraph, Diving, Searching }

    [Header("Patrol")]
    public float patrolRadius = 4f;
    public float patrolSpeed = 2f;
    public float patrolPointWaitTime = 1.5f;

    [Header("Detection")]
    public float detectionRadius = 6f;

    [Header("Positioning (voo até o ponto de flanco)")]
    public float positioningSpeed = 4.5f;
    public float flankHeight = 3f;
    public float flankHorizontalOffset = 2.5f;
    public float positioningArriveThreshold = 0.3f;

    [Header("Telegraph (pausa antes do ataque)")]
    public float telegraphTime = 1.2f;

    [Header("Dive Attack")]
    public float diveSpeed = 9f;
    public float diveOvershoot = 1.5f;
    public float attackCooldown = 2f;

    [Header("Detecção de Chão (Raycast)")]
    public LayerMask groundLayer;
    public float minHeightAboveGround = 1.5f; // altura mínima que o drone mantém acima de qualquer superfície
    public float groundCheckDistance = 20f;   // até onde o raycast procura chão abaixo

    [Header("Search")]
    public float searchTime = 3f;

    [Header("Damage")]
    public int damage = 1;
    public float damageCooldown = 1f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    private EnemyState currentState = EnemyState.Patrolling;

    private Transform player;
    private Vector2 patrolCenter;
    private Vector2 currentPatrolTarget;

    private Vector2 flankTarget;
    private Vector2 diveTarget;
    private Vector2 diveDirection;
    private float diveTraveled;

    private float patrolWaitTimer;
    private float searchTimer;
    private float telegraphTimer;
    private float attackCooldownTimer;

    private bool canDamage = true;
    private bool playerInRange = false;

    void Start()
    {
        patrolCenter = transform.position;
        PickNewPatrolPoint();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        CheckPlayerVisibility();

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Patrolling:  HandlePatrol();      break;
            case EnemyState.Positioning: HandlePositioning(); break;
            case EnemyState.Telegraph:   HandleTelegraph();   break;
            case EnemyState.Diving:      HandleDive();        break;
            case EnemyState.Searching:   HandleSearch();      break;
        }

        HandleFlip();
    }

    // ──────────────────────────────────────
    // Raycast de chão — retorna a altura mínima
    // segura (Y do chão + margem) para uma
    // posição X qualquer
    // ──────────────────────────────────────
    private float GetMinSafeHeight(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, groundCheckDistance, groundLayer);

        if (hit.collider != null)
            return hit.point.y + minHeightAboveGround;

        // Não achou chão abaixo — não força clamp (deixa passar livre)
        return float.NegativeInfinity;
    }

    private Vector2 ClampAboveGround(Vector2 position)
    {
        float minSafeY = GetMinSafeHeight(position);
        position.y = Mathf.Max(position.y, minSafeY);
        return position;
    }

    // ──────────────────────────────────────
    // Visibilidade
    // ──────────────────────────────────────
    private void CheckPlayerVisibility()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= detectionRadius;

        if (playerInRange)
        {
            if (currentState == EnemyState.Patrolling || currentState == EnemyState.Searching)
            {
                if (attackCooldownTimer <= 0f)
                    StartPositioning();
            }
        }
        else
        {
            if (currentState == EnemyState.Positioning)
            {
                currentState = EnemyState.Searching;
                searchTimer = searchTime;
            }
        }
    }

    // ──────────────────────────────────────
    // Patrulha
    // ──────────────────────────────────────
    private void HandlePatrol()
    {
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
        Vector2 target = patrolCenter + randomOffset;
        currentPatrolTarget = ClampAboveGround(target);
        patrolWaitTimer = patrolPointWaitTime;
    }

    // ──────────────────────────────────────
    // Positioning
    // ──────────────────────────────────────
    private void StartPositioning()
    {
        currentState = EnemyState.Positioning;
        RecalculateFlankTarget();
    }

    private void RecalculateFlankTarget()
    {
        if (player == null) return;

        float side = -Mathf.Sign(player.position.x - transform.position.x);
        if (side == 0f) side = 1f;

        Vector2 target = (Vector2)player.position
            + Vector2.up * flankHeight
            + Vector2.right * flankHorizontalOffset * side;

        flankTarget = ClampAboveGround(target);
    }

    private void HandlePositioning()
    {
        if (player == null) return;

        RecalculateFlankTarget();
        MoveTowards(flankTarget, positioningSpeed);

        if (Vector2.Distance(transform.position, flankTarget) <= positioningArriveThreshold)
        {
            currentState = EnemyState.Telegraph;
            telegraphTimer = telegraphTime;
        }
    }

    // ──────────────────────────────────────
    // Telegraph
    // ──────────────────────────────────────
    private void HandleTelegraph()
    {
        telegraphTimer -= Time.deltaTime;

        if (telegraphTimer <= 0f)
        {
            StartDive();
        }
    }

    // ──────────────────────────────────────
    // Dive
    // ──────────────────────────────────────
    private void StartDive()
    {
        currentState = EnemyState.Diving;

        Vector2 targetAtAttackTime = player != null ? (Vector2)player.position : (Vector2)transform.position;
        diveDirection = (targetAtAttackTime - (Vector2)transform.position).normalized;
        Vector2 target = targetAtAttackTime + diveDirection * diveOvershoot;

        diveTarget = ClampAboveGround(target);
        diveTraveled = 0f;
    }

    private void HandleDive()
    {
        Vector2 previousPos = transform.position;

        Vector2 newPos = Vector2.MoveTowards(
            transform.position,
            diveTarget,
            diveSpeed * Time.deltaTime
        );

        // Clamp contínuo via raycast — nenhum frame passa da superfície abaixo dele
        newPos = ClampAboveGround(newPos);

        transform.position = newPos;
        diveTraveled += Vector2.Distance(previousPos, transform.position);

        if (Vector2.Distance(transform.position, diveTarget) < 0.1f)
        {
            EndDive();
        }
    }

    private void EndDive()
    {
        attackCooldownTimer = attackCooldown;
        currentState = EnemyState.Searching;
        searchTimer = searchTime;
    }

    // ──────────────────────────────────────
    // Search
    // ──────────────────────────────────────
    private void HandleSearch()
    {
        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            currentState = EnemyState.Patrolling;
            PickNewPatrolPoint();
            return;
        }

        if (playerInRange && attackCooldownTimer <= 0f)
        {
            StartPositioning();
        }
    }

    // ──────────────────────────────────────
    // Movimento suave
    // ──────────────────────────────────────
    private void MoveTowards(Vector2 target, float speed)
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }

    // ──────────────────────────────────────
    // Flip
    // ──────────────────────────────────────
    private void HandleFlip()
    {
        if (spriteRenderer == null) return;

        Vector2 target = currentState == EnemyState.Diving
            ? diveTarget
            : (currentState == EnemyState.Positioning ? flankTarget : currentPatrolTarget);

        spriteRenderer.flipX = target.x < transform.position.x;
    }

    // ──────────────────────────────────────
    // Dano por contato
    // ──────────────────────────────────────
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!canDamage) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.TakeDamage(damage, transform);
            canDamage = false;
            Invoke(nameof(ResetDamage), damageCooldown);
        }
    }

    private void ResetDamage()
    {
        canDamage = true;
    }

    // ──────────────────────────────────────
    // Gizmos
    // ──────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)patrolCenter : transform.position, patrolRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Raycast de chão a partir da posição atual
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

        if (Application.isPlaying && currentState == EnemyState.Positioning)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(flankTarget, 0.3f);
        }

        if (Application.isPlaying && currentState == EnemyState.Diving)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, diveTarget);
        }
    }
}