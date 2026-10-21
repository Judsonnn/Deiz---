using UnityEngine;

public class VirarSprite : MonoBehaviour
{
    public enum Modo { DirecaoDoMovimento, OlharParaOPlayer }

    [SerializeField] private Modo modo = Modo.DirecaoDoMovimento;
    [SerializeField] private bool spriteOlhaParaDireita = true;   // para onde o sprite original está virado
    [SerializeField] private Transform player;                    // só usado no modo OlharParaOPlayer
    [SerializeField] private float minimoParaVirar = 0.01f;       // evita tremer quando está quase parado

    private SpriteRenderer sprite;
    private float ultimoX;

    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        ultimoX = transform.position.x;
    }

    private void Start()
    {
        if (modo == Modo.OlharParaOPlayer && player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void LateUpdate()
    {
        float direcao = 0f;

        if (modo == Modo.DirecaoDoMovimento)
        {
            direcao = transform.position.x - ultimoX;
            ultimoX = transform.position.x;
        }
        else if (player != null)
        {
            direcao = player.position.x - transform.position.x;
        }

        if (Mathf.Abs(direcao) < minimoParaVirar) return;

        bool indoParaDireita = direcao > 0f;
        sprite.flipX = spriteOlhaParaDireita ? !indoParaDireita : indoParaDireita;
    }
}