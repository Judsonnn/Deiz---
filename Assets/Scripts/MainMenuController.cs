
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Cena")]
    public string cutsceneSceneName = "Cutscene";
    public string gameplaySceneName = "Fase1";

    [Header("Painel de Configurações")]
    public GameObject configPanel;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Controle de Volume")]
    public Slider volumeSlider;
    public Slider efeitosSlider;

    [Header("Som de Teste dos Efeitos")]
    public AudioSource testeSource;
    public AudioClip somTeste;

    private const string VolumeKey = "VolumeJogo";
    private const string EfeitosKey = "VolumeEfeitos";

    private const string MusicParam = "MusicVolume";
    private const string SfxParam = "SFXVolume";

    private const float PadraoMusica = 0.6f;
    private const float PadraoEfeitos = 0.8f;

    private void Start()
    {
        if (configPanel != null)
            configPanel.SetActive(false);

        AudioListener.volume = 1f;

        float musicaSalva = PlayerPrefs.GetFloat(VolumeKey, PadraoMusica);
        float efeitosSalvo = PlayerPrefs.GetFloat(EfeitosKey, PadraoEfeitos);

        AplicarVolume(MusicParam, musicaSalva);
        AplicarVolume(SfxParam, efeitosSalvo);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.SetValueWithoutNotify(musicaSalva);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (efeitosSlider != null)
        {
            efeitosSlider.minValue = 0f;
            efeitosSlider.maxValue = 1f;
            efeitosSlider.SetValueWithoutNotify(efeitosSalvo);
            efeitosSlider.onValueChanged.AddListener(SetVolumeEfeitos);
        }
    }

    public void OnClickIniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cutsceneSceneName);
    }

    public void AbrirConfiguracoes()
    {
        if (configPanel != null)
            configPanel.SetActive(true);
    }

    public void FecharConfiguracoes()
    {
        if (configPanel != null)
            configPanel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        AplicarVolume(MusicParam, volume);

        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void SetVolumeEfeitos(float volume)
    {
        AplicarVolume(SfxParam, volume);

        PlayerPrefs.SetFloat(EfeitosKey, volume);
        PlayerPrefs.Save();
    }

    public void TocarSomTeste()
    {
        if (testeSource != null && somTeste != null)
            testeSource.PlayOneShot(somTeste);
    }

    private void AplicarVolume(string parametro, float valor)
    {
        if (mixer == null)
            return;

        float seguro = Mathf.Max(valor, 0.0001f);
        mixer.SetFloat(parametro, Mathf.Log10(seguro) * 20f);
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
