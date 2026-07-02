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
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    public float rotateSpeed = 2f;  // controla só a velocidade do balanço
    public float rotateAngle = 45f; // controla só o quanto inclina
    
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

   void Update()
   {
       // Flutua
       float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
       transform.position = new Vector3(transform.position.x, newY, transform.position.z);
   
       // Balança — velocidade e ângulo separados
       float angle = Mathf.Sin(Time.time * rotateSpeed) * rotateAngle;
       transform.rotation = Quaternion.Euler(0f, angle, 0f);
   }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Avisa o manager
        CollectableManager.Instance.Collect(collectableID, value);

        Destroy(gameObject);
    }
}