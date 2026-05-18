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

        if (NoteReader.Instance != null && NoteReader.Instance.IsOpen)
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (slot.data != null && slot.data.worldPrefab != null)
            {
                Transform cam = Camera.main.transform;

                Vector3 dropPos =
                    cam.position +
                    cam.forward * 2f;

                Instantiate(
                    slot.data.worldPrefab,
                    dropPos,
                    Quaternion.identity
                );
            }

            // Удаляем только 1 предмет
            slot.Remove(1);

            model.OnInventoryChanged?.Invoke();

            // Если слот опустел
            if (slot.isEmpty)
            {
                isHovering = false;

                if (InventoryTooltip.Instance != null)
                    InventoryTooltip.Instance.Hide();
            }

            return;
        }
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
            else if (slot.data.resourceType == ResourceType.Note)
            {
                if (InventoryTooltip.Instance != null)
                    InventoryTooltip.Instance.Hide();

                if (NoteReader.Instance != null)
                    NoteReader.Instance.OpenForItem(slot.data);
            }
            else
            {
                if (ItemUseSystem.Instance != null)
                    ItemUseSystem.Instance.UseItem(slot);
            }
        }
    }
}