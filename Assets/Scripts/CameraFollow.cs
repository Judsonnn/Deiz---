using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeedHorizontal = 5f;
    public float smoothSpeedUp = 4f;
    public float smoothSpeedDown = 12f; // desce rápido para mostrar a plataforma
    public Vector3 offset;

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 desiredPosition = player.position + offset;

        // Velocidade diferente para cada direção
        float speedY = desiredPosition.y < transform.position.y
            ? smoothSpeedDown  // descendo — rápido
            : smoothSpeedUp;   // subindo — suave

        float newX = Mathf.Lerp(transform.position.x, desiredPosition.x, smoothSpeedHorizontal * Time.deltaTime);
        float newY = Mathf.Lerp(transform.position.y, desiredPosition.y, speedY * Time.deltaTime);

        transform.position = new Vector3(newX, newY, transform.position.z);
    }
}