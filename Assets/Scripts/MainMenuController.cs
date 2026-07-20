using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Nome exato da cena da fase (tem que estar em File > Build Settings > Scenes in Build)")]
    public string gameplaySceneName = "SampleScene";

    // Chame este método no OnClick() do botão "Iniciar Jogo" no Canvas
    public void OnClickIniciar()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    // Opcional: botão de sair do jogo
    public void OnClickSair()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo (só funciona no build, não no Editor)");
    }
}