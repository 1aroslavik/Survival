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
 
    [Header("Animation")]
    [Tooltip("Порог, выше которого врага считаем 'бегущим' (для bool IsMoving)")]
    public float movingThreshold = 0.15f;
 
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
 
        TryAcquirePlayer();
 
        agent.stoppingDistance = 0.1f;
        agent.autoBraking = true;
        agent.speed = patrolSpeed;
        agent.updateRotation = true;

        if (!agent.isOnNavMesh)
            Debug.LogWarning($"[EnemyAI] {name} NOT on NavMesh at {transform.position}. AgentTypeID={agent.agentTypeID}", this);

        ChangeState(State.Patrol);
    }
 
    void Update()
    {
        if (isDead || agent == null || !agent.isOnNavMesh) return;
 
        if (player == null) TryAcquirePlayer();
 
        float distance = player != null
            ? Vector3.Distance(transform.position, player.position)
            : Mathf.Infinity;
 
        // === ANIMATION DRIVER ===
        // Используем desiredVelocity (желаемая скорость пасфайндинга),
        // а не velocity (фактическая скорость) — она не проседает от
        // autoBraking'а и физических коллизий, поэтому не моргает.
        float normalizedSpeed = agent.desiredVelocity.magnitude / Mathf.Max(0.01f, agent.speed);
        animator.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
 
        // Дублируем булом — для переходов Idle <-> Run в аниматоре
        // это надёжнее, чем пороги на float'е.
        bool isMoving = !agent.isStopped && normalizedSpeed > movingThreshold;
        animator.SetBool("IsMoving", isMoving);
 
        switch (currentState)
        {
            case State.Patrol: PatrolUpdate(distance); break;
            case State.Scream: ScreamUpdate(); break;
            case State.Chase: ChaseUpdate(distance); break;
            case State.Attack: AttackUpdate(distance); break;
        }
 
        if (attackQueued)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0f)
            {
                attackQueued = false;
                DealDamage();
            }
        }
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
        if (screamTimer <= 0f) ChangeState(State.Chase);
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
 
        // В Chase целимся прямо в игрока, без approach point —
        // не даём агенту тормозить заранее. Переход в Attack по дистанции
        // сработает, когда подбежим достаточно близко.
        agent.SetDestination(player.position);
    }
 
    Vector3 GetApproachPoint()
    {
        // точка на расстоянии attackDistance от игрока в сторону врага
        Vector3 toEnemy = transform.position - player.position;
        toEnemy.y = 0f;
        if (toEnemy.sqrMagnitude < 0.0001f) toEnemy = -transform.forward;
        Vector3 target = player.position + toEnemy.normalized * attackDistance;
 
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            return hit.position;
        return target;
    }
 
    void AttackUpdate(float distance)
    {
        if (player == null)
        {
            ChangeState(State.Patrol);
            return;
        }
 
        // целимся в точку перед игроком, а не в самого игрока
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
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
            attackQueued = true;
            hitTimer = attackHitDelay;
            attackTimer = attackCooldown;
        }
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
        float dist = dir.magnitude;
        if (dist > attackRadius) return;
 
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > attackAngle * 0.5f) return;
 
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null) stats.TakeDamage(damage);
    }
 
    void ChangeState(State newState)
    {
        currentState = newState;
 
        switch (newState)
        {
            case State.Patrol:
                agent.isStopped = false;
                agent.autoBraking = true;      // на патруле тормозить норм — агент подходит к точке и встаёт
                agent.speed = patrolSpeed;
                patrolWaitTimer = 0f;
                SetRandomDestination();
                break;
 
            case State.Scream:
                agent.isStopped = true;
                agent.ResetPath();
                screamTimer = screamDuration;
                animator.ResetTrigger("Scream");
                animator.SetTrigger("Scream");
                break;
 
            case State.Chase:
                agent.isStopped = false;
                agent.autoBraking = false;     // <-- ключевой фикс: не тормозим во время погони
                agent.speed = chaseSpeed;
                if (player != null) agent.SetDestination(player.position);
                break;
 
            case State.Attack:
                agent.isStopped = false;
                agent.autoBraking = true;      // в атаке тормозим нормально, стоим на approach point
                agent.speed = chaseSpeed;
                if (player != null) agent.SetDestination(GetApproachPoint());
                // attackTimer НЕ сбрасываем — иначе кулдаун теряется при re-entry из Chase
                break;
        }
    }
 void SetRandomDestination()
{
    for (int i = 0; i < 10; i++)
    {
        Vector2 rnd = Random.insideUnitCircle.normalized * Random.Range(3f, patrolRadius);
        Vector3 candidate = transform.position + new Vector3(rnd.x, 0f, rnd.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            // проверяем что путь реально строится, а не в пустоту
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
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        animator.SetBool("IsMoving", false);
        animator.SetBool("Dead", true);
    }
 
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        Gizmos.color = Color.cyan;
        Vector3 c = patrolCenter != null ? patrolCenter.position : (Application.isPlaying ? spawnPoint : transform.position);
        Gizmos.DrawWireSphere(c, patrolRadius);
    }
}