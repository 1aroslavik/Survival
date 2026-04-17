using UnityEngine;

public class WeaponEquipment : MonoBehaviour
{
    public HandsController hands;
    public InventoryModel inventory; // 🔥 ВАЖНО

    public void Equip(ItemData item)
    {
        if (item.itemType != ItemType.Weapon)
            return;

        hands.SetWeapon(item.handPrefab);
    }

    public void EquipFromSlot(int slotIndex)
    {
        if (inventory == null) return;

        if (slotIndex >= inventory.slots.Count)
            return;

        var slot = inventory.slots[slotIndex];

        if (slot.isEmpty)
            return;

        Equip(slot.data);
    }

    public void Unequip()
    {
        hands.ClearHands();
    }
}