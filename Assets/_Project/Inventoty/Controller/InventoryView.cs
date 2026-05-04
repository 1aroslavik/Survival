using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    public InventoryModel model;
    public List<Transform> slotPoints = new();

    Dictionary<int, List<GameObject>> visuals = new();

    void Start()
    {
        if (model == null)
            model = InventoryModel.Instance;

        Render();

        model.OnInventoryChanged += Render;
    }

    public void Render()
    {
        if (model == null) return;

        // очистка
        foreach (var list in visuals.Values)
        {
            foreach (var obj in list)
            {
                if (obj != null)
                    Destroy(obj);
            }
        }

        visuals.Clear();

        for (int i = 0; i < model.slots.Count && i < slotPoints.Count; i++)
        {
            var slot = model.slots[i];

            if (slot.isEmpty || slot.data.inventoryPrefab == null)
                continue;

            visuals[i] = new List<GameObject>();

            int visualCount = Mathf.Min(slot.amount, 5);

            var prefab = slot.data.inventoryPrefab;

            // 🔥 получаем РЕАЛЬНЫЙ размер предмета
            float itemSize = GetItemSize(prefab);

            // 🔥 генерим позиции с учётом размера
            List<Vector3> positions = GenerateClusterPositions(visualCount, itemSize);

            for (int j = 0; j < visualCount; j++)
            {
                Vector3 originalScale = prefab.transform.localScale;
                Quaternion originalRotation = prefab.transform.localRotation;

                var obj = Instantiate(prefab, slotPoints[i], false);

                var itemView = obj.GetComponent<InventoryItemsView>();
                if (itemView != null)
                {
                    itemView.model = model;
                    itemView.slotIndex = i;
                }

                obj.transform.localPosition = positions[j];

                obj.transform.localRotation = slot.data.itemType == ItemType.Quest
                    ? originalRotation
                    : originalRotation * GetRandomRotation();

                obj.transform.localScale = originalScale;

                visuals[i].Add(obj);
            }
        }
    }

    // 🔥 Получаем "радиус" предмета
    float GetItemSize(GameObject prefab)
    {
        Renderer r = prefab.GetComponentInChildren<Renderer>();
        if (r == null) return 0.3f;

        // берём максимальный размер по X/Y
        return Mathf.Max(r.bounds.size.x, r.bounds.size.y) * 0.5f;
    }

    // 🔥 КУЧКА с учётом размера предмета
    List<Vector3> GenerateClusterPositions(int count, float itemSize)
    {
        List<Vector3> result = new List<Vector3>();

        float minDistance = itemSize * 1.8f;   // чуть плотнее
        float radius = itemSize * (0.8f + count * 0.3f); // компактнее
        int maxTries = 200;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = Vector3.zero;
            bool valid = false;

            for (int t = 0; t < maxTries; t++)
            {
                // 🔥 БИАС К ЦЕНТРУ (ключ!)
                float r = Mathf.Pow(Random.value, 1.8f) * radius;
                float angle = Random.Range(0f, Mathf.PI * 2f);

                float x = Mathf.Cos(angle) * r;
                float y = Mathf.Sin(angle) * r;

                pos = new Vector3(x, i * itemSize * 0.15f, 0f);

                valid = true;

                foreach (var existing in result)
                {
                    if (Vector3.Distance(existing, pos) < minDistance)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid) break;
            }

            result.Add(pos);
        }

        // 🔥 ЛЁГКОЕ СЖАТИЕ К ЦЕНТРУ (делает “кучку”, а не разброс)
        Vector3 center = Vector3.zero;
        foreach (var p in result) center += p;
        center /= result.Count;

        for (int i = 0; i < result.Count; i++)
        {
            result[i] = Vector3.Lerp(result[i], center, 0.25f);
        }

        return result;
    }

    // 🔥 рандом поворота
    Quaternion GetRandomRotation()
    {
        float randomY = Random.Range(-60f, 60f);
        float randomX = Random.Range(-30f, 30f);
        float randomZ = Random.Range(-15f, 15f);

        return Quaternion.Euler(randomX, randomY, randomZ);
    }
}