using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform player;
    public DayNightCycle dayNight;

    public int maxEnemies = 6;
    public float spawnRadius = 30f;
    public float minDistance = 10f;
    public float spawnDelay = 4f;

    [Header("Night threshold")]
    [Tooltip("Порог по Dot(sun.forward, Vector3.down). Чем выше — тем раньше начнётся ночь.")]
    public float nightThreshold = 0.1f;

    [Header("Debug")]
    public bool debugLogs = true;

    readonly List<GameObject> spawned = new List<GameObject>();
    float timer;
    bool wasNight;

    void Update()
    {
        if (player == null || dayNight == null) return;

        spawned.RemoveAll(e => e == null);

        bool night = IsNight();

        if (night != wasNight)
        {
            wasNight = night;
            if (debugLogs)
                Debug.Log(night
                    ? $"[EnemySpawner] Наступила ночь (timeOfDay={dayNight.timeOfDay:F2}). Начинаю спавн."
                    : $"[EnemySpawner] Наступил день (timeOfDay={dayNight.timeOfDay:F2}). Уничтожаю врагов: {spawned.Count}.");
        }

        if (night)
        {
            timer += Time.deltaTime;

            if (timer >= spawnDelay && spawned.Count < maxEnemies)
            {
                timer = 0f;
                Spawn();
            }
        }
        else
        {
            timer = 0f;

            if (spawned.Count > 0)
                DespawnAll();
        }
    }

    bool IsNight()
    {
        if (dayNight.sun == null) return false;

        float sunDot = Vector3.Dot(dayNight.sun.transform.forward, Vector3.down);
        return sunDot < nightThreshold;
    }

    void Spawn()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 rand = Random.insideUnitCircle.normalized * Random.Range(minDistance, spawnRadius);
            Vector3 pos = player.position + new Vector3(rand.x, 0, rand.y);

            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                GameObject enemy = Instantiate(prefab, hit.position, Quaternion.identity);
                spawned.Add(enemy);

                if (debugLogs)
                    Debug.Log($"[EnemySpawner] Spawned {prefab.name} at {hit.position}. Всего врагов: {spawned.Count}/{maxEnemies}");

                return;
            }
        }
    }

    void DespawnAll()
    {
        foreach (var e in spawned)
            if (e != null) Destroy(e);

        spawned.Clear();
    }
}
