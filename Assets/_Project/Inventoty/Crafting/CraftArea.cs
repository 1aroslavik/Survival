using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftSlot
{
    public ItemData item;
    public int amount;

    public List<GameObject> visuals = new();
}

public class CraftArea : MonoBehaviour
{
    public static CraftArea Instance;

    public Transform[] craftSlots;
    public Camera inventoryCamera;
    private List<CraftSlot> slots = new();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("ПКМ нажата");

            Ray ray = inventoryCamera.ScreenPointToRay(Input.mousePosition);

            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 5f);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log("ПОПАЛИ В: " + hit.collider.name);

                foreach (var slot in slots)
                {
                    foreach (var obj in slot.visuals)
                    {
                        Debug.Log("ПРОВЕРЯЕМ: " + obj.name);

                        if (hit.collider.gameObject == obj ||
                            hit.collider.transform.IsChildOf(obj.transform))
                        {
                            Debug.Log("НАЙДЕН ПРЕДМЕТ КРАФТА");

                            RemoveItem(slot);

                            return;
                        }
                    }
                }

                Debug.LogError("НЕ НАЙДЕН В visuals");
            }
            else
            {
                Debug.LogError("RAYCAST НЕ ПОПАЛ");
            }
        }
    }
    public void RemoveItem(CraftSlot slot)
    {
        if (slot == null)
            return;

        InventoryModel inventory = FindObjectOfType<InventoryModel>();

        bool added = false;

        // ищем существующий стак
        foreach (var invSlot in inventory.slots)
        {
            if (invSlot.data == slot.item)
            {
                invSlot.amount++;

                added = true;

                break;
            }
        }

        // ищем пустой слот
        if (!added)
        {
            foreach (var invSlot in inventory.slots)
            {
                if (invSlot.isEmpty)
                {
                    invSlot.data = slot.item;
                    invSlot.amount = 1;

                    added = true;

                    break;
                }
            }
        }

        if (!added)
        {
            Debug.LogError("Нет места в инвентаре");
            return;
        }

        // ОБНОВИТЬ INVENTORY
        inventory.OnInventoryChanged?.Invoke();

        // удалить объект из крафта
        if (slot.visuals.Count > 0)
        {
            GameObject obj = slot.visuals[slot.visuals.Count - 1];

            slot.visuals.RemoveAt(slot.visuals.Count - 1);

            Destroy(obj);
        }

        slot.amount--;

        if (slot.amount <= 0)
        {
            slots.Remove(slot);

            RebuildSlots();
        }

        CraftingSystem.Instance.CheckRecipes(GetItems());

        FindObjectOfType<InventoryView>().Render();
    }
    public void AddItem(ItemData item)
    {
        Debug.Log("AddItem called: " + item.name);

        // ищем существующий стак
        foreach (var slot in slots)
        {
            if (slot.item == item)
            {
                slot.amount++;

                Transform parent = slot.visuals[0].transform.parent;

                Vector3 offset = new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    0,
                    Random.Range(-0.1f, 0.1f)
                );

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

                CraftingSystem.Instance.CheckRecipes(GetItems());
                return;
            }
        }

        // новый слот
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

    public void RemoveItem(ItemData item)
    {
        CraftSlot slot = slots.Find(s => s.item == item);

        if (slot == null)
            return;

        // вернуть предмет в инвентарь
        InventoryModel inventory = FindObjectOfType<InventoryModel>();

        foreach (var invSlot in inventory.slots)
        {
            if (invSlot.data == item)
            {
                invSlot.Add(1);
                break;
            }
        }

        // удалить визуальный объект
        if (slot.visuals.Count > 0)
        {
            GameObject obj = slot.visuals[slot.visuals.Count - 1];

            slot.visuals.RemoveAt(slot.visuals.Count - 1);

            Destroy(obj);
        }

        // уменьшить количество в крафте
        slot.amount--;

        // удалить слот если пуст
        if (slot.amount <= 0)
        {
            slots.Remove(slot);

            RebuildSlots();
        }

        CraftingSystem.Instance.CheckRecipes(GetItems());

        FindObjectOfType<InventoryView>().Render();
    }

    void RebuildSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            Transform target = craftSlots[i];

            foreach (var obj in slots[i].visuals)
            {
                obj.transform.SetParent(target);

                obj.transform.position =
                    target.position +
                    new Vector3(
                        Random.Range(-0.1f, 0.1f),
                        0,
                        Random.Range(-0.1f, 0.1f)
                    );

                obj.transform.rotation =
                    Quaternion.Euler(
                        Random.Range(-10f, 10f),
                        Random.Range(0f, 360f),
                        Random.Range(-10f, 10f)
                    );
            }
        }
    }

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