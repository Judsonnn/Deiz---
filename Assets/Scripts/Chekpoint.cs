using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Tooltip("Opcional: troca o sprite quando o checkpoint é ativado (ex: bandeira abaixada -> levantada)")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteInativo;
    public Sprite spriteAtivo;

    private bool activated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;

        if (GameManager.Instance != null)
            GameManager.Instance.SetCheckpoint(transform.position);
        else
            Debug.LogWarning("GameManager não encontrado na cena! Crie um GameObject com o script GameManager.");

        if (spriteRenderer != null && spriteAtivo != null)
            spriteRenderer.sprite = spriteAtivo;
    }

    private void Reset()
    {
        // Garante que o collider desse objeto seja um trigger por padrão,
        // já que ele não deve bloquear o player fisicamente
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }
}