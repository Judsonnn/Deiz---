
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class DialogoCutscene
{
    [Header("Fala")]
    public string nomePersonagem;

    [TextArea(3, 6)]
    public string fala;

    [Header("Imagem da personagem")]
    public Sprite imagemPersonagem;
    public bool mostrarPersonagem = true;

    [Header("Posição da personagem")]
    public bool alterarPosicao = false;
    public Vector2 posicaoPersonagem;

    [Header("Cenário")]
    public Sprite imagemFundo;
}

public class CutsceneController : MonoBehaviour
{
    [Header("Elementos da tela")]
    public Image fundo;
    public Image personagem;
    public TMP_Text nomePersonagem;
    public TMP_Text textoFala;
    public TMP_Text indicadorContinuar;

    [Header("Configuração dos diálogos")]
    public List<DialogoCutscene> dialogos = new List<DialogoCutscene>();

    [Header("Final da cutscene")]
    public string nomeCenaGameplay = "Fase1";

    private int dialogoAtual = 0;
    private RectTransform personagemRect;

    private void Start()
    {
        personagemRect = personagem.GetComponent<RectTransform>();

        if (dialogos.Count > 0)
        {
            MostrarDialogo();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ProximoDialogo();
        }
    }

    private void ProximoDialogo()
    {
        dialogoAtual++;

        if (dialogoAtual < dialogos.Count)
        {
            MostrarDialogo();
        }
        else
        {
            SceneManager.LoadScene(nomeCenaGameplay);
        }
    }

    private void MostrarDialogo()
    {
        DialogoCutscene dialogo = dialogos[dialogoAtual];

        nomePersonagem.text = dialogo.nomePersonagem;
        textoFala.text = dialogo.fala;

        personagem.gameObject.SetActive(dialogo.mostrarPersonagem);

        if (dialogo.imagemPersonagem != null)
        {
            personagem.sprite = dialogo.imagemPersonagem;
        }

        if (dialogo.alterarPosicao)
        {
            personagemRect.anchoredPosition = dialogo.posicaoPersonagem;
        }

        if (dialogo.imagemFundo != null)
        {
            fundo.sprite = dialogo.imagemFundo;
        }

        if (indicadorContinuar != null)
        {
            indicadorContinuar.gameObject.SetActive(true);
        }
    }
}
