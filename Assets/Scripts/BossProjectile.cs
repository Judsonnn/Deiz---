using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float tempoDeVida = 4f;
    [SerializeField] private LayerMask camadaChao;

    private Vector2 direcao;
    private float velocidade;
    private int dano;

    public void Init(Vector2 novaDirecao, float novaVelocidade, int novoDano)
    {
        direcao = novaDirecao.normalized;
        velocidade = novaVelocidade;
        dano = novoDano;

        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);

        Destroy(gameObject, tempoDeVida);
    }

    private void Update()
    {
        transform.position += (Vector3)(direcao * velocidade * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HeartSystem coracoes = other.GetComponentInParent<HeartSystem>();

        if (coracoes != null)
        {
            coracoes.ReceberDano(dano);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & camadaChao) != 0)
            Destroy(gameObject);
    }
}