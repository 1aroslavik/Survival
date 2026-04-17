using UnityEngine;

public class InventoryItemsView : MonoBehaviour
{
    public int slotIndex;
    public InventoryModel model;

    bool isHovering;

    void OnMouseEnter()
    {
        if (model == null || slotIndex >= model.slots.Count)
            return;

        isHovering = true;

        var slot = model.slots[slotIndex];

        if (slot == null || slot.isEmpty)
            return;

        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.Show(slot);
    }

    void OnMouseExit()
    {
        isHovering = false;

        if (InventoryTooltip.Instance != null)
            InventoryTooltip.Instance.Hide();
    }

    void Update()
    {
        if (!isHovering) return;

        // 🔥 защита от краша
        if (model == null || slotIndex >= model.slots.Count)
            return;

        var slot = model.slots[slotIndex];

        if (slot == null || slot.isEmpty)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (slot.data == null)
                return;

            if (slot.data.itemType == ItemType.Weapon)
            {
                WeaponEquipment equipment = FindFirstObjectByType<WeaponEquipment>();

                if (equipment != null)
                    equipment.Equip(slot.data);
            }
            else
            {
                if (ItemUseSystem.Instance != null)
                    ItemUseSystem.Instance.UseItem(slot);
            }
        }
    }
}