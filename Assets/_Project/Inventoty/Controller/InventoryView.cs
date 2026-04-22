using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    public InventoryModel model;
    public List<Transform> slotPoints = new();

    Dictionary<int, List<GameObject>> visuals = new();

    void Start()
    {
        Debug.Log("=== INVENTORY VIEW START ===");
        Render();
    }

    public void Render()
    {
        Debug.Log("=== RENDER START ===");

        if (model == null)
        {
            Debug.LogError("❌ MODEL NULL");
            return;
        }

        // 🔹 очистка
        Debug.Log("Clearing old visuals...");
        foreach (var list in visuals.Values)
        {
            foreach (var obj in list)
            {
                if (obj != null)
                    Destroy(obj);
            }
        }

        visuals.Clear();

        // 🔥 ПРОВЕРКА ВСЕХ СЛОТОВ
        Debug.Log("=== MODEL SLOTS STATE ===");

        for (int i = 0; i < model.slots.Count; i++)
        {
            var s = model.slots[i];

            if (s.isEmpty)
            {
                Debug.Log($"Slot {i}: EMPTY");
            }
            else
            {
                Debug.Log($"Slot {i}: {s.data.name} | amount: {s.amount}");
            }
        }

        Debug.Log("=== START SPAWN VISUALS ===");

        for (int i = 0; i < model.slots.Count && i < slotPoints.Count; i++)
        {
            var slot = model.slots[i];

            if (slot.isEmpty)
            {
                Debug.Log($"Skip slot {i} (empty)");
                continue;
            }

            if (slot.data.inventoryPrefab == null)
            {
                Debug.LogError($"❌ Slot {i} has NO prefab: {slot.data.name}");
                continue;
            }

            Debug.Log($"Render slot {i}: {slot.data.name}");

            int visualCount = Mathf.Min(slot.amount, 5);

            visuals[i] = new List<GameObject>();

            for (int j = 0; j < visualCount; j++)
            {
                Debug.Log($"  Spawn visual {j} for slot {i}");

                var obj = Instantiate(slot.data.inventoryPrefab, slotPoints[i]);

                Debug.Log($"    Spawned prefab: {obj.name}");

                var itemView = obj.GetComponent<InventoryItemsView>();

                if (itemView == null)
                {
                    Debug.LogError("❌ NO InventoryItemsView on prefab!");
                }
                else
                {
                    itemView.model = model;
                    itemView.slotIndex = i;

                    Debug.Log($"    Assigned slotIndex: {i}");
                }

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                visuals[i].Add(obj);
            }
        }

        Debug.Log("=== RENDER END ===");
    }
}