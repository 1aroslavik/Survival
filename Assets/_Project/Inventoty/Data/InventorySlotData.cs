using UnityEngine;

[System.Serializable]
public class InventorySlotData
{
    public ItemData data;
    public int amount;

    // 🔥 правильная проверка
    public bool isEmpty => data == null || amount <= 0;

    public void Clear()
    {
        data = null;
        amount = 0;
    }
}