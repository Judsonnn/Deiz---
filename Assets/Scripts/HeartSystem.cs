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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HealthLogic();
        DeadState();
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

    // ReSharper disable Unity.PerformanceAnalysis
    void DeadState()
    {
        Debug.Log("vida atual" + vida);
        if (vida <= 0)
        {
            Debug.Log("Morreu");

            GetComponent<PlayerController>().enabled = false;
            Destroy(gameObject, 1.0f);
        }
       // if (vida <= 0)
        {
            
          //  Debug.Log("Game Over");
           // GetComponent<PlayerController>().enabled = false;
           // Destroy(gameObject, 1.0f);
        }
    }
}
