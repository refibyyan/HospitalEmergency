using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("References")]
    public ESP32Input espInput;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    [Header("SFX")]
    public AudioSource footstepSource;
    public AudioClip derapLangkah;

    [Range(0f, 1f)]
    public float footstepVolume = 1f;

    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (espInput == null)
            Debug.LogWarning("[PlayerMovement] espInput BELUM di-assign di Inspector!");

        // FOOTSTEP SETUP
        if (footstepSource != null)
        {
            footstepSource.playOnAwake = false;
            footstepSource.loop = true;
            footstepSource.volume = footstepVolume;
        }
    }

    void Update()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;

            UpdateAnimator();

            HandleFootstepSound();

            return;
        }

        // ==================================================
        // PRIORITY INPUT: ESP32
        // ==================================================

        if (espInput != null && espInput.isConnected)
        {
            moveInput = new Vector2(
                espInput.horizontal,
                espInput.vertical
            );
        }

        // Kalau espInput null/tidak konek,
        // moveInput diisi oleh Move() dari InputSystem

        UpdateAnimator();

        HandleFootstepSound();
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        bool isWalking = moveInput != Vector2.zero;

        animator.SetBool("isWalking", isWalking);

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (isWalking)
        {
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }

    // =========================================
    // FOOTSTEP SOUND
    // =========================================

    private void HandleFootstepSound()
    {
        bool isWalking =
            animator != null &&
            animator.GetBool("isWalking");

        // PLAY FOOTSTEP
        if (isWalking)
        {
            if (footstepSource != null &&
                derapLangkah != null)
            {
                footstepSource.volume =
                    footstepVolume;

                if (!footstepSource.isPlaying)
                {
                    footstepSource.clip =
                        derapLangkah;

                    footstepSource.loop = true;

                    footstepSource.Play();
                }
            }
        }
        // STOP FOOTSTEP
        else
        {
            if (footstepSource != null &&
                footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

    // INPUT SYSTEM (keyboard / gamepad)
    // dinonaktifkan kalau ESP32 konek

    public void Move(InputAction.CallbackContext context)
    {
        if (espInput != null &&
            espInput.isConnected)
            return;

        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput =
            Vector2.ClampMagnitude(
                context.ReadValue<Vector2>(),
                1f
            );
    }

    // Override manual
    public void SetExternalInput(Vector2 input)
    {
        moveInput =
            Vector2.ClampMagnitude(
                input,
                1f
            );
    }
}