using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpPower = 7f;
    public float gravity = 10f;

    [Tooltip("0 = no slip, higher = more slippery")]
    public float slipperiness = 0f;

    [Header("Crouch")]
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    [Header("Look")]
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    private CharacterController characterController;
    private float rotationX = 0f;

    [HideInInspector] public float verticalVelocity;
    private Vector3 currentHorizontalVelocity;

    private float defaultWalkSpeed;
    private float defaultRunSpeed;

    public bool canMove = true;
    public Animator animator;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultWalkSpeed = walkSpeed;
        defaultRunSpeed = runSpeed;

        // Safety check: tries to find the Animator automatically if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("No Animator found on this object or its children! Please drag the child model into the Animator slot in the Inspector.", this);
        }
    }

    void Update()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float inputZ = Input.GetAxisRaw("Vertical");
        float inputX = Input.GetAxisRaw("Horizontal");

        // 1. CROUCH HANDLING (evaluated first so animations and speed know the state)
        bool isCrouching = Input.GetKey(KeyCode.LeftControl) && canMove;

        if (isCrouching)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
            if (animator != null) animator.SetBool("IsCrouched", true);
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = defaultWalkSpeed;
            runSpeed = defaultRunSpeed;
            if (animator != null) animator.SetBool("IsCrouched", false);
        }

        // Calculated AFTER crouch so it always uses the current speeds
        float speed = canMove ? (isRunning ? runSpeed : walkSpeed) : 0f;

        // 2. ANIMATION CONTROLLER HANDLING
        if (animator != null)
        {
            bool isMoving = (inputZ != 0f || inputX != 0f);
            animator.SetBool("IsWalking", isMoving);

            // AnimSpeed now carries the real speed (walk = 6, run = 12),
            // so a transition condition of "AnimSpeed Greater 1.8" works.
            if (inputZ < 0)
            {
                animator.SetFloat("AnimSpeed", -speed);   // Moving backward
            }
            else if (inputZ > 0 || inputX != 0f)
            {
                animator.SetFloat("AnimSpeed", speed);    // Forward, or only strafing
            }
            else
            {
                animator.SetFloat("AnimSpeed", 0f);       // Standing still
            }

            // Strafe parameters (only allow standing strafe if NOT crouching)
            if (inputX > 0 && !isCrouching)
            {
                animator.SetFloat("Strafe", 1f);
                animator.SetBool("IsStrafe", true);
            }
            else if (inputX < 0 && !isCrouching)
            {
                animator.SetFloat("Strafe", -1f);
                animator.SetBool("IsStrafe", true);
            }
            else
            {
                animator.SetBool("IsStrafe", false);
            }
        }

        // 3. PHYSICS & CALCULATIONS
        Vector3 targetVelocity = (transform.forward * inputZ + transform.right * inputX) * speed;

        // Slipperiness control
        if (slipperiness <= 0f)
        {
            currentHorizontalVelocity = targetVelocity;
        }
        else
        {
            currentHorizontalVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                targetVelocity,
                Time.deltaTime * (1f / slipperiness)
            );
        }

        // Jump & gravity
        if (characterController.isGrounded)
        {
            if (Input.GetButtonDown("Jump") && canMove)
                verticalVelocity = jumpPower;
            else
                verticalVelocity = -1f;
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 move = currentHorizontalVelocity;
        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);

        // Mouse look
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    public void ResetLook()
    {
        rotationX = 0f;
        playerCamera.transform.localRotation = Quaternion.identity;
    }
}