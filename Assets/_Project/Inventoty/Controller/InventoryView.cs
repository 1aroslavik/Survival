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
        if (model == null) return;

        // очистка
        foreach (var list in visuals.Values)
            foreach (var obj in list)
                if (obj != null) Destroy(obj);

        visuals.Clear();

        for (int i = 0; i < model.slots.Count && i < slotPoints.Count; i++)
        {
            var slot = model.slots[i];
            if (slot.isEmpty || slot.data.inventoryPrefab == null)
                continue;

            visuals[i] = new List<GameObject>();

            int visualCount = Mathf.Min(slot.amount, 5);

            for (int j = 0; j < visualCount; j++)
            {
                // 🔥 сохраняем ОРИГИНАЛ префаба
                var prefab = slot.data.inventoryPrefab;
                Vector3 originalScale = prefab.transform.localScale;
                Quaternion originalRotation = prefab.transform.localRotation;

                // создаём
                var obj = Instantiate(prefab, slotPoints[i], false);

                // логика
                var itemView = obj.GetComponent<InventoryItemsView>();
                if (itemView != null)
                {
                    itemView.model = model;
                    itemView.slotIndex = i;
                }

                // ✅ ставим в центр
                obj.transform.localPosition = Vector3.zero;

                // 🔥 ВОЗВРАЩАЕМ как в префабе
                obj.transform.localRotation = originalRotation;
                obj.transform.localScale = originalScale;

                visuals[i].Add(obj);
            }
        }
    }
}