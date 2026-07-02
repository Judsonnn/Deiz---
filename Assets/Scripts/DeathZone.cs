using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Tipo de dano")]
    public bool instantKill = true; // true = mata tudo, false = perde 1 coração

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        ApplyDamage(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        ApplyDamage(collision.gameObject);
    }

    private void ApplyDamage(GameObject player)
    {
        HeartSystem heart = player.GetComponent<HeartSystem>();
        if (heart == null) return;

        if (instantKill)
            heart.vida = 0;
        else
            heart.vida -= 1;
    }
}