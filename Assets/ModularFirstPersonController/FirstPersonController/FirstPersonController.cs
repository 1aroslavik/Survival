using UnityEngine;
using UnityEngine.UI;

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("WATER")]
    public bool isInWater = false;
    public float swimSpeed = 3f;
    public float swimVerticalSpeed = 3f;
    public float waterDrag = 3f;

    [Header("Sprint Thresholds")]
    public float staminaToStartSprint = 30f;
    public float staminaToStopSprint = 5f;
    bool sprintLocked;

    [Header("STATS")]
    public PlayerStats stats;

    #region Camera
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    float yaw;
    float pitch;
    Image crosshairObject;
    #endregion

    #region Movement
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    bool isWalking;
    bool isGrounded;
    #endregion

    #region Sprint
    public bool enableSprint = true;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    [Header("Stamina Cost")]
    public float sprintStaminaCostPerSecond = 20f;
    public float jumpStaminaCost = 25f;

    public bool useSprintBar = true;
    public Image sprintBar;

    bool isSprinting;
    #endregion

    #region Jump
    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        crosshairObject = GetComponentInChildren<Image>();
        playerCamera.fieldOfView = fov;
    }

    void Start()
    {
        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else crosshairObject.gameObject.SetActive(false);
    }

    void Update()
    {
        #region Camera
        if (cameraCanMove)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch += (invertCamera ? 1 : -1) * Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
        #endregion

        if (enableJump && Input.GetKeyDown(jumpKey))
            Jump();

        CheckGround();
        UpdateSprintBar();
    }

    void FixedUpdate()
    {
        if (isInWater)
        {
            HandleSwimming();
            return;
        }

        if (!playerCanMove) return;

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool hasMoveInput = input.sqrMagnitude > 0.01f;

        Vector3 targetVelocity = transform.TransformDirection(input) * walkSpeed;

        isWalking = hasMoveInput && isGrounded;

        if (!sprintLocked)
        {
            if (enableSprint &&
                hasMoveInput &&
                Input.GetKey(sprintKey) &&
                stats != null &&
                stats.stamina >= staminaToStartSprint)
            {
                sprintLocked = true;
            }
        }
        else
        {
            if (!Input.GetKey(sprintKey) ||
                !hasMoveInput ||
                stats == null ||
                stats.stamina <= staminaToStopSprint)
            {
                sprintLocked = false;
            }
        }

        if (sprintLocked)
        {
            targetVelocity = transform.TransformDirection(input) * sprintSpeed;

            if (stats != null)
                stats.UseStamina(sprintStaminaCostPerSecond * Time.fixedDeltaTime);

            isSprinting = true;

            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                sprintFOV,
                sprintFOVStepTime * Time.fixedDeltaTime
            );
        }
        else
        {
            isSprinting = false;

            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                fov,
                sprintFOVStepTime * Time.fixedDeltaTime
            );
        }

        if (stats != null)
        {
            stats.isWalking = isWalking && !isSprinting;
            stats.isSprinting = isSprinting;
        }

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - velocity;
        velocityChange.y = 0;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void HandleSwimming()
    {
        rb.useGravity = true; // гравитация должна быть включена
        rb.angularDamping = 2f;

        float waterLevel = 10f;   // уровень воды
        float objectHeight = 2f;  // примерная высота капсулы

        float bottom = transform.position.y - objectHeight * 0.5f;
        float top = transform.position.y + objectHeight * 0.5f;

        float submerged = Mathf.Clamp01((waterLevel - bottom) / objectHeight);

        // Плавучесть (Архимед)
        Vector3 buoyancy = Vector3.up * Physics.gravity.magnitude * submerged * 1.2f;
        rb.AddForce(buoyancy, ForceMode.Acceleration);

        // Сопротивление воды (квадратичное)
        Vector3 drag = -rb.linearVelocity * rb.linearVelocity.magnitude * 0.5f;
        rb.AddForce(drag, ForceMode.Acceleration);

        // Управление — это просто добавление силы
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 camForward = playerCamera.transform.forward;
        Vector3 camRight = playerCamera.transform.right;

        Vector3 moveDir = camForward * v + camRight * h;

        rb.AddForce(moveDir * swimSpeed, ForceMode.Acceleration);
    }





    void Jump()
    {
        if (!isGrounded) return;
        if (stats != null && !stats.CanUseStamina(jumpStaminaCost)) return;

        if (stats != null)
            stats.UseStamina(jumpStaminaCost);

        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        isGrounded = false;
    }

    void UpdateSprintBar()
    {
        if (!useSprintBar || sprintBar == null || stats == null) return;

        float percent = stats.stamina / stats.maxStamina;
        sprintBar.fillAmount = percent;
    }

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.down * 0.5f;
        isGrounded = Physics.Raycast(origin, Vector3.down, 0.7f);
    }
}
