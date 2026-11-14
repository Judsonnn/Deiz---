using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZonaSemChuva : MonoBehaviour
{
    public static readonly List<Collider2D> Zonas = new List<Collider2D>();

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (col == null) col = GetComponent<Collider2D>();
        if (!Zonas.Contains(col)) Zonas.Add(col);
    }

    private void OnDisable()
    {
        Zonas.Remove(col);
    }
}