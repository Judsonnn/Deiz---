using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    public int vidaRecuperada = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HeartSystem heartSystem = other.GetComponent<HeartSystem>();

            if (heartSystem != null)
            {
                // Só coleta se o jogador não estiver com a vida cheia
                if (heartSystem.vida < heartSystem.vidaMaxima)
                {
                    heartSystem.vida += vidaRecuperada;

                    // Remove o coletável
                    Destroy(gameObject);
                }
            }
        }
    }
}