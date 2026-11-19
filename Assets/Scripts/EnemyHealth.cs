using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Barra de Vida")]
    public EnemyHealthBar healthBar;

    [Header("Knockback ao tomar dano")]
    public float knockbackForce = 5f;
    public bool canBeKnockedBack = true;
    private Rigidbody2D rb;

    [Header("Piscar ao tomar dano")]
    public float blinkDuration = 0.4f;
    public float blinkInterval = 0.08f;
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    [Header("Som de Dano (inimigos sem EnemyAudio)")]
    public AudioClip hurtSound;
    [Range(0f, 1f)] public float hurtVolume = 1f;
    public AudioMixerGroup sfxGroup;

    private EnemyAudio enemyAudio;
    private BossController boss;

    private void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        enemyAudio = GetComponent<EnemyAudio>();
        boss = GetComponent<BossController>();

        if (healthBar != null)
            healthBar.Hide();
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        if (boss != null && boss.Invulneravel)
            return;

        currentHealth -= damage;

        if (enemyAudio != null)
            enemyAudio.PlayHurt(currentHealth <= 0);
        else
            PlayHurtSound();

        if (healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateBar(currentHealth, maxHealth);
        }

        if (canBeKnockedBack && rb != null && attacker != null)
        {
            float direction =
                transform.position.x > attacker.position.x ? 1f : -1f;

            rb.linearVelocity = Vector2.zero;

            rb.AddForce(
                new Vector2(direction, 0.3f).normalized * knockbackForce,
                ForceMode2D.Impulse
            );
        }

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkCoroutine());

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (boss != null)
            boss.OnVidaMudou((float)currentHealth / maxHealth);
    }

    private void PlayHurtSound()
    {
        if (hurtSound == null)
            return;

        GameObject temp = new GameObject("HurtSound");
        temp.transform.position = transform.position;

        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = hurtSound;
        src.volume = hurtVolume;
        src.spatialBlend = 0f;
        src.outputAudioMixerGroup = sfxGroup;
        src.Play();

        Destroy(temp, hurtSound.length);
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

    private void Die()
    {
        if (boss != null)
        {
            boss.Parar();

            VictoryScreenManager victory =
                FindFirstObjectByType<VictoryScreenManager>();

            if (victory != null)
            {
                victory.ShowVictoryScreen();
            }
            else
            {
                Debug.LogError(
                    "VictoryScreenManager não encontrado na cena!"
                );
            }
        }

        Destroy(gameObject);
    }
}