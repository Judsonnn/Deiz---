using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Som")]
    public AudioClip collectSound;
    [Range(0f, 1f)] public float collectVolume = 1f;

    private Vector3 startPosition;
    private bool canBeCollected = false;
    private string uid;

    void Awake()
    {
        Vector3 p = transform.position;
        uid = SceneManager.GetActiveScene().name + "_" + collectableID + "_"
              + Mathf.RoundToInt(p.x * 100) + "_" + Mathf.RoundToInt(p.y * 100);
    }

    void Start()
    {
        startPosition = transform.position;
        Invoke(nameof(EnableCollection), 0.2f);

        if (CollectableManager.Instance != null)
            CollectableManager.Instance.OnRestored += Refresh;

        Refresh();
    }

    void OnDestroy()
    {
        if (CollectableManager.Instance != null)
            CollectableManager.Instance.OnRestored -= Refresh;
    }

    private void EnableCollection()
    {
        canBeCollected = true;
    }

    private void Refresh()
    {
        bool collected = CollectableManager.Instance != null
                      && CollectableManager.Instance.IsCollected(uid);

        gameObject.SetActive(!collected);
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
        CollectableManager.Instance.MarkCollected(uid);

        PlayCollectSound();

        gameObject.SetActive(false);
    }

    private void PlayCollectSound()
    {
        if (collectSound == null) return;
        Vector3 pos = Camera.main != null ? Camera.main.transform.position : transform.position;
        AudioSource.PlayClipAtPoint(collectSound, pos, collectVolume);
    }
}