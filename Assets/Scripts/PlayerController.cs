using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 4.5f;
    public float firstJumpForce = 8f;
    public float secondJumpForce = 5f;
    public int maxJumps = 2;
    
    public int health = 3;

    private int jumpCount;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
    

    private bool isGrounded;
    public Transform cameraTarget;
    public float cameraSpeed;
    public float lookAheadDistance;

    private Vector3 targetPosition;
    
    public float knockbackForce = 10f;

    private Rigidbody2D rb;
    
    private bool takingDamage = false;
    
  
    
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        if (move > 0)
        {
            transform.localScale = new Vector3(
                1.3f,
                1.68f,
                1f
            );
        }
        else if (move < 0)
        {
            transform.localScale = new Vector3(
                -1.3f,
                1.68f,
                1f
            );
        }

        if (!takingDamage)
        {
            rb.linearVelocity = new Vector2(
                move * speed,
                rb.linearVelocity.y
            );
        }

        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0,
            groundLayer
        );

        if (isGrounded)
        {
            jumpCount = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            if (jumpCount == 0)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    firstJumpForce
                );
            }
            else
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    secondJumpForce
                );
            }

            jumpCount++;
        }
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * 0.5f
            );
        }
        if (move > 0)
        {
            targetPosition = new Vector3(
                lookAheadDistance,
                cameraTarget.localPosition.y,
                cameraTarget.localPosition.z
            );
        }
        else if (move < 0)
        {
            targetPosition = new Vector3(
                -lookAheadDistance,
                cameraTarget.localPosition.y,
                cameraTarget.localPosition.z
            );
        }

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            targetPosition,
            cameraSpeed * Time.deltaTime
        );
    }
    public void TakeDamage(int damage, Transform enemy)
    {
        HeartSystem heart = GetComponent<HeartSystem>();

        if (heart != null)
        {
            heart.vida -= damage;
        }

        takingDamage = true;

        float direction =
            transform.position.x > enemy.position.x ? 1f : -1f;

        Vector2 knockbackDirection =
            new Vector2(direction, 1f).normalized;

        rb.linearVelocity = Vector2.zero;

        rb.AddForce(
            knockbackDirection * knockbackForce,
            ForceMode2D.Impulse
        );

        Invoke(nameof(StopTakingDamage), 0.3f);
    }
    private void StopTakingDamage()
    {
        takingDamage = false;
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            groundCheck.position,
            groundCheckSize
        );
    }
    public void TakeDamage(int damage)
    {
        health -= damage;

        Debug.Log("Vida: " + health);

        if (health <= 0)
        {
            Debug.Log("Morreu");
        }
    }
}