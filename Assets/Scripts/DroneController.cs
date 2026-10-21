using UnityEngine;

public class DroneController : MonoBehaviour
{
    private enum Estado { Parado, Indo, Continuando, Voltando }

    [Header("Waypoints")]
    [Tooltip("Arraste os pontos aqui, na ordem do percurso")]
    public Transform[] waypoints;

    [Header("Movimento")]
    public float velocidadeIda = 3f;
    public float velocidadeVolta = 5f;

    [Header("Depois do último waypoint")]
    [Tooltip("Quanto ele continua andando, na mesma direção, antes de voltar")]
    public float distanciaExtra = 20f;

    [Header("Detecção")]
    public float margemTopo = 0.25f;

    private Estado estado = Estado.Parado;
    private Vector3 posInicial;
    private int indice;
    private Transform player;
    private Vector3 direcaoFinal;
    private float distanciaPercorrida;

    private void Start()
    {
        posInicial = transform.position;
    }

    private void Update()
    {
        switch (estado)
        {
            case Estado.Indo:        Ir();         break;
            case Estado.Continuando: Continuar();  break;
            case Estado.Voltando:    Voltar();     break;
        }
    }

    private void Ir()
    {
        Vector3 alvo = Alvo(indice);
        transform.position = Vector3.MoveTowards(transform.position, alvo, velocidadeIda * Time.deltaTime);

        if (transform.position != alvo) return;

        indice++;
        if (indice >= waypoints.Length)
        {
            // direção do último trecho
            Vector3 anterior = waypoints.Length >= 2 ? waypoints[waypoints.Length - 2].position : posInicial;
            Vector3 ultimo = waypoints[waypoints.Length - 1].position;
            direcaoFinal = new Vector3(ultimo.x - anterior.x, ultimo.y - anterior.y, 0f).normalized;

            distanciaPercorrida = 0f;
            estado = Estado.Continuando;
        }
    }

    private void Continuar()
    {
        float passo = velocidadeIda * Time.deltaTime;
        transform.position += direcaoFinal * passo;
        distanciaPercorrida += passo;

        if (distanciaPercorrida >= distanciaExtra)
            IniciarVolta();
    }

    private void Voltar()
    {
        transform.position = Vector3.MoveTowards(transform.position, posInicial, velocidadeVolta * Time.deltaTime);

        if (transform.position == posInicial)
        {
            indice = 0;
            estado = Estado.Parado;
        }
    }

    private Vector3 Alvo(int i)
    {
        Vector3 p = waypoints[i].position;
        return new Vector3(p.x, p.y, transform.position.z);
    }

    private void IniciarVolta()
    {
        SoltarPlayer();
        estado = Estado.Voltando;
    }

    private void SoltarPlayer()
    {
        if (player == null) return;
        player.SetParent(null);
        player = null;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (estado != Estado.Parado) return;
        if (!col.gameObject.CompareTag("Player")) return;
        if (waypoints == null || waypoints.Length == 0) return;

        foreach (ContactPoint2D contato in col.contacts)
        {
            if (contato.point.y >= transform.position.y + margemTopo)
            {
                player = col.transform;
                player.SetParent(transform);
                indice = 0;
                estado = Estado.Indo;
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        // se o player pular fora durante o trajeto, o drone volta
        if (col.gameObject.CompareTag("Player") && (estado == Estado.Indo || estado == Estado.Continuando))
            IniciarVolta();
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null) return;

        Gizmos.color = Color.cyan;
        Vector3 anterior = Application.isPlaying ? posInicial : transform.position;

        foreach (Transform w in waypoints)
        {
            if (w == null) continue;
            Gizmos.DrawWireSphere(w.position, 0.2f);
            Gizmos.DrawLine(anterior, w.position);
            anterior = w.position;
        }
    }
}