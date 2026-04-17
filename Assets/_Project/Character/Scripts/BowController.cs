using UnityEngine;
using System.Collections;

public class BowController : MonoBehaviour
{
    public Animator animator;

    public GameObject arrowPrefab;
    public Transform arrowSpawn;

    public float shootForce = 40f;
    public float shootDelay = 0.2f; // момент вылета стрелы

    bool isAiming;
    bool isShooting;

    void Update()
    {
        // ПКМ — начать прицеливание
        if (Input.GetMouseButtonDown(1))
        {
            animator.SetBool("Aim", true);
            isAiming = true;
        }

        // ПКМ отпустить — убрать лук
        if (Input.GetMouseButtonUp(1))
        {
            animator.SetBool("Aim", false);
            isAiming = false;
        }

        // ЛКМ — выстрел
        if (Input.GetMouseButtonDown(0) && isAiming && !isShooting)
        {
            animator.SetTrigger("Shoot");
            StartCoroutine(ShootRoutine());
        }
    }

    IEnumerator ShootRoutine()
    {
        isShooting = true;

        // ждём момент отпускания тетивы
        yield return new WaitForSeconds(shootDelay);

        ShootArrow();

        isShooting = false;
    }

    void ShootArrow()
    {
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawn.position, arrowSpawn.rotation);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();

        // игнорировать коллайдеры игрока
        Collider arrowCollider = arrow.GetComponent<Collider>();

        foreach (Collider col in GetComponentsInParent<Collider>())
        {
            Physics.IgnoreCollision(arrowCollider, col);
        }

        // 🔥 правильный выстрел
        rb.linearVelocity = Camera.main.transform.forward * shootForce;
    }
}