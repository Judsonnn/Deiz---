using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private EnemyController enemy;

    private void Start()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entrou algo");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detectado");
            enemy.DetectPlayer(other.transform);
        }
    }
}