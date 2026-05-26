using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private Vector2 moveInput;

    private Animator animator;

    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // freeze movement
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;

            animator.SetBool("isWalking", false);

            return;
        }

        // movement
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        // kalau freeze jangan baca input
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;

            animator.SetBool("isWalking", false);

            return;
        }

        moveInput = context.ReadValue<Vector2>();

        // walking animation
        animator.SetBool("isWalking", moveInput != Vector2.zero);

        // direction animation
        animator.SetFloat("InputX", moveInput.x);

        animator.SetFloat("InputY", moveInput.y);

        // last direction
        if (context.canceled)
        {
            animator.SetFloat("LastInputX", moveInput.x);

            animator.SetFloat("LastInputY", moveInput.y);
        }
    }
}