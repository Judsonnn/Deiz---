using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 2;
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
    private Coroutine blinkCoroutine; // << controla só o piscar

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (healthBar != null)
            healthBar.Hide();
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;

        if (healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateBar(currentHealth, maxHealth);
        }

        if (canBeKnockedBack && rb != null && attacker != null)
        {
            float direction = transform.position.x > attacker.position.x ? 1f : -1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction, 0.3f).normalized * knockbackForce, ForceMode2D.Impulse);
        }

        // Para só a coroutine do piscar, não todas
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkCoroutine());

        if (currentHealth <= 0)
            Die();
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
        Destroy(gameObject);
    }
}