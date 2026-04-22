using UnityEngine;
using UnityEngine.AI;

public class AnimalHealth : MonoBehaviour
{
    [Header("Health")]
    public float health = 50f;

    [Header("Attack")]
    public float damage = 10f;          // 👈 У КАЖДОГО ЖИВОТНОГО СВОЙ
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("Drop Settings")]
    public GameObject meatPrefab;
    public int meatCount = 1;

    public Animator animator;

    private bool isDead = false;
    private float lastAttackTime;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        TryAttackPlayer();
    }

    // ================= АТАКА =================

    void TryAttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player"))
                continue;

            PlayerStats player = hit.GetComponentInParent<PlayerStats>();

            if (player != null)
            {
                player.TakeDamage(damage);

                lastAttackTime = Time.time;

                if (animator != null)
                    animator.SetTrigger("Attack");

                break;
            }
        }
    }

    // ================= УРОН ЖИВОТНОМУ =================

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

    // ================= ДРОП =================

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

    // ================= DEBUG =================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}