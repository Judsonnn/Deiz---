
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(Light2D))]
public class LuzPiscandoAleatoria : MonoBehaviour
{
    [Header("Intervalo entre piscadas")]
    public float intervaloMinimo = 3f;
    public float intervaloMaximo = 9f;

    [Header("Intensidade durante a falha")]
    [Range(0f, 1f)]
    public float intensidadeMinima = 0.45f;

    [Range(0f, 1f)]
    public float intensidadeMaxima = 0.85f;

    [Header("Duração das piscadas")]
    public float duracaoMinima = 0.04f;
    public float duracaoMaxima = 0.12f;

    [Header("Quantidade de piscadas")]
    public int pulsosMinimos = 2;
    public int pulsosMaximos = 4;

    private Light2D luz;
    private float intensidadeOriginal;

    private void Start()
    {
        luz = GetComponent<Light2D>();
        intensidadeOriginal = luz.intensity;

        StartCoroutine(PiscarAleatoriamente());
    }

    private IEnumerator PiscarAleatoriamente()
    {
        while (true)
        {
            // Espera um intervalo aleatório
            yield return new WaitForSeconds(
                Random.Range(intervaloMinimo, intervaloMaximo)
            );

            // Escolhe quantas vezes vai piscar
            int quantidade = Random.Range(
                pulsosMinimos,
                pulsosMaximos + 1
            );

            for (int i = 0; i < quantidade; i++)
            {
                float intensidade = Random.Range(
                    intensidadeMinima,
                    intensidadeMaxima
                );

                float duracao = Random.Range(
                    duracaoMinima,
                    duracaoMaxima
                );

                // Diminui a luz
                yield return AlterarIntensidade(
                    intensidadeOriginal * intensidade,
                    duracao
                );

                yield return new WaitForSeconds(
                    Random.Range(0.03f, 0.10f)
                );

                // Recupera a iluminação
                yield return AlterarIntensidade(
                    intensidadeOriginal,
                    duracao
                );
            }
        }
    }

    private IEnumerator AlterarIntensidade(
        float destino,
        float duracao
    )
    {
        float inicial = luz.intensity;
        float tempo = 0f;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;

            luz.intensity = Mathf.Lerp(
                inicial,
                destino,
                tempo / duracao
            );

            yield return null;
        }

        luz.intensity = destino;
    }
}
