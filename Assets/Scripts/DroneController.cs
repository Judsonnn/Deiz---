using System.Collections;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public enum DroneState { Floating, Moving, Launching, Returning }

    [Header("Waypoints")]
    [Tooltip("Arraste os Transforms dos pontos de rota aqui, na ordem que o drone vai percorrer")]
    public Transform[] waypoints;

    [Header("Flutuação (estado passivo)")]
    public float floatAmplitude = 0.3f;
    public float floatFrequency = 1.5f;

    [Header("Movimento")]
    public float moveSpeed = 3f;
    [Tooltip("Distância pra considerar que chegou num waypoint")]
    public float waypointThreshold = 0.1f;

    [Header("Empurrão no último waypoint")]
    [Tooltip("Força vertical aplicada no player ao chegar no último waypoint")]
    public float launchForce = 15f;
    [Tooltip("Força horizontal aplicada no player (na direção que o drone estava andando)")]
    public float launchHorizontalForce = 8f;
    [Tooltip("Delay antes de retornar à posição inicial (dá tempo do player sair)")]
    public float returnDelay = 0.5f;

    [Header("Retorno")]
    public float returnSpeed = 5f;

    [Header("Detecção do Player em cima")]
    public float topDetectionHeight = 0.5f;

    private DroneState currentState = DroneState.Floating;
    private int currentWaypointIndex = 0;
    private Vector3 startPosition;
    private Transform playerOnTop;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        switch (currentState)
        {
            case DroneState.Floating:  HandleFloating();  break;
            case DroneState.Moving:    HandleMoving();    break;
            case DroneState.Returning: HandleReturning(); break;
        }
    }

    private void HandleFloating()
    {
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
                // Chegou no último waypoint — empurra o player pra cima e retorna
                currentState = DroneState.Launching;
                StartCoroutine(LaunchAndReturn());
            }
        }
    }

    private void HandleReturning()
    {
        // Volta suavemente pra posição inicial
        transform.position = Vector3.MoveTowards(
            transform.position,
            startPosition,
            returnSpeed * Time.deltaTime
        );

        // Chegou na posição inicial — volta a flutuar normalmente
        if (Vector3.Distance(transform.position, startPosition) <= 0.05f)
        {
            transform.position = startPosition;
            currentWaypointIndex = 0;
            currentState = DroneState.Floating;
        }
    }

    private IEnumerator LaunchAndReturn()
    {
        // Calcula a direção horizontal que o drone estava andando
        // (do waypoint anterior pro último — ou do início pro primeiro se só tiver 1)
        float horizontalDirection = 0f;
        if (waypoints.Length >= 2)
        {
            Transform prev = waypoints[waypoints.Length - 2];
            Transform last = waypoints[waypoints.Length - 1];
            horizontalDirection = Mathf.Sign(last.position.x - prev.position.x);
        }
        else if (waypoints.Length == 1)
        {
            horizontalDirection = Mathf.Sign(waypoints[0].position.x - startPosition.x);
        }

        // Empurra o player na direção do movimento do drone + pra cima
        if (playerOnTop != null)
        {
            Rigidbody2D playerRb = playerOnTop.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                ReleasePlayer();
                playerRb.linearVelocity = new Vector2(
                    horizontalDirection * launchHorizontalForce,
                    launchForce
                );
            }
            else
            {
                ReleasePlayer();
            }
        }

        yield return new WaitForSeconds(returnDelay);

        currentState = DroneState.Returning;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (currentState != DroneState.Floating) return;

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

        // Player saiu do drone (pulou durante a rota) — solta e retorna
        if (playerOnTop != null)
        {
            ReleasePlayer();

            if (currentState == DroneState.Moving)
                StartCoroutine(LaunchAndReturn());
        }
    }

    private void OnPlayerLanded(Transform player)
    {
        playerOnTop = player;
        player.SetParent(transform);

        if (waypoints != null && waypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            currentState = DroneState.Moving;
        }
    }

    private void ReleasePlayer()
    {
        if (playerOnTop == null) return;
        playerOnTop.SetParent(null);
        playerOnTop = null;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, 0.2f);

            if (i > 0 && waypoints[i - 1] != null)
                Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
        }

        if (waypoints[0] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, waypoints[0].position);
        }

        // Marca o ponto inicial de retorno em verde
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, 0.25f);
    }
}