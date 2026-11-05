
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Transform limiteEsquerdo;
    [SerializeField] private Transform limiteDireito;
    [SerializeField] private Transform pe;
    [SerializeField] private LayerMask camadaChao;
    [SerializeField] private BossHealthBar barraVida;

    [Header("Geral")]
    [SerializeField] private float raioVisao = 10f;
    [SerializeField, Range(0.1f, 0.9f)]
    private float vidaFase2 = 0.5f;
    [SerializeField] private float tempoAviso = 1.2f;
    [SerializeField] private float tempoDescanso = 3f;

    [Header("Fase 1 - Dash")]
    [SerializeField] private float velocidadeDash = 12f;
    [SerializeField] private float tempoMaximoDash = 1.5f;

    [Header("Fase 2 - Teletransporte")]
    [SerializeField] private float tempoTransicao = 1.5f;
    [SerializeField] private float alturaTeleporte = 4f;
    [SerializeField] private float deslocamentoX = 0f;
    [SerializeField] private float tempoNoAr = 0.8f;
    [SerializeField] private float velocidadeQueda = 20f;

    [Header("Dano por contato")]
    [SerializeField] private int danoContato = 1;
    [SerializeField] private float intervaloDanoContato = 1.5f;

    public bool Invulneravel => invulneravel;

    private Rigidbody2D rb;
    private Collider2D meuCollider;
    private Coroutine rotina;

    private float gravidadeOriginal;
    private float alphaOriginal = 1f;
    private float proximoDano;

    private bool fase2;
    private bool acordou;
    private bool perigoso;
    private bool invulneravel;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        meuCollider = GetComponentInChildren<Collider2D>();

        gravidadeOriginal = rb.gravityScale;

        if (sprite != null)
            alphaOriginal = sprite.color.a;
    }

    private void Start()
    {
        if (player == null || sprite == null || pe == null ||
            limiteEsquerdo == null || limiteDireito == null)
        {
            Debug.LogError(
                "Boss: falta preencher algum campo de Referências!",
                this
            );

            enabled = false;
            return;
        }

        if (barraVida != null)
            barraVida.Mostrar();

        IgnorarColisaoComPlayer();

        rotina = StartCoroutine(Fase1());
    }

    private void Update()
    {
        if (player == null)
        {
            Parar();
            enabled = false;
        }
    }

    // ---------- COLISÃO COM PLAYER ----------

    private void IgnorarColisaoComPlayer()
    {
        foreach (Collider2D meu in GetComponentsInChildren<Collider2D>())
        {
            foreach (Collider2D dele in
                     player.GetComponentsInChildren<Collider2D>())
            {
                Physics2D.IgnoreCollision(meu, dele, true);
            }
        }
    }

    // ---------- VIDA E FASES ----------

    public void OnVidaMudou(float porcentagem)
    {
        acordou = true;

        if (barraVida != null)
            barraVida.Atualizar(porcentagem);

        if (fase2)
            return;

        if (porcentagem <= vidaFase2)
            IniciarFase2();
    }

    public void Parar()
    {
        StopAllCoroutines();

        rotina = null;

        Resetar();

        if (barraVida != null)
            barraVida.Ocultar();
    }

    private void IniciarFase2()
    {
        fase2 = true;

        if (rotina != null)
            StopCoroutine(rotina);

        Resetar();

        rotina = StartCoroutine(Fase2());
    }

    private void Resetar()
    {
        perigoso = false;
        invulneravel = false;

        rb.gravityScale = gravidadeOriginal;
        rb.linearVelocity = Vector2.zero;

        DefinirAlpha(alphaOriginal);
    }

    // ---------- FASE 1 ----------

    private IEnumerator Fase1()
    {
        // Espera o jogador chegar perto ou atacar.
        while (!acordou &&
               Vector2.Distance(player.position, transform.position) > raioVisao)
        {
            yield return null;
        }

        while (true)
        {
            // GUARDA A POSIÇÃO DO PLAYER ANTES DO AVISO.
            float alvoX = Mathf.Clamp(
                player.position.x,
                limiteEsquerdo.position.x,
                limiteDireito.position.x
            );

            // O jogador tem tempo para fugir.
            yield return Piscar(tempoAviso);

            // Ataca somente a posição que foi marcada.
            yield return Dash(alvoX);

            // Descanso: boss vulnerável.
            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    private IEnumerator Dash(float alvoX)
    {
        float diferenca = alvoX - rb.position.x;

        if (Mathf.Abs(diferenca) < 0.25f)
            yield break;

        float dir = diferenca > 0f ? 1f : -1f;

        sprite.flipX = dir < 0f;

        invulneravel = true;
        perigoso = true;

        float limite = Time.time + tempoMaximoDash;

        while (Time.time < limite && !Chegou(dir, alvoX))
        {
            float distancia = Mathf.Abs(alvoX - rb.position.x);

            float velocidadeAtual = Mathf.Min(
                velocidadeDash,
                distancia / Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(
                dir * velocidadeAtual,
                rb.linearVelocity.y
            );

            yield return new WaitForFixedUpdate();
        }

        PararHorizontal();

        invulneravel = false;
        perigoso = false;
    }

    // ---------- FASE 2 ----------

    private IEnumerator Fase2()
    {
        // Pequena pausa para anunciar a mudança de fase.
        yield return new WaitForSeconds(tempoTransicao);

        while (true)
        {
            yield return Piscar(tempoAviso);

            yield return AtaqueAereo();

            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    private IEnumerator AtaqueAereo()
    {
        float yChao = transform.position.y;

        float x = Mathf.Clamp(
            player.position.x + deslocamentoX,
            limiteEsquerdo.position.x,
            limiteDireito.position.x
        );

        invulneravel = true;
        perigoso = true;

        // Desliga a gravidade para teletransportar.
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        Vector2 destino = new Vector2(
            x,
            yChao + alturaTeleporte
        );

        rb.position = destino;
        transform.position = destino;

        Virar();

        // Aguarda no ar antes de cair.
        yield return new WaitForSeconds(tempoNoAr);

        // Queda rápida.
        float limite = Time.time + 3f;

        while (!NoChao() && Time.time < limite)
        {
            rb.linearVelocity = new Vector2(
                0f,
                -velocidadeQueda
            );

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = gravidadeOriginal;

        invulneravel = false;
        perigoso = false;
    }

    // ---------- DANO POR CONTATO ----------

    private void FixedUpdate()
    {
        if (!perigoso ||
            Time.time < proximoDano ||
            meuCollider == null)
            return;

        Bounds b = meuCollider.bounds;

        foreach (Collider2D h in Physics2D.OverlapBoxAll(
            b.center,
            b.size,
            0f))
        {
            HeartSystem coracoes =
                h.GetComponentInParent<HeartSystem>();

            if (coracoes == null)
                continue;

            coracoes.ReceberDano(danoContato);

            proximoDano =
                Time.time + intervaloDanoContato;

            break;
        }
    }

    // ---------- AUXILIARES ----------

    private bool Chegou(float dir, float alvoX)
    {
        float x = rb.position.x;

        return dir > 0f
            ? x >= alvoX
            : x <= alvoX;
    }

    private void PararHorizontal()
    {
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );
    }

    private IEnumerator Piscar(float duracao)
    {
        float fim = Time.time + duracao;

        while (Time.time < fim)
        {
            Virar();

            DefinirAlpha(0.2f);

            yield return new WaitForSeconds(0.08f);

            DefinirAlpha(alphaOriginal);

            yield return new WaitForSeconds(0.08f);
        }

        DefinirAlpha(alphaOriginal);
    }

    private void DefinirAlpha(float a)
    {
        if (sprite == null)
            return;

        Color c = sprite.color;
        c.a = a;

        sprite.color = c;
    }

    private bool NoChao()
    {
        return Physics2D.OverlapCircle(
            pe.position,
            0.15f,
            camadaChao
        );
    }

    private void Virar()
    {
        sprite.flipX =
            player.position.x < transform.position.x;
    }
}
