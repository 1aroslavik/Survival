using System.Collections.Generic;
using UnityEngine;

public class ZoneVisionSpawner : MonoBehaviour
{
    public List<GameObject> prefabs;

    public int spawnCount = 3;
    public float spawnRadius = 25f;

    public Transform player;

    public float viewAngle = 120f;
    public float maxViewDistance = 50f;

    public float hideDelay = 0.8f;

    private List<GameObject> spawned = new List<GameObject>();
    private bool isSpawned = false;
    private float notLookingTimer = 0f;

    void Update()
    {
        if (player == null) return;

        bool looking = IsPlayerLooking();

        if (looking)
        {
            notLookingTimer = 0f;

            if (!isSpawned)
            {
                SpawnAll();
                isSpawned = true;
            }

            SetVisible(true);
        }
        else
        {
            notLookingTimer += Time.deltaTime;

            if (notLookingTimer >= hideDelay)
            {
                SetVisible(false);
            }
        }
    }

    bool IsPlayerLooking()
    {
        Vector3 dir = (transform.position - player.position).normalized;

        float angle = Vector3.Angle(player.forward, dir);
        float dist = Vector3.Distance(player.position, transform.position);

        return angle < viewAngle && dist < maxViewDistance;
    }

    void SpawnAll()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            pos.y = transform.position.y;

            GameObject obj = Instantiate(prefabs[Random.Range(0, prefabs.Count)], pos, Quaternion.identity);
            spawned.Add(obj);
        }
    }

    void SetVisible(bool state)
    {
        foreach (var obj in spawned)
        {
            if (obj == null) continue;

            Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
                r.enabled = state;
        }
    }
}