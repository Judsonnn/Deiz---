using UnityEngine;
using Unity.Cinemachine;

public class CameraZoneTrigger : MonoBehaviour
{
    [Header("Câmera de Zoom desta zona")]
    public CinemachineCamera zoomCam;

    [Header("Prioridades")]
    public int zoomPriority = 20;
    public int normalPriority = 10;

    [Header("Transição")]
    public float blendInSpeed = 0.2f;
    public float blendOutSpeed = 1.2f;

    private CinemachineBrain brain;

    void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();

        // Garante que esta câmera começa desligada
        if (zoomCam != null)
        {
            zoomCam.Priority = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Procura todas as zonas de zoom
        CameraZoneTrigger[] zones =
            FindObjectsByType<CameraZoneTrigger>(FindObjectsSortMode.None);

        // Desativa o zoom de todas as outras zonas
        foreach (CameraZoneTrigger zone in zones)
        {
            if (zone != this && zone.zoomCam != null)
            {
                zone.zoomCam.Priority = 0;
            }
        }

        // Transição rápida para o zoom
        if (brain != null)
        {
            brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseIn,
                blendInSpeed
            );
        }

        // Ativa o zoom desta zona
        if (zoomCam != null)
        {
            zoomCam.Priority = zoomPriority;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Transição suave de volta para a câmera normal
        if (brain != null)
        {
            brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseOut,
                blendOutSpeed
            );
        }

        // Desativa o zoom
        if (zoomCam != null)
        {
            zoomCam.Priority = 0;
        }

        // A câmera normal volta automaticamente,
        // pois ela permanece com prioridade 10.
    }
}
