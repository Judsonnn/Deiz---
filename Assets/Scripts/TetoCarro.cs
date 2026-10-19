using UnityEngine;

public class TetoCarro : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (other.transform.parent == transform.parent) return;

        var rb = other.attachedRigidbody;
        if (rb != null && rb.linearVelocity.y <= 0.1f)   // só gruda se não estiver subindo
            other.transform.SetParent(transform.parent);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.transform.parent == transform.parent)
            other.transform.SetParent(null);
    }
}