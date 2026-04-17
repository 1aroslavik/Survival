using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerSwimmingSystem : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Movement")]
    public float swimForce = 20f;
    public float waterDrag = 4f;

    [Header("Buoyancy")]
    public float buoyancyForce = 15f;
    public float waterLevel = 10f;

    private Rigidbody rb;
    private bool inWater = false;

    [Header("Arms")]
    public GameObject armsRoot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!inWater) return;

        // === ДВИЖЕНИЕ В НАПРАВЛЕНИИ КАМЕРЫ ===
        float move = Input.GetAxis("Vertical");
        float strafe = Input.GetAxis("Horizontal");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        Vector3 moveDirection = camForward * move + camRight * strafe;

        rb.AddForce(moveDirection * swimForce, ForceMode.Acceleration);

        // === ПЛАВУЧЕСТЬ ===
        float depth = waterLevel - transform.position.y;

        if (depth > 0)
        {
            rb.AddForce(Vector3.up * buoyancyForce * depth, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SwimZone"))
        {
            inWater = true;

            rb.useGravity = false;
            rb.linearDamping = waterDrag;

            if (armsRoot) armsRoot.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SwimZone"))
        {
            inWater = false;

            rb.useGravity = true;
            rb.linearDamping = 0f;

            if (armsRoot) armsRoot.SetActive(true);
        }
    }
}