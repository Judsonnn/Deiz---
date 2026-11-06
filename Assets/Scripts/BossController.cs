
using System.Collections;
using UnityEngine;

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

    [Header("Configurações gerais")]
    [SerializeField] private float raioVisao = 10f;
    [SerializeField] private float vidaFase2 = 0.5f;
    [SerializeField] private float tempoAviso = 0.8f;
    [SerializeField] private float tempoDescanso = 2.5f;

    [Header("Fase 1 - Pulo")]
    [SerializeField] private float forcaPulo = 8f;
    [SerializeField] private float velocidadePuloHorizontal = 3f;
    [SerializeField] private float tempoMaximoPulo = 2.5f;

    [Header("Fase 1 - Dash")]
    [SerializeField] private float velocidadeDash = 16f;
    [SerializeField] private float distanciaAlemDoPlayer = 4f;
    [SerializeField] private float tempoMaximoDash = 1.2f;

    [Header("Fase 2 - Ataque aéreo")]
    [SerializeField] private float tempoTransicao = 1.5f;
    [SerializeField] private float alturaTeleporte = 4f;
    [SerializeField] private float deslocamentoX = 0f;
    [SerializeField] private float velocidadeSubida = 18f;
    [SerializeField] private float velocidadeSubidaHorizontal = 10f;
    [SerializeField] private float tempoNoAr = 0.8f;
    [SerializeField] private float velocidadeQueda = 20f;

    [Header("Dano ao jogador")]
    [SerializeField] private int danoContato = 1;
    [SerializeField] private float intervaloDanoContato = 1.5f;

    private Rigidbody2D rb;
    private Collider2D meuCollider;

    private float gravidadeOriginal;
    private float alphaOriginal;
    private float proximoDano;

    private bool acordou;
    private bool invulneravel;
    private bool perigoso;
    private bool faseDois;

    public bool Invulneravel => invulneravel;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        meuCollider = GetComponentInChildren<Collider2D>();

        if (rb != null)
            gravidadeOriginal = rb.gravityScale;

        if (sprite != null)
            alphaOriginal = sprite.color.a;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("BossController: Player não foi configurado!");
            return;
        }

        if (rb == null)
        {
            Debug.LogError("BossController: Rigidbody2D não encontrado!");
            return;
        }

        if (sprite == null)
        {
            Debug.LogError("BossController: SpriteRenderer não foi configurado!");
            return;
        }

        if (pe == null)
        {
            Debug.LogError("BossController: Transform do pé não foi configurado!");
            return;
        }

        if (barraVida != null)
            barraVida.Mostrar();

        IgnorarColisaoComPlayer();

        StartCoroutine(Fase1());
    }

    private void Update()
    {
        if (player == null)
            gameObject.SetActive(false);
    }

    public void OnVidaMudou(float porcentagem)
    {
        acordou = true;

        if (barraVida != null)
            barraVida.Atualizar(porcentagem);

        if (!faseDois && porcentagem <= vidaFase2)
        {
            faseDois = true;
            StopAllCoroutines();
            Resetar();
            StartCoroutine(Fase2());
        }
    }

    public void Parar()
    {
        StopAllCoroutines();
        Resetar();

        if (barraVida != null)
            barraVida.Ocultar();
    }

    private void Resetar()
    {
        perigoso = false;
        invulneravel = false;

        if (rb != null)
        {
            rb.gravityScale = gravidadeOriginal;
            rb.linearVelocity = Vector2.zero;
        }

        DefinirAlpha(alphaOriginal);
    }

    private void IgnorarColisaoComPlayer()
    {
        if (player == null)
            return;

        Collider2D[] collidersBoss =
            GetComponentsInChildren<Collider2D>();

        Collider2D[] collidersPlayer =
            player.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D bossCollider in collidersBoss)
        {
            foreach (Collider2D playerCollider in collidersPlayer)
            {
                Physics2D.IgnoreCollision(
                    bossCollider,
                    playerCollider,
                    true
                );
            }
        }
    }

    // =====================================================
    // FASE 1
    // UM PULO + DASH
    // =====================================================

    private IEnumerator Fase1()
    {
        while (!acordou &&
               Vector2.Distance(player.position, transform.position) > raioVisao)
        {
            yield return null;
        }

        while (true)
        {
            yield return Piscar(tempoAviso);

            // Primeiro movimento: pula na direção do jogador.
            yield return PuloAproximacao();

            if (!NoChao())
            {
                yield return new WaitForSeconds(tempoDescanso);
                continue;
            }

            // Marca a posição do jogador depois do pouso.
            float alvoX = Mathf.Clamp(
                player.position.x,
                limiteEsquerdo.position.x,
                limiteDireito.position.x
            );

            // Segundo movimento: dash.
            yield return Dash(alvoX);

            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    // =====================================================
    // PULO DE APROXIMAÇÃO
    // =====================================================

    private IEnumerator PuloAproximacao()
    {
        float direcao = player.position.x >= transform.position.x
            ? 1f
            : -1f;

        sprite.flipX = direcao < 0f;

        rb.linearVelocity = new Vector2(
            direcao * velocidadePuloHorizontal,
            forcaPulo
        );

        bool saiuDoChao = false;
        float limite = Time.time + tempoMaximoPulo;

        while (Time.time < limite)
        {
            bool noChao = NoChao();

            if (!noChao)
                saiuDoChao = true;

            if (saiuDoChao && noChao)
                break;

            yield return new WaitForFixedUpdate();
        }

        PararHorizontal();

        if (!NoChao())
        {
            Debug.LogWarning(
                "Boss: o tempo máximo do pulo acabou antes do pouso. Verifique o chão e a LayerMask."
            );
        }
    }

    // =====================================================
    // DASH
    // =====================================================

    private IEnumerator Dash(float alvoX)
    {
        float diferenca = alvoX - rb.position.x;

        if (Mathf.Abs(diferenca) < 0.25f)
            yield break;

        float direcao = diferenca > 0f ? 1f : -1f;

        sprite.flipX = direcao < 0f;

        invulneravel = true;
        perigoso = true;

        float limite = Time.time + tempoMaximoDash;

        while (Time.time < limite && !Chegou(direcao, alvoX))
        {
            float distancia = Mathf.Abs(alvoX - rb.position.x);

            float velocidadeAtual = Mathf.Min(
                velocidadeDash,
                distancia / Time.fixedDeltaTime
            );

            rb.linearVelocity = new Vector2(
                direcao * velocidadeAtual,
                rb.linearVelocity.y
            );

            yield return new WaitForFixedUpdate();
        }

        PararHorizontal();

        invulneravel = false;
        perigoso = false;
    }

    // =====================================================
    // FASE 2
    // DOIS PULOS + ATAQUE AÉREO
    // =====================================================

    private IEnumerator Fase2()
    {
        while (true)
        {
            yield return Piscar(tempoTransicao);

            // Primeiro pulo.
            yield return PuloAproximacao();

            // Segundo pulo.
            yield return PuloAproximacao();

            // Ataque aéreo depois dos dois pousos.
            yield return AtaqueAereo();

            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    // =====================================================
    // ATAQUE AÉREO
    // SUBIDA VISÍVEL + PAUSA + QUEDA
    // =====================================================

    private IEnumerator AtaqueAereo()
    {
        if (player == null)
            yield break;

        float alvoX = Mathf.Clamp(
            player.position.x + deslocamentoX,
            limiteEsquerdo.position.x,
            limiteDireito.position.x
        );

        float alvoY = rb.position.y + alturaTeleporte;

        sprite.flipX = alvoX < transform.position.x;

        invulneravel = true;
        perigoso = true;

        // Desliga a gravidade para controlar a subida.
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // Sobe rapidamente em direção ao alvo.
        float limiteSubida = Time.time + 3f;

        while (Time.time < limiteSubida)
        {
            float diferencaX = alvoX - rb.position.x;
            float diferencaY = alvoY - rb.position.y;

            if (Mathf.Abs(diferencaX) < 0.1f &&
                Mathf.Abs(diferencaY) < 0.1f)
            {
                break;
            }

            float velocidadeX = Mathf.Clamp(
                diferencaX / Time.fixedDeltaTime,
                -velocidadeSubidaHorizontal,
                velocidadeSubidaHorizontal
            );

            float velocidadeY = Mathf.Clamp(
                diferencaY / Time.fixedDeltaTime,
                0f,
                velocidadeSubida
            );

            rb.linearVelocity = new Vector2(
                velocidadeX,
                velocidadeY
            );

            yield return new WaitForFixedUpdate();
        }

        // Para o movimento quando chega ao alto.
        rb.linearVelocity = Vector2.zero;

        // Pausa antes da descida.
        yield return new WaitForSeconds(tempoNoAr);

        // Descida rápida.
        rb.linearVelocity = new Vector2(
            0f,
            -velocidadeQueda
        );

        float limiteQueda = Time.time + 3f;

        while (!NoChao() && Time.time < limiteQueda)
        {
            yield return new WaitForFixedUpdate();
        }

        PararHorizontal();

        rb.gravityScale = gravidadeOriginal;

        invulneravel = false;
        perigoso = false;
    }

    // =====================================================
    // DANO AO JOGADOR
    // =====================================================

    private void FixedUpdate()
    {
        if (!perigoso || Time.time < proximoDano)
            return;

        if (meuCollider == null)
            return;

        Collider2D[] atingidos = Physics2D.OverlapBoxAll(
            meuCollider.bounds.center,
            meuCollider.bounds.size,
            0f
        );

        foreach (Collider2D col in atingidos)
        {
            HeartSystem coracoes =
                col.GetComponentInParent<HeartSystem>();

            if (coracoes != null)
            {
                coracoes.ReceberDano(danoContato);
                proximoDano = Time.time + intervaloDanoContato;
                break;
            }
        }
    }

    // =====================================================
    // FUNÇÕES AUXILIARES
    // =====================================================

    private bool Chegou(float direcao, float alvoX)
    {
        if (direcao > 0f)
            return rb.position.x >= alvoX;

        return rb.position.x <= alvoX;
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
        float tempo = 0f;

        while (tempo < duracao)
        {
            DefinirAlpha(0.2f);
            yield return new WaitForSeconds(0.08f);

            DefinirAlpha(alphaOriginal);
            yield return new WaitForSeconds(0.08f);

            tempo += 0.16f;
        }

        DefinirAlpha(alphaOriginal);
    }

    private void DefinirAlpha(float alpha)
    {
        if (sprite == null)
            return;

        Color cor = sprite.color;
        cor.a = alpha;
        sprite.color = cor;
    }

    private bool NoChao()
    {
        return Physics2D.OverlapCircle(
            pe.position,
            0.15f,
            camadaChao
        );
    }

    private void Virar(float direcao)
    {
        sprite.flipX = direcao < 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (pe != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pe.position, 0.15f);
        }
    }
}
