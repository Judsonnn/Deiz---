using UnityEngine;

public class SpringPlatform : MonoBehaviour
{
    [Header("Impulso")]
    [Tooltip("Força vertical aplicada no player ao pousar em cima")]
    public float bounceForce = 20f;
    [Tooltip("Mantém a velocidade horizontal do player ao quicar")]
    public bool keepHorizontalVelocity = true;

    [Header("Detecção do Player em cima")]
    [Tooltip("Altura mínima do ponto de contato pra considerar que o player está em cima")]
    public float topDetectionHeight = 0.3f;

    [Header("Squash & Stretch (feedback visual)")]
    [Tooltip("Quanto a plataforma achata no momento do impacto")]
    public float squashAmount = 0.6f;
    [Tooltip("Velocidade de voltar ao tamanho normal")]
    public float stretchSpeed = 8f;

    private Vector3 originalScale;
    private bool isSqueezed = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Volta suavemente ao tamanho original depois do squash
        if (isSqueezed)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                stretchSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.localScale, originalScale) < 0.01f)
            {
                transform.localScale = originalScale;
                isSqueezed = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Verifica se o player está vindo de cima (não do lado nem de baixo)
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y >= transform.position.y + topDetectionHeight * 0.5f)
            {
                ApplyBounce(collision.rigidbody);
                return;
            }
        }
    }

    private void ApplyBounce(Rigidbody2D playerRb)
    {
        if (playerRb == null) return;

        float horizontalVelocity = keepHorizontalVelocity ? playerRb.linearVelocity.x : 0f;
        playerRb.linearVelocity = new Vector2(horizontalVelocity, bounceForce);

        // Squash visual no momento do impacto
        TriggerSquash();
    }

    private void TriggerSquash()
    {
        // Achata no Y e estica no X pra dar sensação de compressão
        transform.localScale = new Vector3(
            originalScale.x * (1f + (1f - squashAmount) * 0.5f),
            originalScale.y * squashAmount,
            originalScale.z
        );
        isSqueezed = true;
    }

    private void OnDrawGizmosSelected()
    {
        // Linha mostrando a altura de detecção do topo
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 0.5f, transform.position.y + topDetectionHeight * 0.5f),
            new Vector3(transform.position.x + 0.5f, transform.position.y + topDetectionHeight * 0.5f)
        );
    }
}