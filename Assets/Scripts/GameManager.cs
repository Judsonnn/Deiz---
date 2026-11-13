
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Checkpoint")]
    private bool hasCheckpoint = false;
    private Vector3 checkpointPosition;

    [Header("Musica de Gameplay")]
    public AudioSource musicaGameplay;

    [Tooltip("Nome exato da cena da primeira fase")]
    public string fase1SceneName = "Fase1";

    [Tooltip("Nome exato da cena da segunda fase")]
    public string fase2SceneName = "Fase2";

    [Tooltip("Nome da cena do menu")]
    public string menuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += AoCarregarCena;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Verifica a cena em que o jogo começou
        ControlarMusica(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
        }
    }

    private void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        ControlarMusica(cena.name);
    }

    private void ControlarMusica(string nomeCena)
    {
        if (musicaGameplay == null)
            return;

        bool deveTocar =
            nomeCena == fase1SceneName ||
            nomeCena == fase2SceneName;

        if (deveTocar)
        {
            if (!musicaGameplay.isPlaying)
                musicaGameplay.Play();
        }
        else if (nomeCena == menuSceneName)
        {
            musicaGameplay.Stop();
        }
    }

    public void SetCheckpoint(Vector3 position)
    {
        hasCheckpoint = true;
        checkpointPosition = position;

        Debug.Log("Checkpoint salvo em: " + position);
    }

    public bool TryGetCheckpoint(out Vector3 position)
    {
        position = checkpointPosition;
        return hasCheckpoint;
    }

    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
    }
}
