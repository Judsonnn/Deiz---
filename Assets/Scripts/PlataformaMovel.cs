using UnityEngine;

public class PlataformaMovel : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 3f;
    [SerializeField] private int direcao = 1;                 // 1 = direita, -1 = esquerda

    [Header("Tempos")]
    [SerializeField] private float tempoNormal = 4f;          // anda na velocidade normal
    [SerializeField] private float tempoAceleracao = 1.2f;    // acelera antes de ir embora
    [SerializeField] private float tempoSumida = 1.5f;        // fica invisível antes de voltar

    [Header("Aceleração")]
    [SerializeField] private float multiplicadorFinal = 4f;   // quantas vezes mais rápido no fim

    private enum Estado { Normal, Acelerando, Sumida }
    private Estado estado = Estado.Normal;

    private SpriteRenderer sprite;
    private Collider2D col;
    private Vector3 posInicial;
    private float timer;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        posInicial = transform.position;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        switch (estado)
        {
            case Estado.Normal:
                Mover(velocidade);
                if (timer >= tempoNormal) IniciarAceleracao();
                break;

            case Estado.Acelerando:
                float t = Mathf.Clamp01(timer / tempoAceleracao);
                float vel = Mathf.Lerp(velocidade, velocidade * multiplicadorFinal, t * t); // acelera aos poucos
                Mover(vel);
                if (timer >= tempoAceleracao) Sumir();
                break;

            case Estado.Sumida:
                if (timer >= tempoSumida) Reaparecer();
                break;
        }
    }

    private void Mover(float vel)
    {
        transform.Translate(Vector3.right * direcao * vel * Time.deltaTime, Space.World);
    }

    private void IniciarAceleracao()
    {
        estado = Estado.Acelerando;
        timer = 0f;
        SoltarPlayer();   // a Deiz fica pra trás e cai
    }

    private void Sumir()
    {
        estado = Estado.Sumida;
        timer = 0f;
        sprite.enabled = false;
        col.enabled = false;
    }

    private void Reaparecer()
    {
        transform.position = posInicial;
        estado = Estado.Normal;
        timer = 0f;
        sprite.enabled = true;
        col.enabled = true;
    }

    private void SoltarPlayer()
    {
        foreach (Transform filho in transform)
        {
            if (filho.CompareTag("Player"))
            {
                filho.SetParent(null);
                break;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        // só gruda no estado normal, e só se pousar por cima
        if (estado == Estado.Normal && c.gameObject.CompareTag("Player") && c.contacts[0].normal.y < -0.5f)
            c.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player") && c.transform.parent == transform)
            c.transform.SetParent(null);
    }
}