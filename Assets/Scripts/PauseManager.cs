
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Key")]
    public KeyCode pauseKey = KeyCode.Return;

    [Header("UI")]
    public GameObject pauseMenuUI;
    public GameObject configPanelUI;

    [Header("Cenas")]
    public string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    // ABRIR MENU DE PAUSA
    public void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        if (configPanelUI != null)
            configPanelUI.SetActive(false);

        Time.timeScale = 0f;
        IsPaused = true;
    }

    // CONTINUAR O JOGO
    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (configPanelUI != null)
            configPanelUI.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
    }

    // ABRIR CONFIGURAÇÕES
    public void AbrirConfiguracoes()
    {
        configPanelUI.SetActive(true);
        pauseMenuUI.SetActive(false);

        Debug.Log("ConfigPainel ativo: " + configPanelUI.activeSelf);
        Debug.Log("ConfigPainel visível na Hierarchy: " + configPanelUI.activeInHierarchy);
    }

    // VOLTAR DAS CONFIGURAÇÕES PARA A PAUSA
    public void FecharConfiguracoes()
    {
        if (configPanelUI != null)
            configPanelUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    // VOLTAR AO MENU PRINCIPAL
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // SAIR DO JOGO
    public void QuitGame()
    {
        Time.timeScale = 1f;

        IsPaused = false;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // CONTROLE DE VOLUME
    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
    }
}
