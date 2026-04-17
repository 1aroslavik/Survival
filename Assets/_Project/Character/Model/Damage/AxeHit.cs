using UnityEngine;

public class AxeHit : MonoBehaviour
{
    public float animalDamage = 25f;
    public TreeReplacer terrainChopper;
    public GameObject woodChipsEffect;

    bool isSwinging = false;
    bool hasHit = false;

    void Start()
    {
        if (terrainChopper == null)
            terrainChopper = FindObjectOfType<TreeReplacer>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isSwinging = true;
            hasHit = false;

            // через 0.3 сек выключаем удар
            Invoke(nameof(StopSwing), 0.3f);
        }
    }

    void StopSwing()
    {
        isSwinging = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!isSwinging || hasHit) return;

        hasHit = true;

        Vector3 hitPoint = other.ClosestPoint(transform.position);

        SpawnWoodEffect(hitPoint, other);

        // 🐇 ЖИВОТНОЕ
        AnimalHealth animal = other.GetComponentInParent<AnimalHealth>();
        if (animal != null)
        {
            animal.TakeDamage(animalDamage);
            return;
        }

        // 🌲 ГОТОВОЕ ДЕРЕВО
        TreeHealth tree = other.GetComponentInParent<TreeHealth>();
        if (tree != null)
        {
            tree.Hit(transform.position);
            return;
        }

        // 🌳 TERRAIN ДЕРЕВО
        if (terrainChopper != null)
        {
            GameObject obj = terrainChopper.TryChopAndSpawn(hitPoint);

            if (obj != null)
            {
                TreeHealth h = obj.GetComponentInChildren<TreeHealth>();

                if (h != null)
                    h.Hit(transform.position);
            }
        }
    }

    void SpawnWoodEffect(Vector3 point, Collider col)
    {
        if (woodChipsEffect == null) return;

        Vector3 normal = (point - col.bounds.center).normalized;

        Quaternion rot = Quaternion.LookRotation(normal) *
                         Quaternion.Euler(0, Random.Range(0, 360), 0);

        GameObject fx = Instantiate(woodChipsEffect, point, rot);
        Destroy(fx, 2f);
    }
}