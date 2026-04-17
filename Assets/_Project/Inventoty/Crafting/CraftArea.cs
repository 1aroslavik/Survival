using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftSlot
{
    public ItemData item;
    public int amount;

    // 🔥 список всех визуальных объектов (кучка)
    public List<GameObject> visuals = new();
}

public class CraftArea : MonoBehaviour
{
    public static CraftArea Instance;

    public Transform[] craftSlots;

    private List<CraftSlot> slots = new();

    void Awake()
    {
        Instance = this;
    }

    public void AddItem(ItemData item)
    {
        Debug.Log("AddItem called: " + item.name);

        // 🔥 1. ищем существующий слот (стак)
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                slot.amount++;

                Transform parent = slot.visuals[0].transform.parent;

                // 🔥 случайное смещение
                Vector3 offset = new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    0,
                    Random.Range(-0.1f, 0.1f)
                );

                // 🔥 случайный поворот
                Quaternion rot = Quaternion.Euler(
                    Random.Range(-10f, 10f),
                    Random.Range(0f, 360f),
                    Random.Range(-10f, 10f)
                );

                GameObject obj = Instantiate(
                    item.inventoryPrefab,
                    parent.position + offset,
                    rot,
                    parent
                );

                slot.visuals.Add(obj);

                Debug.Log("Stack increased: " + slot.amount);

                CraftingSystem.Instance.CheckRecipes(GetItems());
                return;
            }
        }

        // 🔥 2. создаём новый слот
        if (slots.Count >= craftSlots.Length)
        {
            Debug.Log("No free craft slots");
            return;
        }

        int index = slots.Count;

        Quaternion rotNew = Quaternion.Euler(
            Random.Range(-10f, 10f),
            Random.Range(0f, 360f),
            Random.Range(-10f, 10f)
        );

        GameObject objNew = Instantiate(
            item.inventoryPrefab,
            craftSlots[index].position,
            rotNew,
            craftSlots[index]
        );

        CraftSlot newSlot = new CraftSlot
        {
            item = item,
            amount = 1
        };

        newSlot.visuals.Add(objNew);

        slots.Add(newSlot);

        CraftingSystem.Instance.CheckRecipes(GetItems());
    }

    public void Clear()
    {
        foreach (var slot in slots)
        {
            foreach (var obj in slot.visuals)
            {
                if (obj != null)
                    Destroy(obj);
            }
        }

        slots.Clear();
    }

    // 🔥 возвращаем список для рецептов (с учётом количества)
    public List<ItemData> GetItems()
    {
        List<ItemData> list = new();

        foreach (var slot in slots)
        {
            for (int i = 0; i < slot.amount; i++)
            {
                list.Add(slot.item);
            }
        }

        return list;
    }
}