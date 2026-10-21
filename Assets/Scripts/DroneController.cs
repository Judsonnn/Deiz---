using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Detecção")]
    public float margemTopo = 0.25f;

    private Estado estado = Estado.Parado;
    private Rigidbody2D rb;
    private Rigidbody2D playerRb;      // preenchido só enquanto a Deiz está em cima
    private Vector2 posInicial;
    private Vector2 direcaoFinal;
    private float distanciaPercorrida;
    private int indice;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        posInicial = rb.position;
    }

    private void FixedUpdate()
    {
        switch (estado)
        {
            case Estado.Indo:        Ir();        break;
            case Estado.Continuando: Continuar(); break;
            case Estado.Voltando:    Voltar();    break;
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

    // move o drone e leva a Deiz junto, sem parenting
    private void Mover(Vector2 novaPos)
    {
        Vector2 delta = novaPos - rb.position;
        rb.MovePosition(novaPos);

        if (playerRb != null)
            playerRb.position += delta;
    }

    private void OnCollisionEnter2D(Collision2D col) => Verificar(col);
    private void OnCollisionStay2D(Collision2D col) => Verificar(col);

    private void Verificar(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        foreach (ContactPoint2D contato in col.contacts)
        {
            if (contato.point.y >= transform.position.y + margemTopo)
            {
                playerRb = col.rigidbody;

                // começa a andar só quando a Deiz sobe
                if (estado == Estado.Parado && waypoints != null && waypoints.Length > 0)
                {
                    indice = 0;
                    estado = Estado.Indo;
                }
                return;
            }
        }

        playerRb = null;   // encostou de lado, não está em cima
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        // pulou ou saiu: só para de levar ela. O drone segue o percurso normalmente
        if (col.gameObject.CompareTag("Player"))
            playerRb = null;
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