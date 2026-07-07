// ─────────────────────────────────────────
// EnemyHealth.cs — coloca em todo inimigo
// ─────────────────────────────────────────
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("Barra de Vida")]
    public EnemyHealthBar healthBar;

    [Header("Knockback ao tomar dano")]
    public float knockbackForce = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        if (healthBar != null)
            healthBar.Hide(); // começa escondida
    }

    public void TakeDamage(int damage, Transform attacker)
    {
        currentHealth -= damage;

        // Mostra a barra no primeiro dano e atualiza
        if (healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateBar(currentHealth, maxHealth);
        }

        // Knockback ao tomar dano — evita deslizar
        if (rb != null && attacker != null)
        {
            float direction = transform.position.x > attacker.position.x ? 1f : -1f;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction, 0.5f).normalized * knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}