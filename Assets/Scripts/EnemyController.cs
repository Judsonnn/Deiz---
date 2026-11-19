using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int damage = 1;

    private Transform player;
    private bool playerDetected;

    private bool canDamage = true;
    public float damageCooldown = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;

    [Header("Patrulha")]
    public bool patrolRight = true;
    public float patrolDistance = 2f;
    public float patrolSpeed = 1.5f;

    private Vector3 patrolStartPosition;
    private int patrolDirection = 1;

    [Header("Aceleração ao se aproximar")]
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public float accelerateDistance = 5f;

    [Header("Player Acima")]
    public float heightIgnoreThreshold = 1f;
    public float aboveHorizontalRange = 1f;

    [Header("Perder o Player")]
    public float loseDistance = 10f;
    public float loseTime = 3f;
    public float redetectDelay = 1f;
    public LayerMask obstacleLayer;

    [Header("Áudio")]
    public float alertInterval = 4f;

    private EnemyAudio enemyAudio;
    private float nextAlertTime;
    private float lostTimer;
    private float blockDetectUntil;

    private float currentSpeed;

    void Start()
    {
        patrolStartPosition = transform.position;
        patrolDirection = patrolRight ? 1 : -1;
        enemyAudio = GetComponent<EnemyAudio>();
    }

    void Update()
    {
        if (!playerDetected)
        {
            Patrol();
            return;
        }

        if (player == null)
        {
            LosePlayer();
            return;
        }

        if (CheckLostPlayer())
        {
            LosePlayer();
            return;
        }

        AttackPlayer();
    }

    private bool CheckLostPlayer()
    {
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > loseDistance)
            return true;

        if (!HasLineOfSight(player))
        {
            lostTimer += Time.deltaTime;

            if (lostTimer >= loseTime)
                return true;
        }
        else
        {
            lostTimer = 0f;
        }

        return false;
    }

    private void LosePlayer()
    {
        playerDetected = false;
        player = null;
        lostTimer = 0f;
        blockDetectUntil = Time.time + redetectDelay;

        if (enemyAudio != null)
        {
            enemyAudio.StopChase();
        }
    }

    private void Patrol()
    {
        bool groundAhead = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (!groundAhead)
            return;

        float rightLimit = patrolStartPosition.x + patrolDistance;
        float leftLimit = patrolStartPosition.x - patrolDistance;

        if (transform.position.x >= rightLimit)
        {
            patrolDirection = -1;
        }

        if (transform.position.x <= leftLimit)
        {
            patrolDirection = 1;
        }

        transform.Translate(
            Vector2.right * patrolDirection * patrolSpeed * Time.deltaTime
        );

        if (patrolDirection > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void AttackPlayer()
    {
        bool groundAhead = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        float heightDifference = player.position.y - transform.position.y;

        float distanceToPlayer = Mathf.Abs(
            player.position.x - transform.position.x
        );

        bool playerIsAbove =
            heightDifference > heightIgnoreThreshold &&
            distanceToPlayer <= aboveHorizontalRange;

        if (playerIsAbove)
        {
            return;
        }

        if (Time.time >= nextAlertTime)
        {
            if (enemyAudio != null)
            {
                enemyAudio.PlayAlert();
            }

            nextAlertTime = Time.time + alertInterval;
        }

        float proximityFactor = 1f - Mathf.Clamp01(
            distanceToPlayer / accelerateDistance
        );

        currentSpeed = Mathf.Lerp(
            minSpeed,
            maxSpeed,
            proximityFactor
        );

        Vector2 targetPosition = new Vector2(
            player.position.x,
            transform.position.y
        );

        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);

            if (groundAhead)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    currentSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);

            if (groundAhead)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    currentSpeed * Time.deltaTime
                );
            }
        }
    }

    public void DetectPlayer(Transform target)
    {
        if (playerDetected)
            return;

        if (Time.time < blockDetectUntil)
            return;

        if (Vector2.Distance(transform.position, target.position) > loseDistance)
            return;

        player = target;
        playerDetected = true;
        lostTimer = 0f;
        nextAlertTime = Time.time + alertInterval;

        if (enemyAudio != null)
        {
            enemyAudio.StartChase();
        }
    }

    public bool HasLineOfSight(Transform target)
    {
        if (obstacleLayer.value == 0)
            return true;

        return !Physics2D.Linecast(
            transform.position,
            target.position,
            obstacleLayer
        );
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!canDamage)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        ContactPoint2D contact = collision.GetContact(0);

        if (contact.normal.y > 0.5f)
        {
            return;
        }

        PlayerController playerController =
            collision.gameObject.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.TakeDamage(
                damage,
                transform
            );

            canDamage = false;
            Invoke(nameof(ResetDamage), damageCooldown);
        }
    }

    private void ResetDamage()
    {
        canDamage = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * groundCheckDistance
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            transform.position + Vector3.left * patrolDistance,
            transform.position + Vector3.right * patrolDistance
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position + Vector3.up * heightIgnoreThreshold,
            aboveHorizontalRange
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, loseDistance);
    }
}