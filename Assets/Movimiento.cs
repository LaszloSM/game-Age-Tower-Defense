using UnityEngine;

public class Movimiento : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed  = 5f;
    public float jumpForce  = 8f;

    [Header("Deteccion de Suelo")]
    public float     groundCheckDistance = 0.55f;
    public LayerMask groundLayer;

    Rigidbody rb;
    Animator  anim;
    private  bool    isGrounded;
    private float     h, v;

    void Start()
    {
        rb       = GetComponent<Rigidbody>();
        anim     = GetComponent<Animator>();
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        CheckGround();
        Move();
        Jump();
        UpdateAnimator();
        Attack();
    }

    void CheckGround()
    {
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
        if (groundLayer == 0)
            isGrounded = Physics.Raycast(raycastOrigin, Vector3.down, groundCheckDistance);
        else
            isGrounded = Physics.Raycast(raycastOrigin, Vector3.down, groundCheckDistance, groundLayer);
    }

    void Move()
    {
        Vector3 movement  = new Vector3(h, 0f, v) * moveSpeed;
        // Mantén la velocidad actual en Y, solo cambia X y Z
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);

        // CORREGIDO: Ahora rota también cuando retrocede (v < 0)
        if (movement.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Limpia la rotación angular para evitar giros raros
            rb.angularVelocity = Vector3.zero;
            
            // IMPORTANTE: Resetea la velocidad Y para que no se acumule
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            // Ahora sí aplica la fuerza
            ApplyJumpForce();
            
            // Dispara la animación
            anim.SetTrigger("Jump");
        }
    }

    void UpdateAnimator()
    {
        // Calcula solo la velocidad horizontal (sin Y)
        float speed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
        anim.SetFloat("Speed", speed);
        anim.SetBool("isGrounded", isGrounded);
    }

    public void ApplyJumpForce()
    {
        // IMPORTANTE: Esto debe llamarse desde Jump() o desde una animación
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * groundCheckDistance);
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Attack");
        }
    }
}