// ─────────────────────────────────────────
// CollectableUI.cs — coloca no objeto
// de UI que mostra o coletável
// ─────────────────────────────────────────
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CollectableUI : MonoBehaviour
{
    [Header("Configuração")]
    public string collectableID = "gem"; // deve bater com o ID do Collectable

    [Header("Referências UI")]
    public TextMeshProUGUI countText;
    public Image icon;
    public RectTransform iconRect; // o RectTransform do ícone para o efeito

    [Header("Efeito de pulo")]
    public float jumpHeight = 20f;
    public float jumpDuration = 0.3f;

    private Vector2 iconOriginalPos;

    void Start()
    {
        iconOriginalPos = iconRect.anchoredPosition;
        UpdateText(0);

        CollectableManager.Instance.OnCollect += OnCollect;
    }

    void OnDestroy()
    {
        if (CollectableManager.Instance != null)
            CollectableManager.Instance.OnCollect -= OnCollect;
    }

    private void OnCollect(string id, int total)
    {
        if (id != collectableID) return;

        UpdateText(total);
        StopAllCoroutines();
        StartCoroutine(JumpEffect());
    }

    private void UpdateText(int value)
    {
        if (countText != null)
            countText.text = "x" + value;
    }

    private IEnumerator JumpEffect()
    {
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            // Curva de pulo suave — sobe e volta
            float jump = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            iconRect.anchoredPosition = iconOriginalPos + new Vector2(0f, jump);

            yield return null;
        }

        iconRect.anchoredPosition = iconOriginalPos;
    }
}