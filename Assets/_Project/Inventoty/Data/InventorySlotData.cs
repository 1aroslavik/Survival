using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public ItemData data;
    public int amount;

    // ❗ теперь слот пуст ТОЛЬКО если data == null
    public bool isEmpty => data == null;

    public void Set(ItemData newData, int newAmount)
    {
        data = newData;
        amount = newAmount;
    }

    public void Add(int value)
    {
        amount += value;
    }

    public void Remove(int value)
    {
        amount -= value;

        if (amount <= 0)
            Clear();
    }

    public void Clear()
    {
        data = null;
        amount = 0;
    }
}