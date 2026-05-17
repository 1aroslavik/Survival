using System.Collections.Generic;
using UnityEngine;

public class InventoryModel : MonoBehaviour
{
    public int SlotCount = 20;
    public List<InventorySlotData> slots = new();
    public static InventoryModel Instance;
    public System.Action OnInventoryChanged;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (slots.Count == 0)
        {
            for (int i = 0; i < SlotCount; i++)
                slots.Add(new InventorySlotData());
        }
    }

    // 🔥 ДОБАВЛЕНИЕ
    public bool TryAdd(ItemData data, int amount)
    {
        int remaining = amount;

        if (data.isStackable)
        {
            foreach (var slot in slots)
            {
                if (slot.isEmpty)
                    continue;

                if (slot.data != data)
                    continue;

                if (slot.amount >= data.maxStack)
                    continue;

                int space = data.maxStack - slot.amount;
                int toAdd = Mathf.Min(space, remaining);

                slot.amount += toAdd;
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    OnInventoryChanged?.Invoke(); // ✅ В КОНЦЕ
                    return true;
                }
            }
        }

        foreach (var slot in slots)
        {
            if (!slot.isEmpty)
                continue;

            int toAdd = data.isStackable ? Mathf.Min(data.maxStack, remaining) : 1;

            slot.data = data;
            slot.amount = toAdd;

            remaining -= toAdd;

            if (remaining <= 0)
            {
                OnInventoryChanged?.Invoke(); // ✅ В КОНЦЕ
                return true;
            }
        }

        Debug.Log("🚫 INVENTORY FULL");
        return false;
    }

    // 🔥 ПРОВЕРКА
    public bool HasItem(ResourceType type)
    {
        foreach (var slot in slots)
        {
            if (slot.isEmpty)
                continue;

            if (slot.data.resourceType == type && slot.amount > 0)
                return true;
        }

        return false;
    }

    // 🔥 УДАЛЕНИЕ
    public bool TryRemoveOne(ResourceType type)
    {
        foreach (var slot in slots)
        {
            if (slot.isEmpty)
                continue;

            if (slot.data.resourceType != type)
                continue;

            slot.amount--;

            if (slot.amount <= 0)
                slot.Clear();

            OnInventoryChanged?.Invoke(); // ✅ ВАЖНО

            return true;
        }

        return false;
    }

    public bool IsFull()
    {
        foreach (var slot in slots)
        {
            if (slot.isEmpty)
                return false;
        }

        return true;
    }
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }

        OnInventoryChanged?.Invoke();
    }
}