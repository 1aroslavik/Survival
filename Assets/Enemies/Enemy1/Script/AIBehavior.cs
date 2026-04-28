using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Animator animator;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4.5f;
    public float rotationSpeed = 8f;

    [Header("Detection")]
    public float detectionRadius = 10f;
    public float attackDistance = 2.5f;
    public float attackExitBuffer = 0.8f;
    public float losePlayerDistance = 18f;

    [Header("Patrol")]
    public Transform patrolCenter;
    public float patrolRadius = 8f;
    public float patrolWaitMin = 1.5f;
    public float patrolWaitMax = 4f;

    [Header("Combat")]
    public float attackCooldown = 1.5f;
    public float attackHitDelay = 0.4f;
    public float screamDuration = 2f;

    [Header("Damage")]
    public float damage = 15f;
    public float attackRadius = 2.8f;
    public float attackAngle = 90f;

    [Header("Health")]
    public float maxHealth = 80f;
    private float currentHealth;

    [Header("Animation")]
    [Tooltip("Сколько вариантов удара в Blend Tree (AttackIndex от 0 до N-1).")]
    public int attackVariantCount = 2;

    float attackTimer;
    float screamTimer;
    float patrolWaitTimer;
    float hitTimer;

    bool hasScreamed = false;
    bool isDead = false;
    bool attackQueued = false;

    public bool IsDead => isDead;

    Vector3 spawnPoint;
    State currentState;

    enum State { Patrol, Scream, Chase, Attack }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        spawnPoint = transform.position;
        currentHealth = maxHealth;
        TryAcquirePlayer();

        agent.stoppingDistance = 0.1f;
        agent.autoBraking = true;
        agent.speed = patrolSpeed;
        agent.updateRotation = true;

        if (!agent.isOnNavMesh)
            Debug.LogWarning($"[EnemyAI] {name} NOT on NavMesh", this);

        ChangeState(State.Patrol);
    }

    void Update()
    {
        if (isDead || agent == null || !agent.isOnNavMesh) return;

        if (player == null) TryAcquirePlayer();

        float distance = player != null
            ? Vector3.Distance(transform.position, player.position)
            : Mathf.Infinity;

        switch (currentState)
        {
            case State.Patrol: PatrolUpdate(distance); break;
            case State.Scream: ScreamUpdate(); break;
            case State.Chase:  ChaseUpdate(distance); break;
            case State.Attack: AttackUpdate(distance); break;
        }

        if (attackQueued)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0f)
            {
                attackQueued = false;
                DealDamage();
                animator.SetBool("IsAttacking", false);
            }
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        bool moving = false;
        bool running = false;
        bool rage = false;

        switch (currentState)
        {
            case State.Patrol:
                moving = agent != null && agent.velocity.sqrMagnitude > 0.01f;
                break;

            case State.Scream:
                rage = true;
                break;

            case State.Chase:
                moving = true;
                running = true;
                break;

            case State.Attack:
                moving = false;
                running = false;
                break;
        }

        animator.SetBool("IsMoving", moving);
        animator.SetBool("IsRunning", running);
        animator.SetBool("Rage", rage);
    }

    void TryAcquirePlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;
    }

    void PatrolUpdate(float distance)
    {
        if (player != null && distance < detectionRadius)
        {
            if (!hasScreamed)
            {
                hasScreamed = true;
                ChangeState(State.Scream);
            }
            else
            {
                ChangeState(State.Chase);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0f)
            {
                SetRandomDestination();
                patrolWaitTimer = Random.Range(patrolWaitMin, patrolWaitMax);
            }
        }
    }

    void ScreamUpdate()
    {
        RotateTowardsPlayer();
        screamTimer -= Time.deltaTime;

        if (screamTimer <= 0f)
            ChangeState(State.Chase);
    }

    void ChaseUpdate(float distance)
    {
        if (player == null || distance > losePlayerDistance)
        {
            hasScreamed = false;
            ChangeState(State.Patrol);
            return;
        }

        if (distance <= attackDistance)
        {
            ChangeState(State.Attack);
            return;
        }

        agent.SetDestination(player.position);
    }

    void AttackUpdate(float distance)
    {
        if (player == null)
        {
            ChangeState(State.Patrol);
            return;
        }

        agent.SetDestination(GetApproachPoint());
        RotateTowardsPlayer();

        if (distance > attackDistance + attackExitBuffer)
        {
            ChangeState(State.Chase);
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f && !attackQueued && distance <= attackDistance)
        {
            int variants = Mathf.Max(1, attackVariantCount);
            animator.SetInteger("AttackIndex", Random.Range(0, variants));
            animator.SetBool("IsAttacking", true);

            attackQueued = true;
            hitTimer = attackHitDelay;
            attackTimer = attackCooldown;
        }
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                agent.isStopped = false;
                agent.autoBraking = true;
                agent.speed = patrolSpeed;
                patrolWaitTimer = 0f;
                SetRandomDestination();
                break;

            case State.Scream:
                agent.isStopped = true;
                agent.ResetPath();
                screamTimer = screamDuration;
                animator.SetTrigger("Scream");
                break;

            case State.Chase:
                agent.isStopped = false;
                agent.autoBraking = false;
                agent.speed = chaseSpeed;
                if (player != null)
                    agent.SetDestination(player.position);
                break;

            case State.Attack:
                agent.isStopped = false;
                agent.autoBraking = true;
                agent.speed = chaseSpeed;
                if (player != null)
                    agent.SetDestination(GetApproachPoint());
                break;
        }
    }

    Vector3 GetApproachPoint()
    {
        Vector3 toEnemy = transform.position - player.position;
        toEnemy.y = 0f;

        if (toEnemy.sqrMagnitude < 0.0001f)
            toEnemy = -transform.forward;

        Vector3 target = player.position + toEnemy.normalized * attackDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            return hit.position;

        return target;
    }

    void SetRandomDestination()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 rnd = Random.insideUnitCircle.normalized * Random.Range(3f, patrolRadius);
            Vector3 candidate = transform.position + new Vector3(rnd.x, 0f, rnd.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();

                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    return;
                }
            }
        }

        Debug.LogWarning($"[{name}] не нашёл куда пойти");
    }

    void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }
    }

    void DealDamage()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.magnitude > attackRadius) return;
        if (Vector3.Angle(transform.forward, dir) > attackAngle * 0.5f) return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
            stats.TakeDamage(damage);
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;

        if (animator != null)
            animator.SetTrigger("Hit");

        if (currentHealth <= 0f)
            Die();
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
            animator.SetBool("Rage", false);
            animator.SetBool("IsDead", true);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.cyan;
        Vector3 c = patrolCenter != null
            ? patrolCenter.position
            : (Application.isPlaying ? spawnPoint : transform.position);

        Gizmos.DrawWireSphere(c, patrolRadius);
    }
}