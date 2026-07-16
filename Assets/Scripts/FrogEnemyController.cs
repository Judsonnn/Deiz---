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

        // Trava o alvo do pulo logo no início da carga, assim o indicador
        // já nasce na posição correta (onde o dano vai acontecer de fato)
        if (player != null)
            lockedJumpTarget = player.position;

        UpdateGroundIndicatorAt(lockedJumpTarget);
    }

    private void HandleCharging()
    {
        chargeTimer -= Time.deltaTime;

        // Mantém o indicador fixo embaixo do alvo travado (não do sapo)
        UpdateGroundIndicatorAt(lockedJumpTarget);

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
        rb.gravityScale = 2f;

        // Mantém o indicador visível já no instante em que o sapo sai do chão,
        // em vez de esconder e só reaparecer no pico do pulo
        UpdateGroundIndicatorAt(lockedJumpTarget);

        jumpDirection = lockedJumpTarget.x > transform.position.x ? 1f : -1f;
        shouldJump = true;
    }

    private void HandleJumping()
    {
        jumpGroundCheckTimer -= Time.deltaTime;

        if (rb.linearVelocity.y < 0)
            hasReachedPeak = true;

        // Mantém a área atualizada durante toda a trajetória (subida e queda),
        // não só depois do pico
        UpdateGroundIndicatorAt(lockedJumpTarget);

        if (jumpGroundCheckTimer <= 0f && hasReachedPeak && IsGrounded())
        {
            rb.linearVelocity = Vector2.zero;
            OnLand();
            currentState = FrogState.Landing;
        }
    }

    // Método único responsável por posicionar o indicador de área no chão,
    // sempre usando o X do alvo travado (lockedJumpTarget), não a posição do sapo.
    private void UpdateGroundIndicatorAt(Vector2 targetPos)
    {
        if (areaIndicator == null) return;

        // Origem do raycast bem acima do alvo, garante que pega o chão
        // mesmo que o sapo esteja mais alto ou mais baixo que o ponto de queda
        Vector2 rayOrigin = new Vector2(targetPos.x, transform.position.y + 10f);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 30f, groundLayer);

        if (hit.collider != null)
        {
            areaIndicator.SetActive(true);

            // Encosta quase no chão (bem rente, sem ficar dentro do chão)
            areaIndicator.transform.position = new Vector3(
                targetPos.x,
                hit.point.y + 0.02f,
                areaIndicator.transform.position.z
            );

            // Largura desejada = diâmetro real da área de dano (areaRadius * 2)
            // Altura desejada = espessura fina configurável (indicatorThickness)
            float desiredWidth = areaRadius * 2f;
            float desiredHeight = indicatorThickness;

            // Converte tamanho desejado (em unidades do mundo) em escala local,
            // levando em conta o tamanho nativo do sprite. Isso garante que o
            // indicador cubra EXATAMENTE a área de dano, não importa o sprite usado.
            float scaleX = desiredWidth / Mathf.Max(areaIndicatorNativeSize.x, 0.0001f);
            float scaleY = desiredHeight / Mathf.Max(areaIndicatorNativeSize.y, 0.0001f);

            areaIndicator.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
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
        areaIndicator.SetActive(true);
        areaIndicator.transform.position = new Vector3(
            lockedJumpTarget.x,
            lockedJumpTarget.y,
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