using UnityEngine;
using UnityEngine.SceneManagement;

public class FimDeFase : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private string proximaFase = "Fase2";
    [SerializeField] private bool irDireto = false;  

    [Header("Cutscene (só se Ir Direto estiver desmarcado)")]
    [SerializeField] private GameObject painelCutscene;

    [Header("Opcional")]
    [SerializeField] private MonoBehaviour controlePlayer;

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

        if (irDireto || painelCutscene == null)
        {
            IrParaProximaFase();
            return;
        }

        painelCutscene.SetActive(true);
        Time.timeScale = 0f;
    }

    public void IrParaProximaFase()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(proximaFase);
    }
}