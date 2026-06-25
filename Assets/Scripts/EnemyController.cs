using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 3f;
    public int damage = 1;

    private Transform player;
    private bool playerDetected;

    private bool canDamage = true;
    public float damageCooldown = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;
    
    [Header("Patrol")]
    public bool patrolRight = true;

    void Update()
    {
        if (player == null)
        {
            playerDetected = false;
            return;
        }

        if (!playerDetected)
            return;

        bool groundAhead = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
        
        Vector2 targetPosition = new Vector2(
            player.position.x,
            transform.position.y
        );

        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);

            if (groundAhead)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    speed * Time.deltaTime
                );
            }
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);

            if (groundAhead)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPosition,
                    speed * Time.deltaTime
                );
            }
        }
    }

    public void DetectPlayer(Transform target)
    {
        player = target;
        playerDetected = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!canDamage) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController =
                collision.gameObject.GetComponent<PlayerController>();

            if (playerController != null)
            {
                playerController.TakeDamage(
                    damage,
                    transform
                );

                canDamage = false;
                Invoke(nameof(ResetDamage), damageCooldown);
            }
        }
    }

    private void ResetDamage()
    {
        canDamage = true;
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
}