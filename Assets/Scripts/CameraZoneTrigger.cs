using UnityEngine;
using Unity.Cinemachine;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Câmeras")]
    public CinemachineCamera normalCam;
    public CinemachineCamera zoomCam;

    [Header("Prioridades")]
    public int normalPriority = 10;
    public int zoomPriority = 20;

    [Header("Transição")]
    public float blendInSpeed = 0.2f;  // rápido para abrir
    public float blendOutSpeed = 1.2f; // suave para voltar

    private CinemachineBrain brain;

    void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Ativa zoom out — transição rápida
        brain.DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Styles.EaseIn,
            blendInSpeed
        );

        zoomCam.Priority = zoomPriority;
        normalCam.Priority = normalPriority;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Volta ao normal — transição suave
        brain.DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Styles.EaseOut,
            blendOutSpeed
        );

        zoomCam.Priority = 0;
        normalCam.Priority = normalPriority;
    }
}