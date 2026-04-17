using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public int slotIndex;
    public InventoryModel model;

    public WeaponEquipment weaponEquipment;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (model == null) return;
        if (slotIndex >= model.slots.Count) return;

        var slot = model.slots[slotIndex];

        if (!slot.isEmpty)
        {
            InventoryTooltip.Instance.Show(slot);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryTooltip.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLICK SLOT");
        if (model == null) return;
        if (slotIndex >= model.slots.Count) return;

        var slot = model.slots[slotIndex];

        if (slot.isEmpty) return;

        if (slot.data.itemType == ItemType.Weapon)
        {
            weaponEquipment.EquipFromSlot(slotIndex);
        }
    }
}