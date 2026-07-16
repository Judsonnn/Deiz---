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
    [Tooltip("Raio usado DEPOIS que ele já engajou pelo menos uma vez (deve ser maior que o detectionRadius, pra não desistir fácil)")]
    public float chaseRadius = 9f;

    [Header("Attack")]
    public float chargeTime = 1.5f;
    public float jumpForce = 18f;           // Controla a ALTURA do pulo
    public float jumpHorizontalSpeed = 10f; // Controla a DISTÂNCIA horizontal do pulo
    public float jumpGravityScale = 2f;     // Gravidade durante o pulo (menor = pulo mais alto/lento)
    public float landingCooldown = 0.8f;
    public float areaRadius = 2.5f;
    public int damage = 1;
    public LayerMask playerLayer;

    [Header("Indicator")]
    public GameObject areaIndicator;
    public float indicatorFadeDuration = 0.5f;
    [Tooltip("Altura do indicador no chão (mais fino = valor menor)")]
    public float indicatorThickness = 0.12f;

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

    // Cache do tamanho nativo do sprite do indicador, usado para calcular
    // a escala correta (independente do tamanho da textura/pivot do sprite)
    private SpriteRenderer areaIndicatorRenderer;
    private Vector2 areaIndicatorNativeSize = Vector2.one;

    // Posição/escala já calculadas e travadas para o indicador de chão
    // (calculadas 1x quando o alvo é travado, evita "tremedeira" por recalcular todo frame)
    private Vector3 lockedIndicatorPosition;
    private Vector3 lockedIndicatorScale;
    private bool indicatorLocked = false;

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
        {
            areaIndicatorRenderer = areaIndicator.GetComponent<SpriteRenderer>();
            if (areaIndicatorRenderer != null && areaIndicatorRenderer.sprite != null)
            {
                // Tamanho do sprite em unidades do mundo com escala 1,1,1
                areaIndicatorNativeSize = areaIndicatorRenderer.sprite.bounds.size;
            }

            // Importante: se o indicador for filho do sapo na hierarquia, o
            // movimento físico do sapo (pulo) arrasta o filho entre um frame
            // e outro, mesmo travando a posição mundial no script. Desvincular
            // aqui garante que ele fique 100% parado, independente do sapo.
            areaIndicator.transform.SetParent(null);

            areaIndicator.SetActive(false);
        }
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

        // Trava o alvo do pulo logo no início da carga
        if (player != null)
            lockedJumpTarget = player.position;

        // Calcula a posição/escala do indicador UMA ÚNICA VEZ aqui.
        // Como o alvo já está travado, não precisa (e não deve) recalcular
        // todo frame depois disso — é isso que causava a "tremedeira".
        indicatorLocked = false;
        LockIndicatorAt(lockedJumpTarget);
    }

    private void HandleCharging()
    {
        chargeTimer -= Time.deltaTime;

        // Não recalcula mais aqui — o indicador já está travado desde EnterCharging.
        // Só reaplica os valores travados (sem raycast), caso algo tenha mexido nele.
        ApplyLockedIndicator();

        if (chargeTimer <= 0f)
        {
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        currentState = FrogState.Jumping;
        hasReachedPeak = false;
        jumpGroundCheckTimer = jumpGroundCheckDelay;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = jumpGravityScale;

        // Reaplica o valor JÁ TRAVADO (sem recalcular/raycast de novo),
        // assim o indicador continua visível e parado desde a saída do chão
        ApplyLockedIndicator();

        jumpDirection = lockedJumpTarget.x > transform.position.x ? 1f : -1f;
        shouldJump = true;
    }

    private void HandleJumping()
    {
        jumpGroundCheckTimer -= Time.deltaTime;

        if (rb.linearVelocity.y < 0)
            hasReachedPeak = true;

        // Reaplica o valor JÁ TRAVADO (sem raycast a cada frame) — mantém
        // o indicador 100% parado durante toda a trajetória do pulo
        ApplyLockedIndicator();

        if (jumpGroundCheckTimer <= 0f && hasReachedPeak && IsGrounded())
        {
            rb.linearVelocity = Vector2.zero;
            OnLand();
            currentState = FrogState.Landing;
        }
    }

    // Calcula a posição e escala do indicador de chão UMA VEZ (com raycast)
    // e trava esses valores em cache. Chamado só ao entrar em Charging.
    private void LockIndicatorAt(Vector2 targetPos)
    {
        if (areaIndicator == null) return;

        Vector2 rayOrigin = new Vector2(targetPos.x, transform.position.y + 10f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 30f, groundLayer);

        if (hit.collider == null) return;

        // Largura = diâmetro real da área de dano; altura = espessura fina configurável
        float desiredWidth = areaRadius * 2f;
        float desiredHeight = indicatorThickness;

        float scaleX = desiredWidth / Mathf.Max(areaIndicatorNativeSize.x, 0.0001f);
        float scaleY = desiredHeight / Mathf.Max(areaIndicatorNativeSize.y, 0.0001f);

        lockedIndicatorPosition = new Vector3(
            targetPos.x,
            hit.point.y + 0.02f,
            areaIndicator.transform.position.z
        );
        lockedIndicatorScale = new Vector3(scaleX, scaleY, 1f);
        indicatorLocked = true;

        ApplyLockedIndicator();
    }

    // Só reaplica os valores já travados (posição/escala), sem raycast nenhum.
    // Isso garante que o indicador fique 100% parado até a próxima vez que
    // for travado de novo (próxima carga).
    private void ApplyLockedIndicator()
    {
        if (areaIndicator == null || !indicatorLocked) return;

        areaIndicator.SetActive(true);
        areaIndicator.transform.position = lockedIndicatorPosition;
        areaIndicator.transform.localScale = lockedIndicatorScale;
    }

    private void OnLand()
    {
        // Usa o alvo travado, não a posição atual do sapo, para o dano
        // bater exatamente onde o indicador mostrou
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            lockedJumpTarget,
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
        // Não redimensiona mais nada aqui — só deixa a própria barra (que já
        // está na posição/escala certa, travada desde o pulo) sumir com fade.
        // Isso evita o "quadrado" grande que aparecia ao redimensionar pro
        // tamanho cheio da área de dano.
        SpriteRenderer indicator = areaIndicatorRenderer;
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

        indicatorLocked = false;
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
            Debug.Log("Distância: " + dist + " | ChaseRadius: " + chaseRadius);

            // Usa o chaseRadius (maior) aqui, e não o detectionRadius —
            // depois que já engajou uma vez, ele "persegue" com mais tolerância
            // e não desiste só porque você deu alguns passos pra trás.
            if (player != null && dist <= chaseRadius)
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

        Gizmos.color = new Color(1f, 0.5f, 0f); // laranja
        Gizmos.DrawWireSphere(
            Application.isPlaying ? (Vector3)patrolCenter : transform.position,
            chaseRadius
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