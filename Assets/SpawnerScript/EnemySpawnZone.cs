using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemySpawnZone : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    [Header("Spawn Settings")]
    public int spawnCount = 3;
    public float spawnRadius = 15f;
    public float spawnHeightOffset = 0f;
    public bool spawnOnGround = true;
    public LayerMask groundMask = ~0;

    [Header("Behavior")]
    public string playerTag = "Player";
    public bool despawnOnExit = true;
    public float despawnDelay = 2f;
    public bool respawnOnReentry = true;
    public float respawnCooldown = 30f;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool playerInside = false;
    private bool hasSpawned = false;
    private float exitTimer = 0f;
    private float lastSpawnTime = -Mathf.Infinity;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;
        exitTimer = 0f;

        bool cooldownOk = Time.time - lastSpawnTime >= respawnCooldown;
        if (!hasSpawned || (respawnOnReentry && cooldownOk))
        {
            SpawnEnemies();
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
                DespawnEnemies();
            }
        }
    }

    void SpawnEnemies()
    {
        DespawnEnemies();

        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = GetSpawnPosition();
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            GameObject enemy = Instantiate(prefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            spawnedEnemies.Add(enemy);
        }

        hasSpawned = true;
        lastSpawnTime = Time.time;
    }

    Vector3 GetSpawnPosition()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(circle.x, 0f, circle.y);

        if (spawnOnGround)
        {
            Vector3 rayOrigin = pos + Vector3.up * 50f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f, groundMask, QueryTriggerInteraction.Ignore))
            {
                pos.y = hit.point.y;
            }
        }

        pos.y += spawnHeightOffset;
        return pos;
    }

    void DespawnEnemies()
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
