using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDetect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDetect(other);
    }

    private void TryDetect(Collider2D other)
    {
        if (enemy == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (!enemy.HasLineOfSight(other.transform))
            return;

        enemy.DetectPlayer(other.transform);
    }
}