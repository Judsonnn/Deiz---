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

    // Distância que o inimigo pode andar para cada lado.
    // Exemplo: 2 = aproximadamente 2 unidades para cada lado.
    public float patrolDistance = 2f;

    // Velocidade durante a patrulha
    public float patrolSpeed = 1.5f;

    private Vector3 patrolStartPosition;
    private int patrolDirection = 1;

    [Header("Aceleração ao se aproximar")]
    public float minSpeed = 2f;
    public float maxSpeed = 6f;
    public float accelerateDistance = 5f;

    [Header("Player Acima")]
    // Diferença de altura mínima pra considerar que o player
    // está "acima" do inimigo (ex: pulou em cima dele).
    public float heightIgnoreThreshold = 1f;

    // Distância horizontal máxima pra considerar que o player
    // está "em cima" do inimigo. Se estiver mais longe que isso
    // horizontalmente, o inimigo continua perseguindo normalmente
    // mesmo com o player mais alto.
    public float aboveHorizontalRange = 1f;

    private float currentSpeed;

    void Start()
    {
        // Guarda a posição inicial para definir os limites da patrulha
        patrolStartPosition = transform.position;

        // Define o lado inicial
        patrolDirection = patrolRight ? 1 : -1;
    }

    void Update()
    {
        // Se ainda não detectou o player, fica patrulhando
        if (!playerDetected)
        {
            Patrol();
            return;
        }

        if (player == null)
            return;

        AttackPlayer();
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

        // Limites da patrulha
        float rightLimit = patrolStartPosition.x + patrolDistance;
        float leftLimit = patrolStartPosition.x - patrolDistance;

        // Se chegou ao limite direito, começa a voltar
        if (transform.position.x >= rightLimit)
        {
            patrolDirection = -1;
        }

        // Se chegou ao limite esquerdo, começa a voltar
        if (transform.position.x <= leftLimit)
        {
            patrolDirection = 1;
        }

        // Move o inimigo
        transform.Translate(
            Vector2.right * patrolDirection * patrolSpeed * Time.deltaTime
        );

        // Vira o sprite de acordo com a direção
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

        // Só ignora a perseguição se o player estiver ACIMA
        // e PERTO horizontalmente (ou seja, realmente em cima dele).
        // Se estiver longe, mesmo mais alto, continua perseguindo normal.
        bool playerIsAbove =
            heightDifference > heightIgnoreThreshold &&
            distanceToPlayer <= aboveHorizontalRange;

        if (playerIsAbove)
        {
            return;
        }

        // Calcula a velocidade baseada na distância até o player
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
        player = target;
        playerDetected = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!canDamage)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Verifica a normal do primeiro ponto de contato.
        // Se o player caiu em cima do inimigo, a normal aponta
        // predominantemente pra cima (Y positivo) — nesse caso,
        // não causa dano, só deixa o player "pisando" nele.
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

        // Mostra visualmente a área da patrulha
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(
            transform.position + Vector3.left * patrolDistance,
            transform.position + Vector3.right * patrolDistance
        );

        // Mostra a zona de "player acima" pra ajudar a calibrar
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position + Vector3.up * heightIgnoreThreshold,
            aboveHorizontalRange
        );
    }
}