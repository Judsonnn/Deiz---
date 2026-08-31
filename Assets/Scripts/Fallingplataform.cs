using UnityEngine;
using System.Collections;

public class FallingPlatform : MonoBehaviour
{
    [Header("Tempo")]
    public float delayBeforeFall = 0.3f; // tempo antes de começar a cair
    public float respawnTime = 3f;       // tempo até voltar

    [Header("Queda")]
    public float fallGravity = 3f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Rigidbody2D rb;
    private Collider2D platformCollider;

    private bool activated = false;

    void Start()
    {
        // Guarda a posição original
        startPosition = transform.position;
        startRotation = transform.rotation;

        rb = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<Collider2D>();

        // A plataforma começa parada
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (activated)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        activated = true;

        StartCoroutine(FallPlatform());
    }

    private IEnumerator FallPlatform()
    {
        // Espera um pouco antes de cair
        yield return new WaitForSeconds(delayBeforeFall);

        // Ativa a física
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = fallGravity;
        }

        // Espera o tempo de respawn
        yield return new WaitForSeconds(respawnTime);

        Respawn();
    }

    private void Respawn()
    {
        // Para a física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        // Volta para a posição inicial
        transform.position = startPosition;
        transform.rotation = startRotation;

        activated = false;
    }
}
