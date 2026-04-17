using System.Collections.Generic;
using UnityEngine;

public class TreeReplacer : MonoBehaviour
{
    public static TreeReplacer Instance;

    public Terrain terrain;

    [System.Serializable]
    public class TreeReplacement
    {
        public string treeName;
        public GameObject prefab;

        [Header("Offset")]
        public float yOffset; // 🔥 вручную регулируешь высоту
    }

    public TreeReplacement[] replacements;

    private TerrainData tData;
    private Dictionary<string, TreeReplacement> dict;

    void Awake()
    {
        Instance = this;
    }

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

    public GameObject TryChopAndSpawn(Vector3 hitPoint)
    {
        TreeInstance[] trees = tData.treeInstances;

        for (int i = 0; i < trees.Length; i++)
        {
            Vector3 worldPos = Vector3.Scale(trees[i].position, tData.size) + terrain.transform.position;

            if (Vector3.Distance(hitPoint, worldPos) < 4f)
            {
                string treeName =
                    tData.treePrototypes[trees[i].prototypeIndex].prefab.name;

                if (dict.TryGetValue(treeName, out TreeReplacement data))
                {
                    // 🔥 удаляем дерево
                    List<TreeInstance> list = new List<TreeInstance>(trees);
                    list.RemoveAt(i);
                    tData.treeInstances = list.ToArray();
                    terrain.Flush();

                    Quaternion rot = Quaternion.Euler(
                        0f,
                        trees[i].rotation,
                        0f
                    );

                    // 🔥 базовая позиция по земле
                    float groundY = terrain.SampleHeight(worldPos) + terrain.transform.position.y;

                    Vector3 spawnPos = new Vector3(worldPos.x, groundY, worldPos.z);

                    // 🔥 применяем offset (ВОТ ГЛАВНОЕ)
                    spawnPos.y -= data.yOffset;

                    GameObject obj = Instantiate(data.prefab, spawnPos, rot);

                    return obj;
                }
            }
        }

        return null;
    }
}