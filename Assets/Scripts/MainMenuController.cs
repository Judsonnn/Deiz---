
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Cena")]
    public string gameplaySceneName = "SampleScene";

    [Header("Painel de Configurações")]
    public GameObject configPanel;

    [Header("Controle de Volume")]
    public Slider volumeSlider;

    private const string VolumeKey = "VolumeJogo";

    private void Start()
    {
        // Fecha o painel ao iniciar
        if (configPanel != null)
            configPanel.SetActive(false);

        // Recupera o volume salvo
        float volumeSalvo = PlayerPrefs.GetFloat(VolumeKey, 1f);

        AudioListener.volume = volumeSalvo;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;

            volumeSlider.SetValueWithoutNotify(volumeSalvo);

            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // INICIAR JOGO
    public void OnClickIniciar()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    // ABRIR CONFIGURAÇÕES
    public void AbrirConfiguracoes()
    {
        if (configPanel != null)
            configPanel.SetActive(true);
    }

    // FECHAR CONFIGURAÇÕES
    public void FecharConfiguracoes()
    {
        if (configPanel != null)
            configPanel.SetActive(false);
    }

    // ALTERAR VOLUME
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    // SAIR DO JOGO
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
