using UnityEngine;
using UnityEngine.SceneManagement;

public class FimDeFase : MonoBehaviour
{
    [Header("Cutscene")]
    [SerializeField] private GameObject painelCutscene;      // o painel com a cutscene e o botão
    [SerializeField] private string proximaFase = "Fase2";

    [Header("Opcional")]
    [SerializeField] private MonoBehaviour controlePlayer;   // script de movimento da Deiz, para desligar

    private bool jaAtivou;

    private void Start()
    {
        if (painelCutscene != null)
            painelCutscene.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (jaAtivou) return;
        if (!other.CompareTag("Player")) return;

        jaAtivou = true;

        if (controlePlayer != null)
            controlePlayer.enabled = false;

        painelCutscene.SetActive(true);
        Time.timeScale = 0f;     // pausa a fase
    }

    // ligue esta função no OnClick do botão
    public void IrParaProximaFase()
    {
        Time.timeScale = 1f;     // IMPORTANTE: senão a fase 2 abre congelada
        SceneManager.LoadScene(proximaFase);
    }
}