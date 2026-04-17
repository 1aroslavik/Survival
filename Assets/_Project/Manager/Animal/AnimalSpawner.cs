using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class AnimalSpawner : MonoBehaviour
{
    public Transform player;
    public List<AnimalZone> zones = new List<AnimalZone>();

    public int maxAnimals = 20;
    public float minDistance = 15f;
    public float despawnDistance = 80f;
    public float checkInterval = 2f;

    private float timer;
    private List<GameObject> animals = new List<GameObject>();

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;

            Cleanup();
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        animals.RemoveAll(a => a == null);

        if (animals.Count >= maxAnimals)
            return;

        AnimalZone zone = GetNearestZone();
        if (zone == null) return;

        foreach (var type in zone.animals)
        {
            if (CountAnimals(type.name) >= type.maxCount)
                continue;

            Vector3 pos = type.spawnType == AnimalSpawnType.Water
                ? GetWaterPosition(zone)
                : GetLandPosition(zone);

            if (pos == Vector3.zero)
                continue;

            GameObject obj = Instantiate(type.prefab, pos, Quaternion.identity);

            SetupAnimal(obj, type, zone);
            animals.Add(obj);

            break;
        }
    }

    AnimalZone GetNearestZone()
    {
        AnimalZone best = null;
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

    int CountAnimals(string typeName)
    {
        int count = 0;

        foreach (var a in animals)
        {
            if (a == null) continue;

            if (a.name.Contains(typeName))
                count++;
        }

        return count;
    }

    // 🌊 ВОДА
    Vector3 GetWaterPosition(AnimalZone zone)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 circle = Random.insideUnitCircle * zone.radius;

            float waterY = zone.transform.position.y;

            Vector3 pos = new Vector3(
                zone.transform.position.x + circle.x,
                Random.Range(waterY - zone.maxDepth, waterY - zone.minDepth),
                zone.transform.position.z + circle.y
            );

            if (Vector3.Distance(player.position, pos) < minDistance)
                continue;

            return pos;
        }

        return Vector3.zero;
    }

    // 🌍 ЗЕМЛЯ
    Vector3 GetLandPosition(AnimalZone zone)
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

    void SetupAnimal(GameObject obj, AnimalType type, AnimalZone zone)
    {
        // 🐟 РЫБЫ
        FishAI fish = obj.GetComponent<FishAI>();
        if (fish != null)
        {
            fish.Init(zone.transform, type.swimRadius);
            return;
        }

        // 🐺 ЖИВОТНЫЕ
        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = type.speed;
        }

        // 🔥 базовый AI
        AnimalBaseAI baseAI = obj.GetComponent<AnimalBaseAI>();
        if (baseAI != null)
        {
            baseAI.roamRadius = type.roamRadius;
        }
    }

    void Cleanup()
    {
        for (int i = animals.Count - 1; i >= 0; i--)
        {
            if (animals[i] == null) continue;

            float dist = Vector3.Distance(player.position, animals[i].transform.position);

            if (dist > despawnDistance)
            {
                Destroy(animals[i]);
                animals.RemoveAt(i);
            }
        }
    }
}