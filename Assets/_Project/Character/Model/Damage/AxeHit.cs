using UnityEngine;

public class AxeHit : MonoBehaviour
{
    [Header("Damage")]
    public float animalDamage = 25f;

    [Header("References")]
    public TreeReplacer terrainChopper;
    public Camera playerCamera;

    [Header("Hit Settings")]
    public float hitDistance = 4f;
    public float sphereRadius = 0.5f; // 🔥 для стабильного попадания

    bool hasHit = false;

    void Start()
    {
        if (terrainChopper == null)
            terrainChopper = FindObjectOfType<TreeReplacer>();

        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            hasHit = false;
            Invoke(nameof(DoHit), 0.1f);
        }
    }

    void DoHit()
    {
        if (hasHit) return;
        hasHit = true;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        Debug.DrawRay(ray.origin, ray.direction * hitDistance, Color.red, 2f);

        RaycastHit hit;

        // 🔥 1. Обычный Raycast
        bool hasRayHit = Physics.Raycast(ray, out hit, hitDistance);

        // 🔥 2. Если не попал — SphereCast (как в играх)
        if (!hasRayHit)
        {
            if (Physics.SphereCast(ray, sphereRadius, out hit, hitDistance))
            {
                Debug.Log("[AxeHit] SphereCast hit");
            }
            else
            {
                Debug.Log("[AxeHit] Nothing hit");
                return;
            }
        }

        Debug.Log($"[AxeHit] Hit: {hit.collider.name}");

        Vector3 hitPoint = hit.point;

        // ❌ Игнор игрока
        if (hit.collider.GetComponentInParent<FirstPersonController>() != null)
        {
            Debug.Log("[AxeHit] Hit player → ignored");
            return;
        }

        // 🐇 Животное
        AnimalHealth animal = hit.collider.GetComponentInParent<AnimalHealth>();
        if (animal != null)
        {
            animal.TakeDamage(animalDamage);
            return;
        }

        // 🌲 PREFAB дерево
        TreeHealth tree = hit.collider.GetComponentInParent<TreeHealth>();

        if (tree == null && hit.rigidbody != null)
            tree = hit.rigidbody.GetComponentInParent<TreeHealth>();

        if (tree != null)
        {
            tree.Hit(hitPoint);
            return;
        }

        // 🌳 TERRAIN дерево
        if (terrainChopper != null)
        {
            GameObject obj = terrainChopper.TryChopClosestTree(hitPoint);

            if (obj != null)
            {
                Debug.Log("[AxeHit] Terrain tree replaced");

                TreeHealth h = obj.GetComponent<TreeHealth>();

                if (h == null)
                    h = obj.GetComponentInChildren<TreeHealth>();

                if (h != null)
                {
                    h.Hit(hitPoint);
                    return;
                }
            }
        }

        // 🔥 Fallback — если попали рядом
        Collider[] cols = Physics.OverlapSphere(hitPoint, 1.5f);

        foreach (var c in cols)
        {
            TreeHealth t = c.GetComponentInParent<TreeHealth>();
            if (t != null)
            {
                Debug.Log("[AxeHit] Fallback hit");
                t.Hit(hitPoint);
                return;
            }
        }
    }
}