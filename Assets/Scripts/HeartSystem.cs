
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartSystem : MonoBehaviour
{
    [Header("Vida")]
    public int vida;
    public int vidaMaxima;

    public Image[] coracao;
    public Sprite cheio;
    public Sprite vazio;

    [Header("Piscar ao receber dano")]
    [SerializeField] private float tempoInvulnerabilidade = 1.2f;
    [SerializeField] private float intervaloPiscar = 0.08f;

    private SpriteRenderer[] sprites;
    private Coroutine rotinaPiscar;

    private bool invulneravel;
    private bool isDead = false;

    void Start()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>();
        HealthLogic();
    }

    void Update()
    {
        HealthLogic();
        DeadState();
    }

    // Chamado pelos inimigos e pelo boss.
    public void ReceberDano(int dano)
    {
        if (isDead || invulneravel || dano <= 0)
            return;

        vida -= dano;
        vida = Mathf.Clamp(vida, 0, vidaMaxima);

        if (rotinaPiscar != null)
            StopCoroutine(rotinaPiscar);

        rotinaPiscar = StartCoroutine(Piscar());
    }

    private IEnumerator Piscar()
    {
        invulneravel = true;

        float tempo = 0f;
        bool visivel = true;

        while (tempo < tempoInvulnerabilidade)
        {
            visivel = !visivel;

            foreach (SpriteRenderer sr in sprites)
            {
                if (sr != null)
                    sr.enabled = visivel;
            }

            yield return new WaitForSeconds(intervaloPiscar);
            tempo += intervaloPiscar;
        }

        foreach (SpriteRenderer sr in sprites)
        {
            if (sr != null)
                sr.enabled = true;
        }

        invulneravel = false;
        rotinaPiscar = null;
    }

    void HealthLogic()
    {
        vida = Mathf.Clamp(vida, 0, vidaMaxima);

        for (int i = 0; i < coracao.Length; i++)
        {
            if (i < vida)
                coracao[i].sprite = cheio;
            else
                coracao[i].sprite = vazio;

            coracao[i].enabled = i < vidaMaxima;
        }
    }

    void DeadState()
    {
        if (isDead) return;

        if (vida <= 0)
        {
            isDead = true;

            if (rotinaPiscar != null)
                StopCoroutine(rotinaPiscar);

            foreach (SpriteRenderer sr in sprites)
            {
                if (sr != null)
                    sr.enabled = true;
            }

            PlayerController controlador =
                GetComponent<PlayerController>();

            if (controlador != null)
                controlador.enabled = false;

            if (DeathScreenManager.Instance != null)
                DeathScreenManager.Instance.ShowDeathScreen();
            else
                Debug.LogWarning(
                    "DeathScreenManager não encontrado na cena!"
                );

            Destroy(gameObject, 1f);
        }
    }
}
