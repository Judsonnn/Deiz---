using System.Collections;
using UnityEngine;

public class GatilhoPainel : MonoBehaviour
{
    [SerializeField] private GameObject painel;
    [SerializeField] private AudioSource som;
    [SerializeField] private float duracao = 4f;

    [Header("Opcional: painel com imagem (fecha apertando Espaço)")]
    [SerializeField] private GameObject painelImagem;
    [SerializeField] private float tempoFade = 1f;

    private bool usado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (usado || !other.CompareTag("Player")) return;
        usado = true;
        StartCoroutine(Mostrar());
    }

    private IEnumerator Mostrar()
    {
        if (painel != null) painel.SetActive(true);
        if (som != null) som.Play();

        if (painelImagem != null)
        {
            Time.timeScale = 0f; // congela o jogo

            CanvasGroup grupo = painelImagem.GetComponent<CanvasGroup>();
            if (grupo == null) grupo = painelImagem.AddComponent<CanvasGroup>();

            grupo.alpha = 0f;
            painelImagem.SetActive(true);

            yield return Fade(grupo, 0f, 1f);

            while (!Input.GetKeyDown(KeyCode.Space))
                yield return null;

            yield return Fade(grupo, 1f, 0f);

            painelImagem.SetActive(false);
            Time.timeScale = 1f; // descongela
        }
        else
        {
            yield return new WaitForSeconds(duracao);
        }

        if (painel != null) painel.SetActive(false);
    }

    // usa unscaledDeltaTime porque o timeScale está em 0 durante o fade
    private IEnumerator Fade(CanvasGroup grupo, float de, float para)
    {
        float t = 0f;
        grupo.alpha = de;
        while (t < tempoFade)
        {
            t += Time.unscaledDeltaTime;
            grupo.alpha = Mathf.Lerp(de, para, t / tempoFade);
            yield return null;
        }
        grupo.alpha = para;
    }
}