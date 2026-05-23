using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementKiro : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Movement
        rb.linearVelocity = moveInput * moveSpeed;

        // Animasi jalan
        animator.SetBool("isWalking", moveInput != Vector2.zero);

        // Blend Tree Walk
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        // Simpan arah terakhir untuk idle
        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}