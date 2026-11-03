using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance;

    [Tooltip("O painel de UI que aparece quando o player morre (arraste o DeathPanel aqui)")]
    public GameObject deathPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void OnClickReiniciar()
    {
        Time.timeScale = 1f;

        if (CollectableManager.Instance != null)
            CollectableManager.Instance.RestoreCheckpointSnapshot();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickVoltarMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f;

        if (CollectableManager.Instance != null)
            CollectableManager.Instance.ClearAll();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}