using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    public InventoryModel model;
    public List<Transform> slotPoints = new();

    Dictionary<int, List<GameObject>> visuals = new();

    void Start()
    {
        Render();
    }

    public void Render()
    {
        if (model == null)
        {
            Debug.LogError("MODEL NULL");
            return;
        }

        // 🔹 очистка
        foreach (var list in visuals.Values)
        {
            foreach (var obj in list)
                Destroy(obj);
        }

        visuals.Clear();

        for (int i = 0; i < model.slots.Count && i < slotPoints.Count; i++)
        {
            var slot = model.slots[i];
            if (slot.isEmpty) continue;

            if (slot.data.inventoryPrefab == null)
                continue;

            int visualCount = Mathf.Min(slot.amount, 5);

            visuals[i] = new List<GameObject>();

            for (int j = 0; j < visualCount; j++)
            {
                var obj = Instantiate(slot.data.inventoryPrefab, slotPoints[i]);

                var itemView = obj.GetComponent<InventoryItemsView>();

                // 🔥 ВАЖНО
                itemView.model = model;
                itemView.slotIndex = i;

                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                visuals[i].Add(obj);
            }
        }
    }
}