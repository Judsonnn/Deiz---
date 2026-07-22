using UnityEngine;

public class ForcedFallDamageZone : MonoBehaviour
{
    public int damageAmount = 1;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        HeartSystem heart = other.GetComponent<HeartSystem>();
        if (heart != null)
        {
            heart.vida -= damageAmount;
            triggered = true;
        }
    }
}