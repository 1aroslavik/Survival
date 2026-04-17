using UnityEngine;
using System.Collections;

public class SpearController : MonoBehaviour
{
    public Animator animator;

    public GameObject spearPrefab;
    public Transform spearSpawn;

    public float throwForce = 30f;
    public float throwDelay = 0.25f;

    public float stabDistance = 2.5f;
    public float stabDamage = 30f;

    public Camera cam;

    bool isAiming;
    bool isThrowing;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        // ПКМ — прицеливание
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

        // 🗡️ УДАР КОПЬЁМ
        if (Input.GetMouseButtonDown(0) && !isAiming)
        {
            animator.SetTrigger("Stab");
            Invoke(nameof(DoStabHit), 0.15f); // момент попадания
        }

        // 🏹 БРОСОК
        if (Input.GetMouseButtonDown(0) && isAiming && !isThrowing)
        {
            animator.SetTrigger("Throw");
            StartCoroutine(ThrowRoutine());
        }
    }

    void DoStabHit()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, stabDistance))
        {
            AnimalHealth animal = hit.collider.GetComponentInParent<AnimalHealth>();

            if (animal != null)
            {
                animal.TakeDamage(stabDamage);
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
        GameObject spear = Instantiate(spearPrefab, spearSpawn.position, spearSpawn.rotation);

        Rigidbody rb = spear.GetComponent<Rigidbody>();

        Collider spearCollider = spear.GetComponent<Collider>();

        foreach (Collider col in GetComponentsInParent<Collider>())
        {
            Physics.IgnoreCollision(spearCollider, col);
        }

        rb.linearVelocity = spearSpawn.forward * throwForce;
    }
}