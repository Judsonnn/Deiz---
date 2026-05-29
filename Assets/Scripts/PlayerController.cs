using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 4.5f;
    public float jumpForce = 8f;
    public int maxJumps = 2;

    private int jumpCount;

    private Rigidbody2D rb;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;
    

    private bool isGrounded;
    public Transform cameraTarget;
    public float cameraSpeed;
    public float lookAheadDistance;

    private Vector3 targetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(move * speed, rb.linearVelocity.y);

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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
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
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            groundCheck.position,
            groundCheckSize
        );
    }
}