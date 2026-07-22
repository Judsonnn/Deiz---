using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool hasCheckpoint = false;
    private Vector3 checkpointPosition;

    private void Awake()
    {
        // Singleton persistente: sobrevive ao SceneManager.LoadScene,
        // por isso o checkpoint não se perde quando a fase reinicia.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Já existe um GameManager (veio de antes do reload) —
            // esse novo, recém-criado pela cena, é descartado.
            Destroy(gameObject);
        }
    }

    // Chamado pelo script Checkpoint quando o player passa por um deles
    public void SetCheckpoint(Vector3 position)
    {
        hasCheckpoint = true;
        checkpointPosition = position;
        Debug.Log("Checkpoint salvo em: " + position);
    }

    // Chamado pelo PlayerController no Start(), pra saber onde nascer
    public bool TryGetCheckpoint(out Vector3 position)
    {
        position = checkpointPosition;
        return hasCheckpoint;
    }

    // Opcional: usar isso no botão "Voltar ao Menu" ou ao começar um New Game,
    // pra não carregar um checkpoint de uma partida anterior por engano
    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
    }
}