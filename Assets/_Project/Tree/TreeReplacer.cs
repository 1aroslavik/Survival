using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeReplacer : MonoBehaviour
{
    public Terrain terrain;

    [System.Serializable]
    public class TreeReplacement
    {
        public string treeName;
        public GameObject prefab;
        public float yOffset;
    }

    public TreeReplacement[] replacements;

    private TerrainData tData;
    private Dictionary<string, TreeReplacement> dict;

    void Start()
    {
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("Terrain not assigned!");
            return;
        }

        tData = terrain.terrainData;

        dict = new Dictionary<string, TreeReplacement>();

        foreach (var r in replacements)
        {
            if (!dict.ContainsKey(r.treeName) && r.prefab != null)
                dict.Add(r.treeName, r);
        }
    }

    public GameObject TryChopClosestTree(Vector3 hitPoint)
    {
        TreeInstance[] trees = tData.treeInstances;

        int closestIndex = -1;
        float minDist = 2.5f;

        for (int i = 0; i < trees.Length; i++)
        {
            Vector3 worldPos =
                Vector3.Scale(trees[i].position, tData.size) +
                terrain.transform.position;

            float dist = Vector3.Distance(hitPoint, worldPos);

            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        if (closestIndex == -1)
            return null;

        TreeInstance tree = trees[closestIndex];

        string treeName =
            tData.treePrototypes[tree.prototypeIndex]
            .prefab.name
            .Replace("(Clone)", "")
            .Trim();

        if (!dict.TryGetValue(treeName, out TreeReplacement data))
        {
            Debug.LogWarning("Нет prefab для дерева: " + treeName);
            return null;
        }

        // 🔥 УДАЛЯЕМ дерево из terrain
        List<TreeInstance> list = trees.ToList();
        list.RemoveAt(closestIndex);
        tData.treeInstances = list.ToArray();
        terrain.Flush();

        // 📍 ПОЗИЦИЯ (1 в 1)
        Vector3 worldPosFinal =
            Vector3.Scale(tree.position, tData.size) +
            terrain.transform.position;

        float groundY =
            terrain.SampleHeight(worldPosFinal) +
            terrain.transform.position.y;

        Vector3 spawnPos =
    Vector3.Scale(tree.position, tData.size) +
    terrain.transform.position;

        // если нужен небольшой фикс (обычно 0)
        spawnPos.y -= data.yOffset;

        // 🔄 ПОВОРОТ (ВАЖНО — в радианах!)
        float rotationY = tree.rotation * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, rotationY, 0f);

        // 📏 МАСШТАБ (terrain scale)
        Vector3 scale = new Vector3(
            tree.widthScale,
            tree.heightScale,
            tree.widthScale
        );

        // 🔥 СОЗДАЕМ
        GameObject obj = Instantiate(data.prefab, spawnPos, rot);

        // 🔥 ПРИМЕНЯЕМ SCALE
        obj.transform.localScale = scale;

        return obj;
    }
}