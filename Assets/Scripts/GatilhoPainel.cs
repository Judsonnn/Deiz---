using System.Collections;
using UnityEngine;

public class GatilhoPainel : MonoBehaviour
{
    [SerializeField] private GameObject painel;
    [SerializeField] private AudioSource som;
    [SerializeField] private float duracao = 4f;

    private bool usado;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (usado || !other.CompareTag("Player")) return;
        usado = true;
        StartCoroutine(Mostrar());
    }

    private IEnumerator Mostrar()
    {
        painel.SetActive(true);
        if (som != null) som.Play();
        yield return new WaitForSeconds(duracao);
        painel.SetActive(false);
    }
}