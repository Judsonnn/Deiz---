using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float lifetime = 3f;

    private float speed;
    private float direction;
    private int damage;
    private Collider2D meuCollider;
    private bool usado;

    private void Awake()
    {
        Rigidbody2D corpo = GetComponent<Rigidbody2D>();

        if (corpo == null)
            corpo = gameObject.AddComponent<Rigidbody2D>();

        corpo.bodyType = RigidbodyType2D.Kinematic;
        corpo.gravityScale = 0f;
        corpo.linearVelocity = Vector2.zero;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
            col.isTrigger = true;

        meuCollider = GetComponentInChildren<Collider2D>();
    }

    public void Init(float direction, float speed, int damage)
    {
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
            sr.flipX = direction < 0;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (usado || meuCollider == null)
            return;

        Physics2D.SyncTransforms();

        Collider2D[] atingidos = Physics2D.OverlapBoxAll(
            meuCollider.bounds.center,
            meuCollider.bounds.size,
            0f
        );

        foreach (Collider2D col in atingidos)
        {
            if (col.transform.IsChildOf(transform))
                continue;

            if (col.GetComponentInParent<BossController>() != null)
                continue;

            PlayerController jogador = col.GetComponentInParent<PlayerController>();

            if (jogador != null)
            {
                usado = true;
                jogador.TakeDamage(damage, transform);
                Destroy(gameObject);
                return;
            }

            HeartSystem coracoes = col.GetComponentInParent<HeartSystem>();

            if (coracoes != null)
            {
                usado = true;
                coracoes.ReceberDano(damage);
                Destroy(gameObject);
                return;
            }

            if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                usado = true;
                Destroy(gameObject);
                return;
            }
        }
    }
}