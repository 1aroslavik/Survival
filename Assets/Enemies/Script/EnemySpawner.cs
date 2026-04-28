using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public Transform player;

    public int maxEnemies = 6;
    public float spawnRadius = 30f;
    public float minDistance = 10f;
    public float spawnDelay = 4f;

    int current;
    float timer;

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnDelay && current < maxEnemies)
        {
            timer = 0f;
            Spawn();
        }
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
                Instantiate(prefab, hit.position, Quaternion.identity);
                current++;
                return;
            }
        }
    }
}