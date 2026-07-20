using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class HeartSystem : MonoBehaviour
{
    public int vida;
    public int vidaMaxima;

    public Image[] coracao;
    
    public Sprite cheio;

    public Sprite vazio;

    // Garante que a lógica de morte (tela de morte, destruir o player) só
    // rode UMA VEZ, mesmo o Update() chamando DeadState() todo frame.
    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HealthLogic();
        DeadState();
        if (Input.GetKeyDown(KeyCode.K))
        {
            vida--;
        }
    }
    
    void HealthLogic()
    {
        if (vida > vidaMaxima)
        {
            vida = vidaMaxima;
        }
        for (int i = 0; i < coracao.Length; i++)
        {
            if (i < vida)
            {
                coracao[i].sprite = cheio;
            }
            else
            {
                coracao[i].sprite = vazio;
            }
            if (i < vidaMaxima)
            {
                coracao[i].enabled = true;
            }
            else
            {
                {
                    coracao[i].enabled = false;
                }
            }
        }
    }
    
    void DeadState()
    {
        if (isDead) return; // já morreu, não repete a lógica todo frame

        if (vida < 0)
        {
            vida = 0;
        }

        if (vida <= 0)
        {
            isDead = true;

            Debug.Log("Morreu");

            GetComponent<PlayerController>().enabled = false;

            if (DeathScreenManager.Instance != null)
                DeathScreenManager.Instance.ShowDeathScreen();
            else
                Debug.LogWarning("DeathScreenManager não encontrado na cena!");

            Destroy(gameObject, 1.0f);
        }
    }
}