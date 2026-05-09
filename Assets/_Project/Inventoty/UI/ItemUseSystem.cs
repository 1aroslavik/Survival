using UnityEngine;

public class ItemUseSystem : MonoBehaviour
{
    public static ItemUseSystem Instance;

    [Tooltip("Перетащи сюда ItemData дневника (тот, что лежит в инвентаре и открывает записки)")]
    public ItemData journalItemData;

    PlayerStats playerStats;

    void Awake()
    {
        Instance = this;
        playerStats = FindObjectOfType<PlayerStats>();
    }

    public void UseItem(InventorySlotData slot)
    {
        if (slot == null || slot.isEmpty)
        {
            Debug.Log("❌ SLOT EMPTY");
            return;
        }

        var item = slot.data;

        switch (item.itemType)
        {
            // ================= FOOD =================

            case ItemType.Food:

                playerStats.Eat(item.hungerRestore);

                if (item.healthRestore > 0)
                    playerStats.Heal(item.healthRestore);

                if (item.radiationAdd > 0)
                    playerStats.AddRadiation(item.radiationAdd);

                if (item.radiationRemove > 0)
                    playerStats.RemoveRadiation(item.radiationRemove);

                break;

            // ================= DRINK =================

            case ItemType.Drink:

                playerStats.Drink(item.thirstRestore);

                if (item.healthRestore > 0)
                    playerStats.Heal(item.healthRestore);

                if (item.radiationAdd > 0)
                    playerStats.AddRadiation(item.radiationAdd);

                if (item.radiationRemove > 0)
                    playerStats.RemoveRadiation(item.radiationRemove);

                break;

            // ================= MEDICINE =================

            case ItemType.Medicine:

                playerStats.Heal(item.healthRestore);

                if (item.radiationRemove > 0)
                    playerStats.RemoveRadiation(item.radiationRemove);

                if (item.radiationAdd > 0)
                    playerStats.AddRadiation(item.radiationAdd);

                break;

            // ================= RESOURCE =================

            case ItemType.Resource:
                AddToCraft(slot);
                return;

            // ================= QUEST =================

            case ItemType.Quest:

                if (item == journalItemData)
                {
                    if (NoteReader.Instance != null)
                        NoteReader.Instance.Open();
                    else
                        Debug.LogWarning("NoteReader.Instance == null — добавь NoteReader в сцену");
                }

                if (InventoryTooltip.Instance != null)
                    InventoryTooltip.Instance.Hide();

                return;
        }

        // ================= CONSUME =================

        slot.amount--;

        if (slot.amount <= 0)
            slot.data = null;

        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.Hide();

        // безопасный вызов
        var view = FindObjectOfType<InventoryView>();

        if (view != null)
            view.Render();
    }

    void ConsumeItem(InventorySlotData slot)
    {
        slot.amount--;

        if (slot.amount <= 0)
            slot.data = null;

        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.Hide();

        FindObjectOfType<InventoryView>().Render();
    }

    void AddToCraft(InventorySlotData slot)
    {
        if (CraftArea.Instance == null)
            return;

        // добавляем предмет на коврик
        CraftArea.Instance.AddItem(slot.data);

        // уменьшаем количество в инвентаре
        slot.amount--;

        if (slot.amount <= 0)
            slot.data = null;

        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.Hide();

        FindObjectOfType<InventoryView>().Render();
    }
}