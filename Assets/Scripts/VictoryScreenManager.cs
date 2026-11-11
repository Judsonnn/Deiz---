
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreenManager : MonoBehaviour
{
    [Header("Painel de Vitória")]
    public GameObject victoryPanel;

    [Header("Cena do Menu")]
    public string mainMenuSceneName = "MainMenu";

    private bool victoryShown = false;

    private void Start()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void ShowVictoryScreen()
    {
        if (victoryShown)
            return;

        victoryShown = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void OnClickVoltarMenu()
    {
        Time.timeScale = 1f;

        if (CollectableManager.Instance != null)
            CollectableManager.Instance.ClearAll();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnClickSair()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
