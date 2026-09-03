using UnityEngine;
using System.Collections;

public class BackSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject ratPrefab;

    [Header("Quantidade")]
    public int quantityToSpawn = 1;
    public float delayBetweenSpawns = 1f; // se quantityToSpawn > 1

    [Header("Timing")]
    public float delayBeforeSpawn = 2f; // tempo de espera antes de spawnar, pra dar tempo do rato "andar" até o player

    private Transform player;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            player = other.transform;
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(delayBeforeSpawn);

        for (int i = 0; i < quantityToSpawn; i++)
        {
            GameObject rat = Instantiate(ratPrefab, transform.position, Quaternion.identity);

            EnemyController controller = rat.GetComponent<EnemyController>();
            if (controller != null && player != null)
                controller.DetectPlayer(player);

            if (i < quantityToSpawn - 1)
                yield return new WaitForSeconds(delayBetweenSpawns);
        }
    }
}