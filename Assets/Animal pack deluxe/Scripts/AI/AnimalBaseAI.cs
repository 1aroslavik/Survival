using UnityEngine;
using UnityEngine.AI;

public abstract class AnimalBaseAI : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Transform player;

    public float detectDistance = 15f;
    public float roamRadius = 20f;

    protected Vector3 spawnPoint;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        spawnPoint = transform.position;

        // фикс если не на NavMesh
        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                transform.position = hit.position;
        }

        // чуть рандома, чтобы не были одинаковыми
        if (agent != null)
            agent.speed += Random.Range(-0.5f, 0.5f);
    }

    // 🔹 движение по зоне
    protected void MoveRandom()
    {
        Vector2 rand = Random.insideUnitCircle * roamRadius;
        Vector3 target = spawnPoint + new Vector3(rand.x, 0, rand.y);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // 🔹 безопасная работа с Animator
    protected bool HasParam(string name)
    {
        if (animator == null) return false;

        foreach (var p in animator.parameters)
        {
            if (p.name == name)
                return true;
        }
        return false;
    }

    protected void SetBool(string name, bool value)
    {
        if (HasParam(name))
            animator.SetBool(name, value);
    }

    protected void SetFloat(string name, float value)
    {
        if (HasParam(name))
            animator.SetFloat(name, value);
    }
    // 🔹 движение с контролем дистанции
    protected void MoveRandom(float minDistance, float maxDistance)
    {
        float distance = Random.Range(minDistance, maxDistance);

        Vector2 rand = Random.insideUnitCircle.normalized * distance;
        Vector3 target = transform.position + new Vector3(rand.x, 0, rand.y);

        // ограничение зоной (чтобы не убегал слишком далеко)
        if (Vector3.Distance(spawnPoint, target) > roamRadius)
        {
            target = spawnPoint + (target - spawnPoint).normalized * roamRadius;
        }

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    protected void UpdateAnimation()
    {
        if (agent != null)
            SetFloat("Speed", agent.velocity.magnitude);
    }
}