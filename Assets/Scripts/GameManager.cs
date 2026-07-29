using UnityEngine;

// Este script deve ficar num GameObject chamado "GameManager", dentro da
// cena MainMenu (a PRIMEIRA cena que carrega quando o jogo abre).
//
// Por quê na MainMenu e não na cena da fase? Porque esse objeto usa
// DontDestroyOnLoad — ele precisa existir ANTES de qualquer troca de cena
// pra conseguir sobreviver a ela. Se ele nascesse só dentro da fase, um
// SceneManager.LoadScene(mesma cena) o destruiria e recriaria do zero,
// perdendo o checkpoint salvo — exatamente o problema que queremos evitar.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private bool hasCheckpoint = false;
    private Vector3 checkpointPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Já existe um GameManager (sobrevivente de antes do reload) —
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

    // Chamado pelo PlayerController no Start(), pra saber onde nascer.
    // Retorna true + a posição se existir checkpoint; false se não existir
    // (nesse caso o player deve nascer na posição padrão que já está na cena).
    public bool TryGetCheckpoint(out Vector3 position)
    {
        position = checkpointPosition;
        return hasCheckpoint;
    }

    // Chame isso ao voltar pro menu ou começar um "Novo Jogo", pra não
    // carregar um checkpoint de uma partida anterior por engano.
    public void ClearCheckpoint()
    {
        hasCheckpoint = false;
    }
}