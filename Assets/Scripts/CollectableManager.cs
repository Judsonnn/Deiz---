// ─────────────────────────────────────────
// CollectableManager.cs — coloca em um
// GameObject vazio chamado "GameManager"
// ─────────────────────────────────────────
using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance;

    // Dicionário para suportar múltiplos tipos no futuro
    private Dictionary<string, int> collectableCounts = new Dictionary<string, int>();

    // Evento que a UI escuta
    public event System.Action<string, int> OnCollect;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Collect(string id, int value)
    {
        if (!collectableCounts.ContainsKey(id))
            collectableCounts[id] = 0;

        collectableCounts[id] += value;

        OnCollect?.Invoke(id, collectableCounts[id]);
    }

    public int GetCount(string id)
    {
        return collectableCounts.ContainsKey(id) ? collectableCounts[id] : 0;
    }
}