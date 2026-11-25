
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float speed;
    private float direction;
    private int damage;

    public float lifetime = 3f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(float direction, float speed, int damage)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
