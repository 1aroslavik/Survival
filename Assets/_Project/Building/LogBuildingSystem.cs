using UnityEngine;

public class LogBuildingSystem : MonoBehaviour
{
    [Header("References")]
    public GameObject logPrefab;
    public Transform holdPoint;
    public Animator animator;
    public LineRenderer lineRenderer;

    [Header("Pickup")]
    public float pickupDistance = 3f;

    [Header("Placement")]
    public LayerMask placementMask; // Ground + BuiltLog
    public float placementDistance = 5f;
    public float scrollRotationSpeed = 180f;

    private GameObject worldLog;
    private GameObject carriedVisual;

    private bool isHolding = false;
    private bool verticalMode = false;

    private float currentRotationY = 0f;
    private Vector3 lastValidPosition;
    private bool canPlace = false;

    private float logHeight;
    private float logLength;

    void Start()
    {
        // Точный размер через BoxCollider
        BoxCollider box = logPrefab.GetComponent<BoxCollider>();
        logHeight = box.size.y * logPrefab.transform.localScale.y;
        logLength = box.size.z * logPrefab.transform.localScale.z;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isHolding)
                TryPickup();
            else
                CancelHolding();
        }

        if (!isHolding)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;

        HandleScrollRotation();
        UpdateIndicator();

        if (Input.GetKeyDown(KeyCode.R))
            verticalMode = !verticalMode;

        if (Input.GetMouseButtonDown(0) && canPlace)
            PlaceLog();
    }

    // ================= PICKUP =================

    void TryPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position,
                          Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance))
        {
            if (hit.collider.CompareTag("Log"))
            {
                worldLog = hit.collider.gameObject;

                carriedVisual = Instantiate(logPrefab, holdPoint);
                carriedVisual.transform.localPosition = Vector3.zero;
                carriedVisual.transform.localRotation = Quaternion.identity;

                Destroy(carriedVisual.GetComponent<Rigidbody>());
                Destroy(carriedVisual.GetComponent<Collider>());

                worldLog.SetActive(false);

                animator.SetBool("isHolding", true);
                isHolding = true;
            }
        }
    }

    void CancelHolding()
    {
        Destroy(carriedVisual);

        if (worldLog != null)
            worldLog.SetActive(true);

        animator.SetBool("isHolding", false);

        isHolding = false;
        worldLog = null;
    }

    // ================= PREVIEW =================

    void UpdateIndicator()
    {
        Ray ray = new Ray(Camera.main.transform.position,
                          Camera.main.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, placementDistance, placementMask))
        {
            canPlace = false;
            return;
        }

        BoxCollider box = logPrefab.GetComponent<BoxCollider>();
        float height = box.size.y * logPrefab.transform.localScale.y;
        float halfHeight = height / 2f;

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("BuiltLog"))
        {
            // 🔥 Берём РЕАЛЬНУЮ верхнюю точку коллайдера
            float topY = hit.collider.bounds.max.y;
            lastValidPosition = new Vector3(
                hit.point.x,
                topY + halfHeight,
                hit.point.z
            );
        }
        else
        {
            // На землю
            lastValidPosition = new Vector3(
                hit.point.x,
                hit.point.y + halfHeight,
                hit.point.z
            );
        }

        canPlace = true;
    }

    void HandleScrollRotation()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            currentRotationY += scroll * scrollRotationSpeed;
    }

    // ================= DRAW =================

    void DrawLine(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 24;
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 pos = new Vector3(x, 0, z);
            lineRenderer.SetPosition(i, center + pos);
        }
    }

    // ================= PLACE =================

    void PlaceLog()
    {
        Quaternion rotation;

        if (verticalMode)
            rotation = Quaternion.Euler(0f, currentRotationY, 0f);
        else
            rotation = Quaternion.Euler(90f, currentRotationY, 0f);

        GameObject placed = Instantiate(logPrefab,
                                        lastValidPosition,
                                        rotation);

        placed.layer = LayerMask.NameToLayer("BuiltLog");

        Rigidbody rb = placed.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Destroy(carriedVisual);
        Destroy(worldLog);

        animator.SetBool("isHolding", false);
        isHolding = false;
        worldLog = null;
    }
}