using UnityEngine;
using UnityEngine.Pool;

public class SpawnerCarros : MonoBehaviour
{
    [SerializeField] private CarroPlataforma carroPrefab;
    [SerializeField] private float yDaPista = 0f;
    [SerializeField] private float bordaX = 14f;          // distância do centro até fora da tela
    [SerializeField] private Vector2 intervalo = new(1.5f, 3.5f);
    [SerializeField] private Vector2 velocidade = new(3f, 6f);

    private ObjectPool<CarroPlataforma> pool;
    private float proximoSpawn;
    private bool proximoVaiParaDireita = true;

    private void Awake()
    {
        pool = new ObjectPool<CarroPlataforma>(
            createFunc: () => Instantiate(carroPrefab, transform),
            actionOnGet: c => c.gameObject.SetActive(true),
            actionOnRelease: c => c.gameObject.SetActive(false),
            defaultCapacity: 6);
    }

    private void Update()
    {
        if (Time.time < proximoSpawn) return;

        Spawnar();
        proximoSpawn = Time.time + Random.Range(intervalo.x, intervalo.y);
    }

    private void Spawnar()
    {
        int dir = proximoVaiParaDireita ? 1 : -1;
        proximoVaiParaDireita = !proximoVaiParaDireita;

        var carro = pool.Get();
        carro.transform.position = new Vector3(transform.position.x - dir * bordaX, yDaPista, 0f);
        carro.Iniciar(dir, Random.Range(velocidade.x, velocidade.y), bordaX + 2f, c => pool.Release(c));
    }
}