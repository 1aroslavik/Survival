using System.Collections.Generic;
using UnityEngine;

public class InventoryModel : MonoBehaviour
{
    public int SlotCount = 20;
    public List<InventorySlotData> slots = new();

    private void Awake()
    {
        slots.Clear();

        for (int i = 0; i < SlotCount; i++)
        {
            slots.Add(new InventorySlotData());
        }
    }

    public bool TryAdd(ItemData data, int amount)
    {
        int remaining = amount;

        // 🔹 1. СТАКИ (ОБЯЗАТЕЛЬНО !isEmpty)
        if (data.isStackable)
        {
            foreach (var slot in slots)
            {
                if (!slot.isEmpty && slot.data == data && slot.amount < data.maxStack)
                {
                    int space = data.maxStack - slot.amount;
                    int toAdd = Mathf.Min(space, remaining);

                    slot.amount += toAdd;
                    remaining -= toAdd;

                    if (remaining == 0)
                        return true;
                }
            }
        }

        // 🔹 2. ПУСТЫЕ СЛОТЫ
        foreach (var slot in slots)
        {
            if (slot.isEmpty)
            {
                int toAdd = data.isStackable ? Mathf.Min(data.maxStack, remaining) : 1;

                slot.data = data;
                slot.amount = toAdd;

                remaining -= toAdd;

                if (remaining == 0)
                    return true;
            }
        }

        // ❌ НЕ ВЛЕЗЛО
        Debug.Log("🚫 INVENTORY FULL, remaining = " + remaining);
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

            return true;
        }

        return false;
    }
}