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

    [Header("Swim Sound")]
    public AudioClip swimClip;
    public AudioSource swimSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (swimSource == null)
            swimSource = gameObject.AddComponent<AudioSource>();

        swimSource.clip = swimClip;
        swimSource.loop = true;            // 🔥 теперь зацикленный звук
        swimSource.playOnAwake = false;
        swimSource.spatialBlend = 0f;
        swimSource.volume = 1f;
    }

    void FixedUpdate()
    {
        if (!inWater)
        {
            StopSwimSound();
            return;
        }

        float move = Input.GetAxis("Vertical");
        float strafe = Input.GetAxis("Horizontal");

        Vector3 moveDirection =
            cameraTransform.forward * move +
            cameraTransform.right * strafe;

        rb.AddForce(moveDirection * swimForce, ForceMode.Acceleration);

        float depth = waterLevel - transform.position.y;

        if (depth > 0)
        {
            rb.AddForce(Vector3.up * buoyancyForce * depth, ForceMode.Acceleration);
        }

        bool isMoving = Mathf.Abs(move) > 0.01f || Mathf.Abs(strafe) > 0.01f;

        // 🔥 управление звуком
        if (isMoving)
        {
            if (!swimSource.isPlaying && swimClip != null)
                swimSource.Play();
        }
        else
        {
            StopSwimSound();
        }
    }

    void StopSwimSound()
    {
        if (swimSource.isPlaying)
            swimSource.Stop();
    }

    public bool IsInWater()
    {
        return inWater;
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

            StopSwimSound(); // 🔥 теперь точно выключается

            if (armsRoot) armsRoot.SetActive(true);
        }
    }
}