using UnityEngine;

public class Dryer : MonoBehaviour
{
    public Transform[] slots;
    public GameObject meatPrefab;

    public ResourceType meatType; // ← используем ResourceType

    private bool[] occupied;

    void Start()
    {
        occupied = new bool[slots.Length];
    }

    public bool TryAddMeat(InventoryModel inventory)
    {
        // 🔹 проверяем есть ли мясо
        bool hasMeat = false;

        foreach (var slot in inventory.slots)
        {
            if (!slot.isEmpty && slot.data.resourceType == meatType)
            {
                hasMeat = true;
                break;
            }
        }

        if (!hasMeat)
        {
            Debug.Log("Нет мяса");
            return false;
        }

        // 🔹 ищем свободный слот сушилки
        for (int i = 0; i < slots.Length; i++)
        {
            if (!occupied[i])
            {
                // удаляем 1 мясо
                if (inventory.TryRemoveOne(meatType))
                {
                    SpawnMeat(i);
                    occupied[i] = true;
                    return true;
                }
            }
        }

        Debug.Log("Сушилка заполнена");
        return false;
    }

    void SpawnMeat(int index)
    {
        GameObject meat = Instantiate(meatPrefab, slots[index]);
        meat.transform.localPosition = Vector3.zero;
        meat.transform.localRotation = Quaternion.identity;
    }
}