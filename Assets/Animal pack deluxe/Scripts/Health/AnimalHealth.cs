using UnityEngine;
using UnityEngine.AI;

public class AnimalHealth : MonoBehaviour
{
    public float health = 50f;
    public Animator animator;

    [Header("Drop Settings")]
    public GameObject meatPrefab;
    public int meatCount = 1; // 👈 У КАЖДОГО ЖИВОТНОГО СВОЁ

    private bool isDead = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        DropMeat();

        Destroy(gameObject, 5f);
    }

    void DropMeat()
    {
        if (meatPrefab == null) return;

        for (int i = 0; i < meatCount; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                1f,
                Random.Range(-0.5f, 0.5f)
            );

            GameObject meat = Instantiate(meatPrefab, spawnPos, Quaternion.identity);

            Rigidbody rb = meat.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
            }
        }
    }
}