using System;
using UnityEngine;

public class CarroPlataforma : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;

    private float velocidade;
    private int direcao;        // 1 = direita, -1 = esquerda
    private float limiteX;      // posição X onde o carro some
    private Action<CarroPlataforma> aoSair;

    public void Iniciar(int dir, float vel, float limite, Action<CarroPlataforma> callbackSaida)
    {
        direcao = dir;
        velocidade = vel;
        limiteX = limite;
        aoSair = callbackSaida;
        sprite.flipX = dir < 0;   // ajuste se o sprite original olhar para a esquerda
    }

    private void Update()
    {
        transform.Translate(Vector3.right * direcao * velocidade * Time.deltaTime, Space.World);

        if ((direcao > 0 && transform.position.x > limiteX) ||
            (direcao < 0 && transform.position.x < -limiteX))
        {
            aoSair?.Invoke(this);
        }
    }
}