// ─────────────────────────────────────────
// EnemyHealthBar.cs — coloca no objeto
// filho que vai ser a barra de vida
// ─────────────────────────────────────────
using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Partes da barra")]
    public SpriteRenderer background; // fundo cinza/preto
    public SpriteRenderer fill;       // preenchimento colorido

    [Header("Cores")]
    public Color fullColor = Color.green;
    public Color lowColor = Color.red;

    // Mantém a barra sempre virada para câmera
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Faz a barra não rotacionar com o inimigo
        if (mainCamera != null)
            transform.rotation = Quaternion.identity;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateBar(int current, int max)
    {
        float ratio = (float)current / max;

        // Atualiza o tamanho do fill
        Vector3 scale = fill.transform.localScale;
        scale.x = ratio;
        fill.transform.localScale = scale;

        // Muda a cor conforme a vida
        fill.color = Color.Lerp(lowColor, fullColor, ratio);

        // Centraliza o fill corretamente ao encolher
        Vector3 pos = fill.transform.localPosition;
        pos.x = (ratio - 1f) * 0.5f;
        fill.transform.localPosition = pos;
    }
}