using UnityEngine;

public enum ItemType
{
    Resource,
    Food,
    Medicine,
    Drink,
    Tool,
    Weapon,
    Ammo,
    Building,
    Quest
}

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Base")]
    public string itemName;
    [TextArea] public string description;

    public ItemType itemType;
    [Header("Resource")]
    public ResourceType resourceType;
    [Header("Inventory")]
    public bool isStackable = true;
    public int maxStack = 10;

    [Header("Effects")]
    public float hungerRestore;
    public float thirstRestore;
    public float healthRestore;

    [Header("Visual")]
    public GameObject inventoryPrefab;
    public GameObject handPrefab;

    [Header("Weapon")]
    public RuntimeAnimatorController weaponAnimator;
}