using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DroneController : MonoBehaviour
{
    private enum Estado { Parado, Indo, Continuando, Voltando }

    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Movimento")]
    public float velocidadeIda = 3f;
    public float velocidadeVolta = 5f;

    [Header("Depois do último waypoint")]
    public float distanciaExtra = 20f;

    private Estado estado = Estado.Parado;
    private Rigidbody2D rb;
    private Collider2D col;
    private Rigidbody2D playerRb;
    private Vector2 posInicial;
    private Vector2 direcaoFinal;
    private Vector2 velocidadeAtual;
    private float distanciaPercorrida;
    private int indice;
    private float ultimoContato = -10f;
    private float ultimoVxAplicado = float.NaN;
    private const float tolerancia = 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        posInicial = rb.position;
    }

    private void FixedUpdate()
    {
        velocidadeAtual = Vector2.zero;
        DetectarPlayer();

        switch (estado)
        {
            case Estado.Indo:        Ir();        break;
            case Estado.Continuando: Continuar(); break;
            case Estado.Voltando:    Voltar();    break;
        }
    }

    // roda depois do Update do PlayerController, então soma por cima da velocidade dele
    private void LateUpdate()
    {
        if (playerRb == null) return;

        float vp = velocidadeAtual.x;
        if (Mathf.Approximately(vp, 0f)) return;

        Vector2 v = playerRb.linearVelocity;

        // se o player não reescreveu a velocidade (ex.: levando dano), não soma de novo
        if (Mathf.Approximately(v.x, ultimoVxAplicado)) return;

        v.x += vp;
        playerRb.linearVelocity = v;
        ultimoVxAplicado = v.x;
    }

    private void DetectarPlayer()
    {
        Bounds b = col.bounds;
        Vector2 centro = new Vector2(b.center.x, b.max.y + 0.1f);
        Vector2 tamanho = new Vector2(b.size.x * 0.95f, 0.2f);

        Rigidbody2D achado = null;

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(centro, tamanho, 0f))
        {
            Rigidbody2D r = hit.attachedRigidbody;
            if (r != null && r.CompareTag("Player")) { achado = r; break; }
        }

        // subindo mais rápido que o drone = pulou, não leva junto
        bool emCima = achado != null &&
                      achado.linearVelocity.y - rb.linearVelocity.y <= 0.5f;

        if (emCima)
        {
            playerRb = achado;
            ultimoContato = Time.time;

            if (estado == Estado.Parado && waypoints != null && waypoints.Length > 0)
            {
                indice = 0;
                estado = Estado.Indo;
            }
        }
        else if (playerRb != null && Time.time - ultimoContato > tolerancia)
        {
            playerRb = null;
        }
    }

    private void Ir()
    {
        Vector2 alvo = waypoints[indice].position;
        Vector2 novaPos = Vector2.MoveTowards(rb.position, alvo, velocidadeIda * Time.fixedDeltaTime);
        Mover(novaPos);

        if (novaPos == alvo)
        {
            indice++;
            if (indice >= waypoints.Length)
            {
                Vector2 anterior = waypoints.Length >= 2 ? (Vector2)waypoints[waypoints.Length - 2].position : posInicial;
                direcaoFinal = (alvo - anterior).normalized;
                distanciaPercorrida = 0f;
                estado = Estado.Continuando;
            }
        }
    }

    private void Continuar()
    {
        float passo = velocidadeIda * Time.fixedDeltaTime;
        Mover(rb.position + direcaoFinal * passo);
        distanciaPercorrida += passo;

        if (distanciaPercorrida >= distanciaExtra)
            estado = Estado.Voltando;
    }

    private void Voltar()
    {
        Vector2 novaPos = Vector2.MoveTowards(rb.position, posInicial, velocidadeVolta * Time.fixedDeltaTime);
        Mover(novaPos);

        if (novaPos == posInicial)
        {
            indice = 0;
            estado = Estado.Parado;
        }
    }

    private void Mover(Vector2 novaPos)
    {
        velocidadeAtual = (novaPos - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(novaPos);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null) return;

        Gizmos.color = Color.cyan;
        Vector3 anterior = Application.isPlaying ? (Vector3)posInicial : transform.position;

        foreach (Transform w in waypoints)
        {
            if (w == null) continue;
            Gizmos.DrawWireSphere(w.position, 0.2f);
            Gizmos.DrawLine(anterior, w.position);
            anterior = w.position;
        }
    }
}