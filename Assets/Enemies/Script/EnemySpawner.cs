using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public DayNightCycle dayNight;
    public List<EnemyZone> zones = new List<EnemyZone>();

    public int maxEnemies = 15;
    public float minDistance = 15f;
    public float despawnDistance = 80f;
    public float checkInterval = 2f;

    [Header("Night threshold")]
    [Tooltip("Порог по Dot(sun.forward, Vector3.down). Чем выше — тем раньше начнётся ночь.")]
    public float nightThreshold = 0.1f;

    [Header("Debug")]
    public bool debugLogs = true;

    private float timer;
    private List<GameObject> enemies = new List<GameObject>();
    private bool wasNight;

    void Update()
    {
        if (player == null || dayNight == null) return;

        bool night = IsNight();

        if (night != wasNight)
        {
            wasNight = night;

            if (debugLogs)
                Debug.Log(night
                    ? $"[EnemySpawner] Наступила ночь (timeOfDay={dayNight.timeOfDay:F2}). Начинаю спавн."
                    : $"[EnemySpawner] Наступил день (timeOfDay={dayNight.timeOfDay:F2}). Уничтожаю врагов: {enemies.Count}.");
        }

        if (!night)
        {
            if (enemies.Count > 0)
                DespawnAll();

            return;
        }

        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;

            Cleanup();
            TrySpawn();
        }
    }

    bool IsNight()
    {
        if (dayNight.sun == null) return false;

        float sunDot = Vector3.Dot(dayNight.sun.transform.forward, Vector3.down);
        return sunDot < nightThreshold;
    }

    void TrySpawn()
    {
        enemies.RemoveAll(e => e == null);

        if (enemies.Count >= maxEnemies)
        {
            if (debugLogs) Debug.Log($"[EnemySpawner] Лимит maxEnemies={maxEnemies} достигнут.");
            return;
        }

        if (zones.Count == 0)
        {
            if (debugLogs) Debug.LogWarning("[EnemySpawner] Список Zones пуст!");
            return;
        }

        EnemyZone zone = GetNearestZone();
        if (zone == null)
        {
            if (debugLogs) Debug.LogWarning("[EnemySpawner] Не нашлась ни одна зона (все null?).");
            return;
        }

        if (zone.enemies == null || zone.enemies.Length == 0)
        {
            if (debugLogs) Debug.LogWarning($"[EnemySpawner] У зоны '{zone.name}' пустой массив Enemies.");
            return;
        }

        foreach (var type in zone.enemies)
        {
            if (type.prefab == null)
            {
                if (debugLogs) Debug.LogWarning($"[EnemySpawner] У типа '{type.name}' в зоне '{zone.name}' не указан Prefab.");
                continue;
            }

            if (CountEnemies(type.name) >= type.maxCount)
            {
                if (debugLogs) Debug.Log($"[EnemySpawner] '{type.name}' уже на максимуме ({type.maxCount}), пропускаю.");
                continue;
            }

            Vector3 pos = GetLandPosition(zone);
            if (pos == Vector3.zero)
            {
                if (debugLogs) Debug.LogWarning($"[EnemySpawner] Не нашёл валидную позицию в зоне '{zone.name}'. Проверь NavMesh и положение зоны над землёй.");
                continue;
            }

            GameObject obj = Instantiate(type.prefab, pos, Quaternion.identity);

            SetupEnemy(obj, type, pos);
            enemies.Add(obj);

            if (debugLogs)
                Debug.Log($"[EnemySpawner] Spawned {type.name} в зоне '{zone.name}'. Всего: {enemies.Count}/{maxEnemies}");

            break;
        }
    }

    EnemyZone GetNearestZone()
    {
        EnemyZone best = null;
        float minDist = Mathf.Infinity;

        foreach (var z in zones)
        {
            if (z == null) continue;

            float dist = Vector3.Distance(player.position, z.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                best = z;
            }
        }

        return best;
    }

    int CountEnemies(string typeName)
    {
        int count = 0;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            if (e.name.Contains(typeName))
                count++;
        }

        return count;
    }

    Vector3 GetLandPosition(EnemyZone zone)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 random = Random.insideUnitSphere * zone.radius;
            random.y = 0;

            Vector3 pos = zone.transform.position + random;

            if (Vector3.Distance(player.position, pos) < minDistance)
                continue;

            if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
                {
                    return navHit.position;
                }
            }
        }

        return Vector3.zero;
    }

    void SetupEnemy(GameObject obj, EnemyType type, Vector3 spawnPos)
    {
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = type.walkSpeed;

            if (!agent.isOnNavMesh)
                agent.Warp(spawnPos);
        }

        EnemyBaseAI baseAI = obj.GetComponent<EnemyBaseAI>();
        if (baseAI != null)
        {
            baseAI.walkSpeed = type.walkSpeed;
            baseAI.runSpeed = type.runSpeed;
            baseAI.patrolRadius = type.patrolRadius;
            baseAI.detectDistance = type.detectionRadius;
        }

        EnemyAI ai = obj.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.patrolSpeed = type.walkSpeed;
            ai.chaseSpeed = type.runSpeed;
            ai.patrolRadius = type.patrolRadius;
            ai.detectionRadius = type.detectionRadius;
        }
    }

    void Cleanup()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.position, enemies[i].transform.position);

            if (dist > despawnDistance)
            {
                Destroy(enemies[i]);
                enemies.RemoveAt(i);
            }
        }
    }

    void DespawnAll()
    {
        foreach (var e in enemies)
            if (e != null) Destroy(e);

        enemies.Clear();
    }
}
