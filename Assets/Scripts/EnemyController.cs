using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f;
    
    public int damage = 1;

    private Transform player;
    private bool playerDetected;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer;

    void Update()
    {
        if (playerDetected)
        {
            bool groundAhead = Physics2D.Raycast(
                groundCheck.position,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            Debug.Log("Ground Ahead: " + groundAhead);

            if (!groundAhead)
            {
                Debug.Log("Sem chão");
                return;
            }

            Vector2 targetPosition = new Vector2(
                player.position.x,
                transform.position.y
            );

            if (player.position.x > transform.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            groundCheck.position,
            groundCheck.position + Vector3.down * groundCheckDistance
        );
    }

    public void DetectPlayer(Transform target)
    {
        player = target;
        playerDetected = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colidiu");

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Acertou Player");

            PlayerController player =
                collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}