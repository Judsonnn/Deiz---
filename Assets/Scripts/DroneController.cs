using UnityEngine;

public class DroneController : MonoBehaviour
{
    public enum DroneState { Floating, Moving, Done }

    [Header("Waypoints")]
    [Tooltip("Arraste os Transforms dos pontos de rota aqui, na ordem que o drone vai percorrer")]
    public Transform[] waypoints;

    [Header("Flutuação (estado passivo)")]
    public float floatAmplitude = 0.3f;   // altura do movimento senoidal (sobe/desce)
    public float floatFrequency = 1.5f;   // velocidade da flutuação

    [Header("Movimento")]
    public float moveSpeed = 3f;
    [Tooltip("Distância pra considerar que chegou num waypoint")]
    public float waypointThreshold = 0.1f;

    [Header("Detecção do Player em cima")]
    [Tooltip("Altura máxima acima do drone pra considerar que o player está em cima (não do lado)")]
    public float topDetectionHeight = 0.5f;

    private DroneState currentState = DroneState.Floating;
    private int currentWaypointIndex = 0;
    private Vector3 startPosition;
    private Transform playerOnTop;  // referência ao player enquanto estiver em cima

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        switch (currentState)
        {
            case DroneState.Floating: HandleFloating(); break;
            case DroneState.Moving:   HandleMoving();   break;
            case DroneState.Done:                       break;
        }
    }

    private void HandleFloating()
    {
        // Movimento senoidal no eixo Y — cria o efeito de "boiar" passivamente
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void HandleMoving()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(target.position.x, target.position.y, transform.position.z),
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) <= waypointThreshold)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                // Chegou no último waypoint — para e solta o player
                currentState = DroneState.Done;
                ReleasePlayer();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Verifica se o player está em cima do drone (não do lado nem embaixo)
        // comparando se o ponto de contato está acima do centro do drone
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.point.y >= transform.position.y + topDetectionHeight * 0.5f)
            {
                OnPlayerLanded(collision.transform);
                return;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Player saiu do drone (pulou ou caiu) — solta ele independente do estado
        ReleasePlayer();
    }

    private void OnPlayerLanded(Transform player)
    {
        if (currentState != DroneState.Floating) return;

        // Gruda o player no drone virando-o filho temporário —
        // assim ele se move junto sem precisar de nenhum script no player
        playerOnTop = player;
        player.SetParent(transform);

        // Inicia a rota
        if (waypoints != null && waypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            currentState = DroneState.Moving;
        }
    }

    private void ReleasePlayer()
    {
        if (playerOnTop == null) return;

        // Desvincula o player do drone — ele volta a se mover normalmente
        playerOnTop.SetParent(null);
        playerOnTop = null;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Desenha uma esfera em cada waypoint e uma linha ligando todos eles,
        // assim dá pra ver e ajustar a rota direto na Scene View
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

            if (i > 0 && waypoints[i - 1] != null)
                Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
        }

        // Linha amarela do drone pro primeiro waypoint
        if (waypoints[0] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, waypoints[0].position);
        }
    }
}