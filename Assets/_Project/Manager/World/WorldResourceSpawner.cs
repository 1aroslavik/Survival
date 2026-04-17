using System.Collections.Generic;
using UnityEngine;

public class WorldResourceSpawner : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Prefabs")]
    public GameObject[] prefabs; // палки и камни

    [Header("Spawn Settings")]
    public float spawnRadius = 30f;
    public float despawnDistance = 50f;
    public int maxObjects = 100;
    public float spawnCheckInterval = 2f;

    [Header("Ground")]
    public LayerMask groundLayer;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnCheckInterval)
        {
            timer = 0f;

            DespawnFarObjects();
            SpawnIfNeeded();
        }
    }

    void SpawnIfNeeded()
    {
        if (spawnedObjects.Count >= maxObjects) return;

        int attempts = 10;

        while (spawnedObjects.Count < maxObjects && attempts > 0)
        {
            attempts--;

            Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y += 10f;

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                if (IsPositionFree(hit.point))
                {
                    SpawnObject(hit.point);
                }
            }
        }
    }

    void SpawnObject(Vector3 position)
    {
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);

        // рандом поворот
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        // небольшой рандом масштаба
        obj.transform.localScale *= Random.Range(0.9f, 1.2f);

        spawnedObjects.Add(obj);
    }

    void DespawnFarObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.position, spawnedObjects[i].transform.position);

            if (dist > despawnDistance)
            {
                Destroy(spawnedObjects[i]);
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    bool IsPositionFree(Vector3 pos)
    {
        float checkRadius = 1.5f;

        foreach (var obj in spawnedObjects)
        {
            if (obj == null) continue;

            if (Vector3.Distance(pos, obj.transform.position) < checkRadius)
                return false;
        }

        return true;
    }
}