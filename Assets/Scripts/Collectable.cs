using UnityEngine;

public class Collectable : MonoBehaviour
{
    [Header("Configuração")]
    public string collectableID = "gem";
    public int value = 1;

    [Header("Efeito no mundo")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;
    public float rotateSpeed = 2f;
    public float rotateAngle = 45f;

    private Vector3 startPosition;
    private bool canBeCollected = false; // << declaração que estava faltando

    void Start()
    {
        startPosition = transform.position;
        Invoke(nameof(EnableCollection), 0.2f);
        void Start()
        {
            Debug.Log("Collectable iniciado: " + gameObject.name);
            startPosition = transform.position;
            Invoke(nameof(EnableCollection), 0.2f);
        }
    }

    private void EnableCollection()
    {
        canBeCollected = true;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        float angle = Mathf.Sin(Time.time * rotateSpeed) * rotateAngle;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canBeCollected) return;
        if (!other.CompareTag("Player")) return;

        if (CollectableManager.Instance == null)
        {
            Debug.LogError("CollectableManager não encontrado na cena!");
            return;
        }

        CollectableManager.Instance.Collect(collectableID, value);
        Destroy(gameObject);
    }
    
}