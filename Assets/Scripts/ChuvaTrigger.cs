using UnityEngine;

public class ChuvaTrigger : MonoBehaviour
{
    [Header("Partículas")]
    public ParticleSystem particulasChuva;

    [Header("Áudio")]
    public AudioSource fonteTrovao;   
    public AudioSource fonteChuva;   

    [Header("Volumes")]
    [Range(0f, 1f)] public float volumeTrovao = 1f;
    [Range(0f, 1f)] public float volumeChuva = 0.3f;

    [Header("Opções")]
    public bool ativarSoUmaVez = true;

    private bool ativado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (ativado && ativarSoUmaVez) return;

        ativado = true;

        if (particulasChuva != null && !particulasChuva.isPlaying)
            particulasChuva.Play();

        if (fonteTrovao != null)
        {
            fonteTrovao.loop = false;
            fonteTrovao.volume = volumeTrovao;
            fonteTrovao.Play();
        }

        if (fonteChuva != null && !fonteChuva.isPlaying)
        {
            fonteChuva.loop = true;
            fonteChuva.volume = volumeChuva;
            fonteChuva.Play();
        }
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }
}