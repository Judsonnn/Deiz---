// ─────────────────────────────────────────
// Collectable.cs — coloca no objeto coletável
// ─────────────────────────────────────────
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Configuração")]
    public string collectableID = "gem"; // ID único por tipo de coletável
    public int value = 1;

    [Header("Efeito no mundo")]
    public float bobSpeed = 2f;       // velocidade de flutuar
    public float bobHeight = 0.2f;    // altura do flutuar
    public float rotateSpeed = 90f;   // giro suave no mundo

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Flutua suavemente no mundo
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Gira suavemente no mundo
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Avisa o manager
        CollectableManager.Instance.Collect(collectableID, value);

        Destroy(gameObject);
    }
}