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
    [SerializeField] private TrailRenderer rastro;

    [Header("Configurações gerais")]
    [SerializeField] private float raioVisao = 10f;
    [SerializeField] private float vidaFase2 = 0.5f;
    [SerializeField] private float tempoAviso = 0.8f;
    [SerializeField] private float tempoDescanso = 2.5f;

    [Header("Tiro")]
    [SerializeField] private BossProjectile projetilPrefab;
    [SerializeField] private Vector2 deslocamentoTiro = new Vector2(0.8f, 0f);
    [SerializeField] private float velocidadeProjetil = 10f;
    [SerializeField] private int danoProjetil = 1;
    [SerializeField] private float tempoAntesDoTiro = 0.2f;
    [SerializeField] private float tempoAteODash = 2f;

    [Header("Dash")]
    [SerializeField] private float velocidadeDash = 16f;
    [SerializeField] private float tempoMaximoDash = 1.2f;

    [Header("Fase 2")]
    [SerializeField] private float tempoTransicao = 1.5f;
    [Range(0.1f, 0.9f)]
    [SerializeField] private float fracaoPrimeiroDash = 0.5f;
    [SerializeField] private float pausaEntreDashes = 0.3f;

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

        DefinirRastro(false);
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

        DefinirRastro(false);
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

            yield return AtirarEEsperar();

            float alvoX = Mathf.Clamp(
                player.position.x,
                limiteEsquerdo.position.x,
                limiteDireito.position.x
            );

            yield return Dash(alvoX, true);

            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    private IEnumerator Fase2()
    {
        while (true)
        {
            yield return Piscar(tempoTransicao);

            yield return AtirarEEsperar();

            yield return DashDuplo();

            yield return new WaitForSeconds(tempoDescanso);
        }
    }

    private IEnumerator AtirarEEsperar()
    {
        if (player == null)
            yield break;

        float direcao = player.position.x >= transform.position.x
            ? 1f
            : -1f;

        sprite.flipX = direcao < 0f;

        yield return new WaitForSeconds(tempoAntesDoTiro);

        if (projetilPrefab == null)
        {
            Debug.LogWarning("BossController: Projetil Prefab não foi configurado!");
            yield break;
        }

        Vector2 origem = (Vector2)transform.position + new Vector2(
            direcao * deslocamentoTiro.x,
            deslocamentoTiro.y
        );

        BossProjectile projetil = Instantiate(
            projetilPrefab,
            origem,
            Quaternion.identity
        );

        projetil.Init(direcao, velocidadeProjetil, danoProjetil);

        yield return new WaitForSeconds(tempoAteODash);
    }

    private IEnumerator Dash(float alvoX, bool encerrar)
    {
        float diferenca = alvoX - rb.position.x;

        if (Mathf.Abs(diferenca) < 0.25f)
            yield break;

        float direcao = diferenca > 0f ? 1f : -1f;

        sprite.flipX = direcao < 0f;

        invulneravel = true;
        perigoso = true;

        DefinirRastro(true);

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

        DefinirRastro(false);

        if (encerrar)
        {
            perigoso = false;
            invulneravel = false;
        }
    }

    private IEnumerator DashDuplo()
    {
        float inicioX = rb.position.x;

        float alvoX = Mathf.Clamp(
            player.position.x,
            limiteEsquerdo.position.x,
            limiteDireito.position.x
        );

        float primeiroAlvoX = Mathf.Lerp(
            inicioX,
            alvoX,
            fracaoPrimeiroDash
        );

        yield return Dash(primeiroAlvoX, false);

        yield return new WaitForSeconds(pausaEntreDashes);

        yield return Dash(alvoX, true);

        perigoso = false;
        invulneravel = false;
    }

    private void FixedUpdate()
    {
        if (!perigoso || !invulneravel || Time.time < proximoDano)
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

    private void DefinirRastro(bool ativo)
    {
        if (rastro == null)
            return;

        rastro.emitting = ativo;

        if (!ativo)
            rastro.Clear();
    }

    private bool NoChao()
    {
        return Physics2D.OverlapCircle(
            pe.position,
            0.15f,
            camadaChao
        );
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