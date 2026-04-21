using UnityEngine;
using System.Collections;

public class BowController : MonoBehaviour
{
    public Animator animator;

    public GameObject arrowPrefab;
    public Transform arrowSpawn;

    public GameObject arrowVisual;

    public float shootForce = 40f;
    public float shootDelay = 0.2f;

    bool isAiming;
    bool isShooting;

    public InventoryModel inventory;
    public ResourceType arrowType;

    void Awake()
    {
        // 🔍 авто-поиск инвентаря
        if (inventory == null)
        {
            inventory = FindObjectOfType<InventoryModel>();
        }

        // ❗ сразу скрываем стрелу (чтобы не было 1 кадра)
        if (arrowVisual != null)
        {
            arrowVisual.SetActive(false);
        }
    }

    void Start()
    {
        // на старте сразу проверяем
        ForceUpdateArrowVisual();
    }

    void Update()
    {
        // ПКМ — прицел
        if (Input.GetMouseButtonDown(1))
        {
            animator.SetBool("Aim", true);
            isAiming = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            animator.SetBool("Aim", false);
            isAiming = false;
        }

        // ЛКМ — выстрел ТОЛЬКО если есть стрелы
        if (Input.GetMouseButtonDown(0) && isAiming && !isShooting && HasArrows())
        {
            animator.SetTrigger("Shoot");
            StartCoroutine(ShootRoutine());
        }
    }

    void LateUpdate()
    {
        // 💣 ГЛАВНЫЙ ФИКС: перебиваем Animator КАЖДЫЙ КАДР
        ForceUpdateArrowVisual();
    }

    IEnumerator ShootRoutine()
    {
        isShooting = true;

        yield return new WaitForSeconds(shootDelay);

        ShootArrow();

        isShooting = false;
    }

    void ShootArrow()
    {
        // защита
        if (!inventory.TryRemoveOne(arrowType))
            return;

        GameObject arrow = Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        Collider arrowCollider = arrow.GetComponent<Collider>();

        foreach (Collider col in GetComponentsInParent<Collider>())
        {
            Physics.IgnoreCollision(arrowCollider, col);
        }

        rb.linearVelocity = Camera.main.transform.forward * shootForce;
    }

    bool HasArrows()
    {
        return inventory != null && inventory.HasItem(arrowType);
    }

    void ForceUpdateArrowVisual()
    {
        if (arrowVisual == null || inventory == null)
            return;

        bool hasArrows = inventory.HasItem(arrowType);

        // 💣 всегда принудительно ставим состояние
        arrowVisual.SetActive(hasArrows);
    }
}