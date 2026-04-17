using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class AnimalHealth : MonoBehaviour
{
    public float health = 50f;
    public Animator animator;

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
            Debug.Log("DIE TRIGGER"); // 👈 проверка
            animator.SetTrigger("Die");
        }

        Destroy(gameObject, 5f);
    }
}