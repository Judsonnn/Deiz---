using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("Barra")]
    [SerializeField] private Image preenchimento;

    [Header("Cores")]
    [SerializeField] private Color corInicial =
        new Color(0f, 1f, 0.85f);

    [SerializeField] private Color corMedia =
        new Color(1f, 0.65f, 0f);

    [SerializeField] private Color corCritica =
        new Color(1f, 0.05f, 0.15f);

    public void Atualizar(float porcentagem)
    {
        if (preenchimento == null) return;

        porcentagem = Mathf.Clamp01(porcentagem);

        preenchimento.fillAmount = porcentagem;

        if (porcentagem > 0.6f)
        {
            float t = (1f - porcentagem) / 0.4f;

            preenchimento.color =
                Color.Lerp(corInicial, corMedia, t);
        }
        else
        {
            float t = (0.6f - porcentagem) / 0.6f;

            preenchimento.color =
                Color.Lerp(corMedia, corCritica, t);
        }
    }

    public void Mostrar()
    {
        gameObject.SetActive(true);
        Atualizar(1f);
    }

    public void Ocultar()
    {
        gameObject.SetActive(false);
    }
}