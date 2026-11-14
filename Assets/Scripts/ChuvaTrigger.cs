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
    public string tagPlayer = "Player";
    public float velocidadeFade = 2f;

    private bool ativado = false;
    private Collider2D colPlayer;
    private float fatorChuva = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(tagPlayer)) return;
        if (ativado && ativarSoUmaVez) return;

        ativado = true;

        if (fonteTrovao != null)
        {
            fonteTrovao.loop = false;
            fonteTrovao.volume = volumeTrovao;
            fonteTrovao.Play();
        }

        if (fonteChuva != null && !fonteChuva.isPlaying)
        {
            fonteChuva.loop = true;
            fonteChuva.volume = volumeChuva * fatorChuva;
            fonteChuva.Play();
        }
    }

    private void Update()
    {
        if (!ativado) return;

        bool dentro = PlayerEstaDentro();

        if (particulasChuva != null)
        {
            if (dentro && particulasChuva.isEmitting)
                particulasChuva.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            else if (!dentro && !particulasChuva.isEmitting)
                particulasChuva.Play();
        }

        if (fonteChuva != null)
        {
            float alvo = dentro ? 0f : 1f;
            fatorChuva = Mathf.MoveTowards(fatorChuva, alvo, velocidadeFade * Time.unscaledDeltaTime);
            fonteChuva.volume = volumeChuva * fatorChuva;
        }
    }

    private bool PlayerEstaDentro()
    {
        if (colPlayer == null || !colPlayer.gameObject.activeInHierarchy)
        {
            GameObject p = GameObject.FindGameObjectWithTag(tagPlayer);
            colPlayer = p != null ? p.GetComponent<Collider2D>() : null;
        }

        if (colPlayer == null) return false;

        Vector2 centro = colPlayer.bounds.center;

        for (int i = 0; i < ZonaSemChuva.Zonas.Count; i++)
        {
            Collider2D zona = ZonaSemChuva.Zonas[i];
            if (zona != null && zona.OverlapPoint(centro))
                return true;
        }

        return false;
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }
}