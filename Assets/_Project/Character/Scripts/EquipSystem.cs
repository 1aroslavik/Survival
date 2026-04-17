using UnityEngine;

public class EquipSystem : MonoBehaviour
{
    public Transform HandAnchor;

    GameObject currentItem;
    ItemData currentData;

    public void Equip(ItemData data) 
    {
        if(data == null || data.handPrefab == null) return;

        Unequip();

        currentItem = Instantiate(
            data.handPrefab,
            HandAnchor
        );

        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
        currentData = data;
    }

    public void Unequip() 
    {
        if(currentItem != null) 
        {
            Destroy(currentItem);
            currentItem = null;
            currentData = null;
        }
    }
}
