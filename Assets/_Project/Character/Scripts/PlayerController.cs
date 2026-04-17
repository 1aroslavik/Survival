using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Camera playerCamera;
    public Transform groundCheck;
    public PlayerStats stats;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.8f;

    [Header("Gravity")]
    public float gravity = -20f;

    [Header("Mouse")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Ground Check")]
    public float groundDistance = 1.0f; // 🔥 увеличили
    public LayerMask groundMask;

    [Header("Jump Settings")]
    public float coyoteTime = 0.2f;
    public float jumpBufferTime = 0.2f;

    [Header("Swimming")]
    public bool inWater;
    public float swimSpeed = 4f;
    public float swimUpSpeed = 3f;
    public float waterGravity = -2f;

    float yVelocity;
    float xRotation = 0f;

    float coyoteTimer;
    float jumpBufferTimer;

    bool grounded;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (stats == null)
            stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 🌊 ПЛАВАНИЕ
        if (inWater)
        {
            Vector3 camForward = playerCamera.transform.forward;
            Vector3 camRight = playerCamera.transform.right;

            Vector3 swimDir = camForward * z + camRight * x;

            controller.Move(swimDir * swimSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.Space))
                yVelocity = swimUpSpeed;
            else if (Input.GetKey(KeyCode.LeftControl))
                yVelocity = -swimUpSpeed;
            else
                yVelocity = 0;

            yVelocity += waterGravity * Time.deltaTime;

            controller.Move(Vector3.up * yVelocity * Time.deltaTime);

            return;
        }

        // 🟩 ОБЫЧНОЕ ДВИЖЕНИЕ

        bool wantsSprint = Input.GetKey(KeyCode.LeftShift);
        bool canSprint = stats == null || stats.HasStamina();
        bool isSprinting = wantsSprint && canSprint;

        float speed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (stats != null)
        {
            stats.isSprinting = isSprinting;
            stats.isWalking = move.magnitude > 0.1f && !isSprinting;
        }

        // 🔥 НОВЫЙ GROUND CHECK (фикс камней)
        bool sphereGrounded = Physics.SphereCast(
            groundCheck.position,
            0.3f,
            Vector3.down,
            out RaycastHit hit,
            groundDistance + 0.5f,
            groundMask
        );

        grounded = sphereGrounded || controller.isGrounded;

        if (grounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (grounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }

        yVelocity += gravity * Time.deltaTime;

        controller.Move(Vector3.up * yVelocity * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SwimZone"))
        {
            inWater = true;
            yVelocity = 0f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SwimZone"))
        {
            inWater = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }
}