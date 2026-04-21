using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Зона спавна врагов. Поставьте несколько на сцену — каждая работает независимо.
/// На объекте должен быть Collider с IsTrigger=true, покрывающий "зону входа" игрока.
/// spawnRadius — радиус, в котором случайно размещаются враги внутри зоны.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class EnemySpawnZone : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    [Header("Trigger (зона входа игрока)")]
    [Tooltip("Авто-настройка SphereCollider на объекте (isTrigger + radius).")]
    public bool autoConfigureTrigger = true;
    [Tooltip("Радиус триггера, в котором игрок 'входит' в зону.")]
    public float triggerRadius = 40f;

    [Header("Spawn Settings")]
    [Tooltip("Сколько врагов создать при первом входе игрока.")]
    public int spawnCount = 3;
    [Tooltip("Радиус вокруг центра зоны, в котором размещаются враги.")]
    public float spawnRadius = 30f;
    public float spawnHeightOffset = 0f;
    public bool spawnOnGround = true;
    public LayerMask groundMask = ~0;
    [Tooltip("Спавнить врагов только на NavMesh (исключает воду и не-забейканные области).")]
    public bool requireNavMesh = true;
    [Tooltip("Маска областей NavMesh для спавна. По умолчанию AllAreas. Можно исключить area 'Water'.")]
    public int navMeshAreaMask = NavMesh.AllAreas;
    [Tooltip("Допуск при поиске ближайшей точки NavMesh.")]
    public float navMeshSampleDistance = 3f;
    [Tooltip("Сколько попыток найти валидную точку на одного врага.")]
    public int spawnAttempts = 15;

    [Header("Patrol")]
    [Tooltip("Если true — враги патрулируют внутри spawnRadius этой зоны (patrolCenter/Radius у врага перезаписываются).")]
    public bool overrideEnemyPatrol = true;
    [Tooltip("Радиус патрулирования. Если <=0 — используется spawnRadius.")]
    public float patrolRadius = 0f;

    [Header("Behavior")]
    public string playerTag = "Player";
    [Tooltip("Деактивировать живых врагов после выхода игрока (HP сохраняется).")]
    public bool despawnOnExit = true;
    public float despawnDelay = 2f;
    [Tooltip("Репозиционировать живых врагов внутри зоны при повторном входе игрока.")]
    public bool repositionOnReentry = true;
    [Tooltip("Если все враги мертвы — через это время зона возродит новых. <0 — никогда.")]
    public float respawnAllDeadCooldown = 60f;

    readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    bool playerInside = false;
    bool hasSpawned = false;

    public bool HasSpawned => hasSpawned;
    public bool IsCleared => hasSpawned && AllDead();
    float exitTimer = 0f;
    float allDeadTimer = -1f;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Awake()
    {
        if (!autoConfigureTrigger) return;
        var sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = triggerRadius;
        sphere.center = Vector3.zero;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;
        exitTimer = 0f;

        if (!hasSpawned)
        {
            SpawnFresh();
        }
        else
        {
            ReactivateSurvivors();
            MaybeRespawnIfAllDead();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
    }

    void Update()
    {
        if (!playerInside && hasSpawned && despawnOnExit)
        {
            exitTimer += Time.deltaTime;
            if (exitTimer >= despawnDelay)
            {
                DeactivateSurvivors();
                exitTimer = 0f;
            }
        }

        // Таймер полного респавна запускается, когда все умерли.
        if (hasSpawned && respawnAllDeadCooldown >= 0f && AllDead())
        {
            if (allDeadTimer < 0f) allDeadTimer = respawnAllDeadCooldown;
            allDeadTimer -= Time.deltaTime;
            if (allDeadTimer <= 0f)
            {
                ClearAll();
                if (playerInside) SpawnFresh();
                allDeadTimer = -1f;
            }
        }
        else
        {
            allDeadTimer = -1f;
        }
    }

    void SpawnFresh()
    {
        ClearAll();

        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

        for (int i = 0; i < spawnCount; i++)
        {
            if (!TryGetSpawnPosition(out Vector3 pos)) continue;
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy = Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            ConfigureEnemy(enemy);
            spawnedEnemies.Add(enemy);
        }

        hasSpawned = true;
    }

    void ConfigureEnemy(GameObject enemy)
    {
        if (!overrideEnemyPatrol) return;
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai == null) return;
        ai.patrolCenter = transform;
        ai.patrolRadius = patrolRadius > 0f ? patrolRadius : spawnRadius;
    }

    void ReactivateSurvivors()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            var e = spawnedEnemies[i];
            if (e == null) { spawnedEnemies.RemoveAt(i); continue; }

            var ai = e.GetComponent<EnemyAI>();
            if (ai != null && ai.IsDead) continue;

            if (repositionOnReentry && TryGetSpawnPosition(out Vector3 pos))
            {
                var agent = e.GetComponent<NavMeshAgent>();
                if (agent != null && agent.enabled) agent.Warp(pos);
                else e.transform.position = pos;
            }

            if (!e.activeSelf) e.SetActive(true);
        }
    }

    void DeactivateSurvivors()
    {
        foreach (var e in spawnedEnemies)
        {
            if (e == null) continue;
            var ai = e.GetComponent<EnemyAI>();
            if (ai != null && ai.IsDead) continue;
            if (e.activeSelf) e.SetActive(false);
        }
    }

    void MaybeRespawnIfAllDead()
    {
        if (AllDead() && respawnAllDeadCooldown < 0f)
        {
            // Полностью заблокировано — ничего не делаем.
            return;
        }
        // Если все мертвы, таймер обработает в Update. Здесь только первоначальное
        // ре-активирование уже сделано выше; живым — хватит.
    }

    bool AllDead()
    {
        int alive = 0;
        foreach (var e in spawnedEnemies)
        {
            if (e == null) continue;
            var ai = e.GetComponent<EnemyAI>();
            if (ai == null || !ai.IsDead) alive++;
        }
        return alive == 0;
    }

    bool TryGetSpawnPosition(out Vector3 result)
    {
        for (int attempt = 0; attempt < Mathf.Max(1, spawnAttempts); attempt++)
        {
            Vector2 circle = Random.insideUnitCircle * spawnRadius;
            Vector3 pos = transform.position + new Vector3(circle.x, 0f, circle.y);

            if (spawnOnGround)
            {
                Vector3 rayOrigin = pos + Vector3.up * 50f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Ignore))
                    pos.y = hit.point.y;
            }

            pos.y += spawnHeightOffset;

            if (requireNavMesh)
            {
                if (NavMesh.SamplePosition(pos, out NavMeshHit nmHit, navMeshSampleDistance, navMeshAreaMask))
                {
                    result = nmHit.position;
                    return true;
                }
                continue;
            }

            result = pos;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    void ClearAll()
    {
        foreach (var e in spawnedEnemies)
        {
            if (e != null) Destroy(e);
        }
        spawnedEnemies.Clear();
        hasSpawned = false;
        exitTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.25f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        float pr = patrolRadius > 0f ? patrolRadius : spawnRadius;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pr);

        var col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box)
                Gizmos.DrawCube(box.center, box.size);
            else if (col is SphereCollider sp)
                Gizmos.DrawSphere(sp.center, sp.radius);
        }
    }
}
