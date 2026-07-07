// ─────────────────────────────────────────
// Bullet.cs — coloca no prefab do tiro
// ─────────────────────────────────────────
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private float direction;
    private int damage;
    public float lifetime = 3f; // some após 3 segundos

    public void Init(float direction, float speed, int damage)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;

        // Vira o sprite do tiro conforme direção
        if (direction < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Acerta inimigo
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform);
            Destroy(gameObject);
            return;
        }

        // Acerta chão ou parede
        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}