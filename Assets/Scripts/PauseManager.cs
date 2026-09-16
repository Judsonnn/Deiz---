using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Key")]
    public KeyCode pauseKey = KeyCode.Return;

    [Header("UI")]
    public GameObject pauseMenuUI;

    [Header("Cenas")]
    public string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        IsPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Placeholder pronto pra quando você tiver o sistema de volume.
    // Só chame isso a partir de um slider da UI: OnValueChanged -> SetMasterVolume
    public void SetMasterVolume(float value)
    {
        // Exemplo futuro:
        // AudioListener.volume = value;
        // ou audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }
}