using UnityEngine;

public class PlataformaPontos : MonoBehaviour
{
    [Header("Percurso (arraste os pontos aqui, na ordem)")]
    [SerializeField] private Transform[] pontos;

    [Header("Movimento")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private float pausaNasPontas = 0.3f;

    [Header("Linha no jogo")]
    [SerializeField] private bool mostrarLinhaNoJogo = true;
    [SerializeField] private Color corDaLinha = new Color(0f, 1f, 1f, 0.6f);
    [SerializeField] private float larguraDaLinha = 0.08f;

    private int indice = 1;
    private int sentido = 1;
    private float pausaRestante;

    private void Start()
    {
        if (pontos == null || pontos.Length < 2)
        {
            Debug.LogWarning(name + ": coloque pelo menos 2 pontos.");
            enabled = false;
            return;
        }

        transform.position = Alvo(0);

        if (mostrarLinhaNoJogo) CriarLinha();
    }

    private Vector3 Alvo(int i)
    {
        Vector3 p = pontos[i].position;
        return new Vector3(p.x, p.y, transform.position.z);
    }

    private void Update()
    {
        if (pausaRestante > 0f)
        {
            pausaRestante -= Time.deltaTime;
            return;
        }

        Vector3 alvo = Alvo(indice);
        transform.position = Vector3.MoveTowards(transform.position, alvo, velocidade * Time.deltaTime);

        if (transform.position == alvo)
        {
            // chegou na ponta: inverte o sentido
            if (indice + sentido >= pontos.Length || indice + sentido < 0)
                sentido *= -1;

            indice += sentido;
            pausaRestante = pausaNasPontas;
        }
    }

    private void CriarLinha()
    {
        var obj = new GameObject("Percurso_" + name);
        var lr = obj.AddComponent<LineRenderer>();

        lr.positionCount = pontos.Length;
        for (int i = 0; i < pontos.Length; i++)
            lr.SetPosition(i, pontos[i].position);

        lr.startWidth = larguraDaLinha;
        lr.endWidth = larguraDaLinha;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = corDaLinha;
        lr.endColor = corDaLinha;
        lr.useWorldSpace = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            lr.sortingLayerID = sr.sortingLayerID;
            lr.sortingOrder = sr.sortingOrder - 1;
        }
    }

    // linha visível no editor (aba Scene), mesmo sem Play
    private void OnDrawGizmos()
    {
        if (pontos == null) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < pontos.Length; i++)
        {
            if (pontos[i] == null) continue;
            Gizmos.DrawWireSphere(pontos[i].position, 0.2f);

            if (i < pontos.Length - 1 && pontos[i + 1] != null)
                Gizmos.DrawLine(pontos[i].position, pontos[i + 1].position);
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player") && c.contacts[0].normal.y < -0.5f)
            c.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.gameObject.CompareTag("Player") && c.transform.parent == transform)
            c.transform.SetParent(null);
    }
}