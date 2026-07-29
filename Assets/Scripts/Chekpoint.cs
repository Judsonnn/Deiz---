using UnityEngine;

// Coloque este script num GameObject com Collider2D marcado como "Is Trigger".
// Vira Prefab em Assets/Prefabs/Checkpoints, e as instâncias ficam dentro da
// pasta "-- CHECKPOINTS --" na Hierarchy da cena de gameplay.
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Opcional: troca o sprite quando o checkpoint é ativado (ex: bandeira abaixada -> levantada)")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteInativo;
    public Sprite spriteAtivo;

    // Controla só a ANIMAÇÃO/troca de sprite (pra não repetir toda vez que
    // o player passar de novo por um checkpoint já visitado)
    private bool visualAtivado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // A POSIÇÃO sempre atualiza, mesmo se o player passar de novo por
        // um checkpoint anterior (ex: andou pra trás por algum motivo) —
        // faz sentido que o checkpoint mais recente tocado seja sempre
        // o ponto de respawn atual.
        if (GameManager.Instance != null)
            GameManager.Instance.SetCheckpoint(transform.position);
        else
            Debug.LogWarning("GameManager não encontrado na cena! Confira se ele existe na cena MainMenu com DontDestroyOnLoad.");

        // O visual (sprite ativo) só troca UMA vez, na primeira visita
        if (!visualAtivado)
        {
            visualAtivado = true;

            if (spriteRenderer != null && spriteAtivo != null)
                spriteRenderer.sprite = spriteAtivo;
        }
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