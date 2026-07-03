using UnityEngine;

public class AerialEnemyController : MonoBehaviour
{
    public enum EnemyState { Patrolling, Chasing, Searching }

    [Header("Patrol")]
    public float patrolRadius = 4f;
    public float patrolSpeed = 2f;
    public float patrolPointWaitTime = 1.5f;

    [Header("Chase")]
    public float chaseSpeed = 4.5f;
    public float detectionRadius = 6f;

    [Header("Search")]
    public float searchTime = 3f; // tempo sem ver o player antes de voltar a patrulhar

    [Header("Damage")]
    public int damage = 1;
    public float damageCooldown = 1f;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;

    // Estado atual
    private EnemyState currentState = EnemyState.Patrolling;

    // Referências
    private Transform player;
    private Vector2 patrolCenter;
    private Vector2 currentPatrolTarget;

    // Timers
    private float patrolWaitTimer;
    private float searchTimer;
    private float damageTimer;

    private bool canDamage = true;
    private bool playerInRange = false;

    void Start()
    {
        patrolCenter = transform.position;
        PickNewPatrolPoint();

        // Busca o player pela tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        CheckPlayerVisibility();

        switch (currentState)
        {
            case EnemyState.Patrolling: HandlePatrol(); break;
            case EnemyState.Chasing:   HandleChase();  break;
            case EnemyState.Searching: HandleSearch(); break;
        }

        HandleFlip();
    }

    // ──────────────────────────────────────
    // Visibilidade — checa se o player está
    // dentro do raio de detecção
    // ──────────────────────────────────────
    private void CheckPlayerVisibility()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        playerInRange = dist <= detectionRadius;

        if (playerInRange)
        {
            // Viu o player — começa a perseguir
            if (currentState != EnemyState.Chasing)
            {
                currentState = EnemyState.Chasing;
                searchTimer = searchTime;
            }
        }
        else
        {
            // Perdeu o player durante a perseguição
            if (currentState == EnemyState.Chasing)
            {
                currentState = EnemyState.Searching;
                searchTimer = searchTime;
            }
        }
    }

    // ──────────────────────────────────────
    // Patrulha aleatória dentro da área
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
        currentPatrolTarget = patrolCenter + randomOffset;
        patrolWaitTimer = patrolPointWaitTime;
    }

    // ──────────────────────────────────────
    // Perseguição
    // ──────────────────────────────────────
    private void HandleChase()
    {
        if (player == null) return;
        MoveTowards(player.position, chaseSpeed);
    }

    // ──────────────────────────────────────
    // Buscando — perdeu o player, aguarda
    // antes de voltar a patrulhar
    // ──────────────────────────────────────
    private void HandleSearch()
    {
        // Fica parado ou se move devagar pelo último ponto
        MoveTowards(currentPatrolTarget, patrolSpeed * 0.5f);

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0f)
        {
            // Desistiu — volta a patrulhar
            currentState = EnemyState.Patrolling;
            PickNewPatrolPoint();
        }

        // Se o player aparecer de novo durante a busca
        if (playerInRange)
        {
            currentState = EnemyState.Chasing;
            searchTimer = searchTime;
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
    // Vira o sprite conforme direção
    // ──────────────────────────────────────
    private void HandleFlip()
    {
        if (spriteRenderer == null) return;

        Vector2 target = currentState == EnemyState.Chasing && player != null
            ? (Vector2)player.position
            : currentPatrolTarget;

        spriteRenderer.flipX = target.x < transform.position.x;
    }

    // ──────────────────────────────────────
    // Dano por contato — compatível com
    // HeartSystem e PlayerController
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
    // Gizmos — mostra raio de detecção
    // e área de patrulha no Editor
    // ──────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        // Área de patrulha
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)patrolCenter : transform.position, patrolRadius);

        // Raio de detecção
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}