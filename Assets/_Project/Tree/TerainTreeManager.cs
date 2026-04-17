using UnityEngine;
using System.Collections.Generic;

public class TerrainTreeManager : MonoBehaviour
{
    public Terrain terrain;
    public Transform player;

    [Header("Settings")]
    public float activeDistance = 25f;
    public GameObject treePrefab;

    Dictionary<int, GameObject> spawnedTrees = new Dictionary<int, GameObject>();
    HashSet<int> removedTrees = new HashSet<int>();

    void Update()
    {
        if (terrain == null || player == null) return;

        TerrainData data = terrain.terrainData;
        TreeInstance[] trees = data.treeInstances;

        for (int i = 0; i < trees.Length; i++)
        {
            if (removedTrees.Contains(i))
                continue;

            Vector3 worldPos =
                Vector3.Scale(trees[i].position, data.size) +
                terrain.transform.position;

            float dist = Vector3.Distance(player.position, worldPos);

            if (dist < activeDistance)
            {
                if (!spawnedTrees.ContainsKey(i))
                {
                    Quaternion rot = Quaternion.Euler(0, trees[i].rotation * Mathf.Rad2Deg, 0);
                    GameObject tree = Instantiate(treePrefab, worldPos, rot);

                    TreeHealth health = tree.GetComponent<TreeHealth>();
                    if (health != null)
                        health.name = "TerrainTree_" + i;

                    spawnedTrees.Add(i, tree);
                }
            }
            else
            {
                if (spawnedTrees.ContainsKey(i))
                {
                    Destroy(spawnedTrees[i]);
                    spawnedTrees.Remove(i);
                }
            }
        }
    }

    // 🔥 ВЫЗЫВАЕМ ПРИ СРУБАНИИ
    public void RemoveTerrainTree(int index)
    {
        TerrainData data = terrain.terrainData;

        var trees = new System.Collections.Generic.List<TreeInstance>(data.treeInstances);

        if (index < 0 || index >= trees.Count)
            return;

        trees.RemoveAt(index);

        data.treeInstances = trees.ToArray();

        terrain.Flush(); // 🔥 обязательно!
    }
}