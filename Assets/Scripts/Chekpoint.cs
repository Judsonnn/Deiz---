using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite spriteInativo;
    public Sprite spriteAtivo;
    public ParticleSystem particulasAgua;

    private bool visualAtivado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.SetCheckpoint(transform.position);
        else
            Debug.LogWarning("GameManager não encontrado na cena!");

        if (!visualAtivado)
        {
            visualAtivado = true;

            if (spriteRenderer != null && spriteAtivo != null)
                spriteRenderer.sprite = spriteAtivo;

            if (particulasAgua != null)
                particulasAgua.Play();
        }
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.isTrigger = true;
    }
}