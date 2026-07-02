using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [Header("Alturas de queda")]
    public float mediumFallHeight = 4f;  // altura em unidades do Unity
    public float lethalFallHeight = 7f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool wasGrounded;
    private float highestPoint; // ponto mais alto durante a queda

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        highestPoint = transform.position.y;
    }

    void Update()
    {
        bool isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0,
            groundLayer
        );

        if (!isGrounded)
        {
            // Atualiza o ponto mais alto enquanto está no ar
            if (transform.position.y > highestPoint)
                highestPoint = transform.position.y;
        }

        // Acabou de pousar
        if (isGrounded && !wasGrounded)
        {
            float fallHeight = highestPoint - transform.position.y;

            Debug.Log("Altura da queda: " + fallHeight);

            if (fallHeight >= lethalFallHeight)
            {
                HeartSystem heart = GetComponent<HeartSystem>();
                if (heart != null)
                    heart.vida = 0;
            }
            else if (fallHeight >= mediumFallHeight)
            {
                HeartSystem heart = GetComponent<HeartSystem>();
                if (heart != null)
                    heart.vida -= 1;
            }

            // Reseta ao pousar
            highestPoint = transform.position.y;
        }

        wasGrounded = isGrounded;
    }
}