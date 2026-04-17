using System.Collections.Generic;
using UnityEngine;

public class TerrainTreeChopper : MonoBehaviour
{
    public Terrain terrain;

    [Header("Assign prefabs in SAME order as Terrain prototypes")]
    public GameObject[] treePrefabs;

    public float searchRadius = 5f;

    TerrainData data;

    void Awake()
    {
        if (!terrain)
            terrain = Terrain.activeTerrain;

        if (!terrain)
        {
            Debug.LogError("NO TERRAIN FOUND");
            enabled = false;
            return;
        }

        data = terrain.terrainData;
    }

    public GameObject TryChopAndSpawn(Vector3 hitPoint)
    {
        List<TreeInstance> trees = new List<TreeInstance>(data.treeInstances);

        int closestIndex = -1;
        float closestDist = float.MaxValue;

        for (int i = 0; i < trees.Count; i++)
        {
            Vector3 worldPos = TreeToWorld(trees[i]);
            float dist = Vector3.Distance(hitPoint, worldPos);

            if (dist < closestDist && dist <= searchRadius)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        if (closestIndex == -1)
            return null;

        TreeInstance tree = trees[closestIndex];

        // 🔥 ВАЖНО: индекс Terrain должен совпадать с массивом
        if (tree.prototypeIndex >= treePrefabs.Length)
        {
            Debug.LogError("Prototype index out of range!");
            return null;
        }

        GameObject prefab = treePrefabs[tree.prototypeIndex];

        if (prefab == null)
        {
            Debug.LogError("Prefab not assigned in array!");
            return null;
        }

        Vector3 spawnPos = TreeToWorld(tree);

        // Удаляем из terrain
        trees.RemoveAt(closestIndex);
        data.treeInstances = trees.ToArray();
        terrain.Flush();

        // Rotation
        Quaternion rotation = Quaternion.Euler(0f, tree.rotation * Mathf.Rad2Deg, 0f);

        GameObject spawned = Instantiate(prefab, spawnPos, rotation);
        Physics.SyncTransforms();


        // 🔥 ГАРАНТИРУЕМ Rigidbody
        Rigidbody rb = spawned.GetComponentInChildren<Rigidbody>(true);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
        }
        else
        {
            Debug.LogError("NO RIGIDBODY FOUND ON SPAWNED TREE");
        }

        return spawned;
    }

    Vector3 TreeToWorld(TreeInstance tree)
    {
        Vector3 p = tree.position;

        p.x *= data.size.x;
        p.y *= data.size.y;
        p.z *= data.size.z;

        return terrain.transform.position + p;
    }
}