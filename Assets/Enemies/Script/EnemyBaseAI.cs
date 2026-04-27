using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBaseAI : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Transform player;

    [Header("Stats")]
    public float maxHealth = 50f;
    protected float currentHealth;
    public float damage = 10f;

    [Header("Movement")]
    public float patrolRadius = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    [Header("Detection")]
    public float detectDistance = 15f;
    public float loseDistance = 25f;

    [Header("Attack")]
    public float attackDistance = 2f;
    public float attackCooldown = 2f;

    protected float lastAttackTime;
    protected Vector3 spawnPoint;

    protected enum State { Patrol, Chase, Attack, Flee }
    protected State state;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        spawnPoint = transform.position;
        currentHealth = maxHealth;

        SetState(State.Patrol);
    }

    protected virtual void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol: PatrolUpdate(dist); break;
            case State.Chase: ChaseUpdate(dist); break;
            case State.Attack: AttackUpdate(dist); break;
            case State.Flee: FleeUpdate(dist); break;
        }

        UpdateAnimation();
    }

    protected virtual void PatrolUpdate(float dist)
    {
        agent.speed = walkSpeed;

        if (!agent.hasPath || agent.remainingDistance < 1f)
            MoveRandom();

        if (dist < detectDistance)
            SetState(State.Chase);
    }

    protected virtual void ChaseUpdate(float dist)
    {
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (dist < attackDistance)
            SetState(State.Attack);

        if (dist > loseDistance)
            SetState(State.Patrol);
    }

    protected virtual void AttackUpdate(float dist)
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (dist > attackDistance)
        {
            SetState(State.Chase);
            return;
        }

        if (Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;

            if (HasParam("Attack"))
                animator.SetTrigger("Attack");

            DealDamage();
        }
    }

    protected virtual void FleeUpdate(float dist) { }

    protected virtual void DealDamage()
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.TakeDamage(damage);
    }

    public virtual void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (HasParam("Die"))
            animator.SetTrigger("Die");

        agent.isStopped = true;
        Destroy(gameObject, 2f);
    }

    protected void MoveRandom()
    {
        Vector2 rand = Random.insideUnitCircle * patrolRadius;
        Vector3 target = spawnPoint + new Vector3(rand.x, 0, rand.y);

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    protected void SetState(State newState)
    {
        state = newState;
    }

    protected void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;

        if (HasParam("Speed"))
            animator.SetFloat("Speed", speed);

        if (HasParam("IsMoving"))
            animator.SetBool("IsMoving", speed > 0.1f);
    }

    protected bool HasParam(string name)
    {
        foreach (var p in animator.parameters)
            if (p.name == name) return true;

        return false;
    }
}