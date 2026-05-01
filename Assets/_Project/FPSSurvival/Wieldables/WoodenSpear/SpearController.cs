using UnityEngine;
using System.Collections;

public class SpearController : MonoBehaviour
{
    public Animator animator;

    public GameObject spearPrefab;
    public Transform spearSpawn;

    public GameObject spearVisual;

    public float throwForce = 30f;
    public float throwDelay = 0.25f;

    public float stabDistance = 2.5f;
    public float stabDamage = 30f;

    public Camera cam;

    bool isAiming;
    bool isThrowing;

    public InventoryModel inventory;
    public ResourceType spearType;

    void Awake()
    {
        AutoFindInventory();
        AutoFindCamera();

        if (spearVisual != null)
            spearVisual.SetActive(false);
        else
            Debug.LogError("❌ spearVisual НЕ назначен!");
    }

    void Start()
    {
        ForceUpdateSpearVisual();
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

        // 🗡️ УДАР
        if (Input.GetMouseButtonDown(0) && !isAiming)
        {
            animator.SetTrigger("Stab");
            Invoke(nameof(DoStabHit), 0.15f);
        }

        // 🏹 БРОСОК
        if (Input.GetMouseButtonDown(0) && isAiming && !isThrowing)
        {
            if (!HasSpears())
                return;

            animator.SetTrigger("Throw");
            StartCoroutine(ThrowRoutine());
        }
    }

    void LateUpdate()
    {
        ForceUpdateSpearVisual();
    }

    // =========================
    // 🔍 AUTO FIND
    // =========================

    void AutoFindInventory()
    {
        if (inventory != null) return;

        inventory = FindObjectOfType<InventoryModel>();

        if (inventory == null)
        {
            inventory = GetComponentInParent<InventoryModel>();
        }

        if (inventory == null)
            Debug.LogError("❌ InventoryModel НЕ найден в сцене!");
        else
            Debug.Log("✅ Inventory найден: " + inventory.name);
    }

    void AutoFindCamera()
    {
        if (cam != null) return;

        cam = Camera.main;

        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (cam == null)
            Debug.LogError("❌ Камера НЕ найдена!");
        else
            Debug.Log("✅ Камера: " + cam.name);
    }

    // =========================

    void DoStabHit()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, stabDistance))
        {
            AnimalHealth animal = hit.collider.GetComponentInParent<AnimalHealth>();

            if (animal != null)
            {
                animal.TakeDamage(stabDamage);
                return;
            }

            EnemyBaseAI enemy = hit.collider.GetComponentInParent<EnemyBaseAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(stabDamage);
            }
        }
    }

    IEnumerator ThrowRoutine()
    {
        isThrowing = true;

        yield return new WaitForSeconds(throwDelay);

        ThrowSpear();

        isThrowing = false;
    }

    void ThrowSpear()
    {
        if (inventory == null)
        {
            Debug.LogError("❌ Inventory NULL");
            return;
        }

        if (!inventory.TryRemoveOne(spearType))
        {
            Debug.Log("❌ Нет копья для удаления");
            return;
        }

        GameObject spear = Instantiate(spearPrefab, spearSpawn.position, spearSpawn.rotation);

        Rigidbody rb = spear.GetComponent<Rigidbody>();
        Collider spearCollider = spear.GetComponent<Collider>();

        if (rb == null)
        {
            Debug.LogError("❌ У копья нет Rigidbody!");
            return;
        }

        foreach (Collider col in GetComponentsInParent<Collider>())
        {
            Physics.IgnoreCollision(spearCollider, col);
        }

        rb.linearVelocity = spearSpawn.forward * throwForce;
    }

    bool HasSpears()
    {
        return inventory != null && inventory.HasItem(spearType);
    }

    void ForceUpdateSpearVisual()
    {
        if (spearVisual == null || inventory == null)
            return;

        bool hasSpear = inventory.HasItem(spearType);

        spearVisual.SetActive(hasSpear);

        // 💀 УДАЛЯЕМ РУКИ ЕСЛИ НЕТ КОПЬЯ
        if (!hasSpear)
        {
            Destroy(gameObject);
        }
    }
}