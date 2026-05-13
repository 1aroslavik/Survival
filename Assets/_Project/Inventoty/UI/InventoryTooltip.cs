using UnityEngine;
using TMPro;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance;

    [Header("Texts")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI amount;
    public TextMeshProUGUI hint;

    [Header("Use Button")]
    public GameObject useButtonObject;

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

        string action = "";

        switch (slot.data.itemType)
        {
            case ItemType.Food:
                action = "Eat";
                break;

            case ItemType.Drink:
                action = "Drink";
                break;

            case ItemType.Medicine:
                action = "Heal";
                break;

            case ItemType.Weapon:
                action = "Equip";
                break;

            case ItemType.Resource:
                action = "Add to Craft";
                break;

            case ItemType.Quest:
                action = "Inspect";
                break;
        }

        hint.text = action;

        if (useButtonObject != null)
            useButtonObject.SetActive(!string.IsNullOrEmpty(action));

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (useButtonObject != null)
            useButtonObject.SetActive(false);

        gameObject.SetActive(false);
    }
}