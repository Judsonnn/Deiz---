using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance;

    [Tooltip("O painel de UI que aparece quando o player morre (arraste o DeathPanel aqui)")]
    public GameObject deathPanel;

    private void Awake()
    {
        // Padrão simples de singleton, pra qualquer script (tipo o do Player)
        // conseguir chamar DeathScreenManager.Instance.ShowDeathScreen()
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

    // Chame este método de dentro do seu script de Player, no momento
    // em que a vida chega a 0 (ex: dentro do TakeDamage ou de um método Die())
    public void ShowDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        Time.timeScale = 0f; // pausa o jogo (opcional, remova se não quiser pausar)
    }

    // Chame este método no OnClick() do botão "Reiniciar" dentro do DeathPanel
    public void OnClickReiniciar()
    {
        Time.timeScale = 1f; // garante que o jogo volta a rodar normalmente
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Opcional: botão pra voltar ao menu principal a partir da tela de morte
    public void OnClickVoltarMenu(string mainMenuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}