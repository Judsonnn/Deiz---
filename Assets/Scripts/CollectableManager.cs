using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance;

    private Dictionary<string, int> collectableCounts = new Dictionary<string, int>();
    private Dictionary<string, int> checkpointCounts = new Dictionary<string, int>();

    private HashSet<string> collectedIds = new HashSet<string>();
    private HashSet<string> checkpointIds = new HashSet<string>();

    public event System.Action<string, int> OnCollect;
    public event System.Action OnRestored;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
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

    public bool IsCollected(string uid)
    {
        return collectedIds.Contains(uid);
    }

    public void MarkCollected(string uid)
    {
        collectedIds.Add(uid);
    }

    public void SaveCheckpointSnapshot()
    {
        checkpointCounts = new Dictionary<string, int>(collectableCounts);
        checkpointIds = new HashSet<string>(collectedIds);

        Debug.Log("SAVE snapshot | gem = " + GetCount("gem") + " | ids = " + checkpointIds.Count);
    }

    public void RestoreCheckpointSnapshot()
    {
        collectableCounts = new Dictionary<string, int>(checkpointCounts);
        collectedIds = new HashSet<string>(checkpointIds);

        Debug.Log("RESTORE snapshot | gem = " + GetCount("gem") + " | ids = " + collectedIds.Count);

        OnRestored?.Invoke();
    }

    public void ClearAll()
    {
        collectableCounts.Clear();
        checkpointCounts.Clear();
        collectedIds.Clear();
        checkpointIds.Clear();
    }
}