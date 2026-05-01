using UnityEngine;
using UnityEngine.AI;

public class EnemyBaseAI : MonoBehaviour
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
    [Tooltip("Длительность анимации атаки — пока IsAttacking=true. Поставь равным длине самого длинного клипа в AttackTree.")]
    public float attackDuration = 0.8f;
    [Tooltip("Через сколько секунд после начала анимации наносится урон.")]
    public float attackHitDelay = 0.35f;

    [Tooltip("Сколько вариантов удара (AttackIndex от 0 до N-1).")]
    public int attackVariantCount = 2;

    [Tooltip("Сколько вариантов смерти (DeathIndex от 0 до N-1).")]
    public int deathVariantCount = 2;

    [Header("Rage")]
    [Tooltip("Длительность стейта rage перед бегом (если в аниматоре есть параметр Rage).")]
    public float rageDuration = 1.2f;

    protected float lastAttackTime;
    protected float attackEndTime;
    protected float hitTime;
    protected bool isAttacking;
    protected bool damageDealt;
    protected float rageEndTime;
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
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (dist < attackDistance)
            SetState(State.Attack);

        if (dist > loseDistance)
            SetState(State.Patrol);
    }

    protected virtual void AttackUpdate(float dist)
    {
        agent.isStopped = true;
        agent.ResetPath();

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 8f * Time.deltaTime);
        }

        if (isAttacking)
        {
            if (!damageDealt && Time.time >= hitTime)
            {
                damageDealt = true;
                DealDamage();
            }

            if (Time.time >= attackEndTime)
                isAttacking = false;
        }

        if (dist > attackDistance + 0.5f)
        {
            isAttacking = false;
            agent.isStopped = false;
            SetState(State.Chase);
            return;
        }

        if (!isAttacking && Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;
            attackEndTime = Time.time + attackDuration;
            hitTime = Time.time + attackHitDelay;
            isAttacking = true;
            damageDealt = false;

            if (animator != null)
            {
                if (HasParam("AttackIndex"))
                    animator.SetInteger("AttackIndex", Random.Range(0, Mathf.Max(1, attackVariantCount)));

                if (HasParam("Attack"))
                    animator.SetTrigger("Attack");
                else if (HasParam("IsAttacking"))
                    animator.SetBool("IsAttacking", true);
            }
        }
    }

    protected virtual void FleeUpdate(float dist) { }

    protected virtual void DealDamage()
    {
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) stats = player.GetComponentInParent<PlayerStats>();
        if (stats == null) stats = player.GetComponentInChildren<PlayerStats>();

        if (stats != null)
            stats.TakeDamage(damage);
    }

    public virtual void TakeDamage(float dmg)
    {
        currentHealth -= dmg;

        if (animator != null && HasParam("Hit"))
            animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (animator != null)
        {
            if (HasParam("IsMoving")) animator.SetBool("IsMoving", false);
            if (HasParam("IsRunning")) animator.SetBool("IsRunning", false);
            if (HasParam("IsAttacking")) animator.SetBool("IsAttacking", false);
            if (HasParam("Rage")) animator.SetBool("Rage", false);

            if (HasParam("DeathIndex"))
                animator.SetInteger("DeathIndex", Random.Range(0, Mathf.Max(1, deathVariantCount)));

            if (HasParam("IsDead"))
                animator.SetBool("IsDead", true);
            else if (HasParam("Die"))
                animator.SetTrigger("Die");
        }

        if (agent != null) agent.isStopped = true;
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
        State old = state;
        state = newState;

        if (old != State.Chase && newState == State.Chase)
        {
            rageEndTime = Time.time + rageDuration;

            if (animator != null && HasParam("Rage"))
                animator.SetBool("Rage", true);
        }
    }

    protected void UpdateAnimation()
    {
        if (agent == null || animator == null) return;

        float speed = agent.velocity.magnitude;
        bool moving = speed > 0.1f;
        bool running = state == State.Chase;

        if (HasParam("Speed"))
            animator.SetFloat("Speed", speed);

        if (HasParam("IsMoving"))
            animator.SetBool("IsMoving", moving);

        if (HasParam("IsRunning"))
            animator.SetBool("IsRunning", running);

        if (HasParam("Rage") && Time.time >= rageEndTime)
            animator.SetBool("Rage", false);
    }

    protected bool HasParam(string name)
    {
        foreach (var p in animator.parameters)
            if (p.name == name) return true;

        return false;
    }
}