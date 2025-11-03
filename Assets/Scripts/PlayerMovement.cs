using System.IO.Compression;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerControl controls;
    private Vector2 movementInput;
    private bool jumpPressed;
    private Rigidbody rb;
    private CapsuleCollider col;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Look Settings")]
    public float lookSensitivity = 2f;
    public float maxLookAngle = 80f;

    private Vector2 lookInput;
    private float pitch;

    [Header("Sprint Settings")]
    public float sprintMultiplier = 1.5f;
    private bool isSprinting;
    public bool canSprint = false;
    public float airControlMultiplier = 0.4f;

    [Header("Crouch Settings")]
    public float crouchHeight;
    public float standingHeight;
    public float crouchShrink = 1.5f;
    public float crouchSpeedMultiplier = 0.5f;
    private bool isCrouching;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float gravityMultiplier = 2.5f;
    private bool isJumping;
    private float jumpTime;

    [Header("Head Bobbing")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;
    private float bobTimer;
    private Vector3 camInitialPos;
    public GameObject camHolder;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        controls = new PlayerControl();

        PhysicsMaterial mat = new()
        {
            bounceCombine = PhysicsMaterialCombine.Average,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            dynamicFriction = 0f,
            staticFriction = 0f
        };

        gameObject.GetComponent<CapsuleCollider>().material = mat;

        controls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => movementInput = Vector2.zero;
        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += _ => lookInput = Vector2.zero;

        controls.Player.Jump.performed += _ => jumpPressed = true;

        controls.Player.Sprint.started += _ =>  isSprinting = canSprint;
        controls.Player.Sprint.canceled += _ => isSprinting = false;
        
        controls.Player.Crouch.started += _ => StartCrouch();
        controls.Player.Crouch.canceled += _ => StopCrouch();

        crouchHeight = col.height / crouchShrink;
        standingHeight = col.height;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        camInitialPos = camHolder.transform.localPosition;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        float currentSpeed = moveSpeed * (isSprinting && !isCrouching ? sprintMultiplier : 1f);
        if (isCrouching)
            currentSpeed *= crouchSpeedMultiplier;
        float currentControl = isGrounded() ? 1f : airControlMultiplier;

        Vector3 move = new(movementInput.x, 0, movementInput.y);
        Vector3 moveDir = transform.TransformDirection(move);
        Vector3 targetVelocity = currentControl * currentSpeed * moveDir;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        if (jumpPressed && isGrounded())
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                jumpForce,
                rb.linearVelocity.z
            );
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = true;
            jumpTime = Time.time;
            StopCrouch();
        }

        if (isJumping)
        {
            float gravityFactor = 1f + gravityMultiplier * (Time.time - jumpTime);
            rb.linearVelocity += gravityFactor * Time.deltaTime * Vector3.down;
        }

        jumpPressed = false;
    }
    void Update()
    {
        transform.Rotate(lookInput.x * lookSensitivity * Vector3.up);

        pitch -= lookInput.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        if (camHolder != null)
            camHolder.transform.localRotation = Quaternion.Euler(pitch, 0, 0);

        HandleHeadBob();
    }

    private bool isGrounded()
    {
        Ray ray = new(
            groundCheck.position,
            Vector3.down
        );
        float rayLength = (col.height / 2) + 0.1f;

        return Physics.Raycast(ray, rayLength, groundLayer);
    }

    private void StartCrouch()
    {
        isCrouching = true;
        col.height = crouchHeight;
    }

    private void StopCrouch()
    {
        isCrouching = false;
        col.height = standingHeight;
    }

    private void HandleHeadBob()
    {
        if (camHolder == null) return;

        bool isMoving = movementInput.magnitude > 0.1f && isGrounded();

        if (isMoving && !isCrouching) 
        {
            bobTimer += Time.deltaTime * bobSpeed * (isSprinting ? 1.5f : 1f);
            float bobX = Mathf.Sin(bobTimer) * bobAmount;
            float bobY = Mathf.Cos(bobTimer * 2) * bobAmount;

            camHolder.transform.localPosition = camInitialPos + new Vector3(bobX, bobY, 0f);
        }
        else
        {
            bobTimer = 0f;
            camHolder.transform.localPosition = Vector3.Lerp(
                camHolder.transform.localPosition,
                camInitialPos,
                Time.deltaTime * bobSpeed
            );
        }
    }
}