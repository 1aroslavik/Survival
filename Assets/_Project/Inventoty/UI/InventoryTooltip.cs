using UnityEngine;
using TMPro;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance;

    public TextMeshProUGUI itemName;
    public TextMeshProUGUI amount;
    public TextMeshProUGUI description;
    public TextMeshProUGUI hint;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Hide();
    }

    void Update()
    {
        transform.position = Input.mousePosition + new Vector3(15, -15, 0);
    }

    public void Show(InventorySlotData slot)
    {
        if (slot == null || slot.isEmpty) return;

        itemName.text = slot.data.itemName;
        amount.text = "x" + slot.amount;
        description.text = slot.data.description;

        string action = "";

        switch (slot.data.itemType)
        {
            case ItemType.Food:
                action = "[E] Eat";
                break;

            case ItemType.Drink:
                action = "[E] Drink"; // 👈 ВОТ ЭТО ТЕБЕ НУЖНО
                break;

            case ItemType.Medicine:
                action = "[E] Heal";
                break;

            case ItemType.Weapon:
                action = "[E] Equip";
                break;

            case ItemType.Resource:
                action = "[E] Add to Craft";
                break;

            case ItemType.Quest:
                action = "[E] Inspect";
                break;
        }

        hint.text = action;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}