using UnityEngine;

public class Dryer : MonoBehaviour
{
    public Transform[] slots;

    [Header("Meat Settings")]
    public ItemData meatInventory;
    public GameObject meatWorld;

    public bool TryAddMeat(InventoryModel inventory)
    {
        // 🔹 ищем мясо в инвентаре
        foreach (var slot in inventory.slots)
        {
            if (!slot.isEmpty && slot.data == meatInventory)
            {
                // 🔹 ищем реально пустой слот
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].childCount == 0) // ✅ единственная проверка
                    {
                        slot.amount--;

                        if (slot.amount <= 0)
                            slot.Clear();

                        SpawnMeat(i);
                        return true;
                    }
                }

                Debug.Log("Сушилка заполнена");
                return false;
            }
        }

        Debug.Log("Нет мяса");
        return false;
    }

    void SpawnMeat(int index)
    {
        GameObject meat = Instantiate(meatWorld, slots[index]);
        meat.transform.localPosition = Vector3.zero;
        meat.transform.localRotation = Quaternion.identity;

        // 🔥 ВЫКЛЮЧАЕМ ФИЗИКУ
        Rigidbody rb = meat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;   // 👈 главное
            rb.useGravity = false;   // 👈 чтобы не падало
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // (опционально) отключаем коллайдер, если мешает
        Collider col = meat.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 🔥 ВКЛЮЧАЕМ СУШКУ
        DryingMeat drying = meat.GetComponent<DryingMeat>();
        if (drying != null)
        {
            drying.isOnDryer = true;
        }
    }
}