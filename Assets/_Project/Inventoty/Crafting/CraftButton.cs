using UnityEngine;
using UnityEngine.EventSystems;

public class CraftButton : MonoBehaviour
{
    public void Craft()
    {
        CraftingSystem.Instance.Craft();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("BUTTON CLICK DETECTED");
    }
}