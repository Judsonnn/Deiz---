using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;

    [Header("Jump")]
    public float firstJumpForce = 13f;
    public float secondJumpForce = 11f;
    public int maxJumps = 2;

    [Header("Jump Keys")]
    public KeyCode jumpKeyPrimary = KeyCode.Space;
    public KeyCode jumpKeyAlt1 = KeyCode.UpArrow;
    public KeyCode jumpKeyAlt2 = KeyCode.W;

    [Header("Jump Gravity")]
    public float normalGravity = 2f;
    public float fallGravity = 5f;
    public float lowJumpGravity = 4f;
    public float maxFallSpeed = 14f;

    [Header("Jump Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;

    [Range(0f, 1f)]
    public float jumpVolume = 0.5f;

    [Header("Footstep Audio")]
    [Tooltip("AudioSource separado só para os passos (evita conflito de pitch com o pulo/dano).")]
    public AudioSource stepAudioSource;
    [Tooltip("Pode colocar vários sons; um é sorteado a cada passo.")]
    public AudioClip[] footstepSounds;
    [Range(0f, 1f)]
    public float footstepVolume = 0.4f;
    [Tooltip("Tempo (segundos) entre um passo e outro.")]
    public float stepInterval = 0.35f;
    [Tooltip("Variação aleatória de pitch pra não ficar repetitivo.")]
    public Vector2 stepPitchRange = new Vector2(0.9f, 1.1f);

    [Header("Damage Audio")]
    // Usa o mesmo audioSource do pulo. É só arrastar o som no Inspector.
    public AudioClip damageSound;

    [Range(0f, 1f)]
    public float damageVolume = 0.7f;

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

    [Header("Dano Visual")]
    public float blinkDuration = 1f;
    public float blinkInterval = 0.1f;

    public int health = 3;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGrounded;
    private int jumpCount;
    private bool takingDamage = false;
    private Vector3 cameraTargetPosition;
    private PlayerShooter shooter;

    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    private float stepTimer;
    private int lastStepIndex = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = normalGravity;
        transform.localScale = Vector3.one;
        shooter = GetComponent<PlayerShooter>();

        // ── CHECKPOINT ──────────────────────────────────────────────────────
        if (GameManager.Instance != null &&
            GameManager.Instance.TryGetCheckpoint(out Vector3 checkpointPos))
        {
            transform.position = checkpointPos;
        }
        // ────────────────────────────────────────────────────────────────────

        if (cameraTarget != null)
            cameraTargetPosition = cameraTarget.localPosition;
    }

    void Update()
    {
        HandleMovement();
        HandleGroundCheck();
        HandleFootsteps();
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
            if (move > 0)
            {
                spriteRenderer.flipX = false;
                shooter?.SetFacing(true);
            }
            else if (move < 0)
            {
                spriteRenderer.flipX = true;
                shooter?.SetFacing(false);
            }
        }

        if (!takingDamage)
            rb.linearVelocity = new Vector2(
                move * speed,
                rb.linearVelocity.y
            );
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

            if (!wasGrounded || jumpCount > 1)
                jumpCount = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;

            if (wasGrounded && jumpCount == 0)
                jumpCount = 1;
        }
    }

    // ── PASSOS ──────────────────────────────────────────────────────────────
    private void HandleFootsteps()
    {
        float move = Input.GetAxisRaw("Horizontal");

        bool walking = isGrounded
            && Mathf.Abs(move) > 0.01f
            && !takingDamage;

        if (!walking)
        {
            // zera o timer: o primeiro passo toca assim que voltar a andar
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    private void PlayFootstep()
    {
        if (stepAudioSource == null || footstepSounds == null || footstepSounds.Length == 0)
            return;

        // sorteia um clip, evitando repetir o mesmo seguido
        int index = Random.Range(0, footstepSounds.Length);

        if (footstepSounds.Length > 1 && index == lastStepIndex)
            index = (index + 1) % footstepSounds.Length;

        lastStepIndex = index;

        AudioClip clip = footstepSounds[index];
        if (clip == null)
            return;

        stepAudioSource.pitch = Random.Range(stepPitchRange.x, stepPitchRange.y);
        stepAudioSource.PlayOneShot(clip, footstepVolume);
    }
    // ────────────────────────────────────────────────────────────────────────

    private bool JumpPressed()
    {
        return Input.GetKeyDown(jumpKeyPrimary)
            || Input.GetKeyDown(jumpKeyAlt1)
            || Input.GetKeyDown(jumpKeyAlt2);
    }

    private bool JumpHeld()
    {
        return Input.GetKey(jumpKeyPrimary)
            || Input.GetKey(jumpKeyAlt1)
            || Input.GetKey(jumpKeyAlt2);
    }

    private void HandleJump()
    {
        if (!JumpPressed())
            return;

        // Primeiro pulo
        if (coyoteTimeCounter > 0f && jumpCount <= 1)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                firstJumpForce
            );

            jumpCount = 1;
            coyoteTimeCounter = 0f;

            PlayJumpSound();
        }
        // Segundo pulo
        else if (jumpCount == 1)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                secondJumpForce
            );

            jumpCount = 2;

            PlayJumpSound();
        }
    }

    private void PlayJumpSound()
    {
        if (audioSource != null && jumpSound != null)
        {
            audioSource.PlayOneShot(
                jumpSound,
                jumpVolume
            );
        }
    }

    // ── DANO (áudio) ────────────────────────────────────────────────────────
    private void PlayDamageSound()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(
                damageSound,
                damageVolume
            );
        }
    }
    // ────────────────────────────────────────────────────────────────────────

    private void HandleGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravity;

            if (rb.linearVelocity.y < -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    -maxFallSpeed
                );
            }
        }
        else if (rb.linearVelocity.y > 0 && !JumpHeld())
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
        if (cameraTarget == null)
            return;

        float move = Input.GetAxisRaw("Horizontal");

        if (move > 0)
        {
            cameraTargetPosition = new Vector3(
                lookAheadDistance,
                0f,
                cameraTarget.localPosition.z
            );
        }
        else if (move < 0)
        {
            cameraTargetPosition = new Vector3(
                -lookAheadDistance,
                0f,
                cameraTarget.localPosition.z
            );
        }
        else
        {
            cameraTargetPosition = new Vector3(
                0f,
                0f,
                cameraTarget.localPosition.z
            );
        }

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
        TriggerBlink();
        PlayDamageSound();

        float direction =
            transform.position.x > enemy.position.x ? 1f : -1f;

        Vector2 knockbackDirection =
            new Vector2(direction, 1f).normalized;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            knockbackDirection * knockbackForce,
            ForceMode2D.Impulse
        );

        Invoke(nameof(StopTakingDamage), 0.3f);
    }

    public void TakeDamage(int damage)
    {
        HeartSystem heart = GetComponent<HeartSystem>();

        if (heart != null)
            heart.vida -= damage;
        else
            health -= damage;

        PlayDamageSound();
    }

    private void StopTakingDamage()
    {
        takingDamage = false;
    }

    private void TriggerBlink()
    {
        StopCoroutine(nameof(BlinkCoroutine));
        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);

            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            groundCheck.position,
            groundCheckSize
        );
    }
}