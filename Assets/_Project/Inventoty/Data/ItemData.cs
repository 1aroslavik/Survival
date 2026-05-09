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
    // ================= BASE =================

    [Header("Base")]
    public string itemName;

    [TextArea]
    public string description;

    public ItemType itemType;

    // ================= RESOURCE =================

    [Header("Resource")]
    public ResourceType resourceType;

    // ================= INVENTORY =================

    [Header("Inventory")]
    public bool isStackable = true;
    public int maxStack = 10;

    // ================= NEEDS =================

    [Header("Needs Restore")]

    [Tooltip("Восстановление голода")]
    public float hungerRestore;

    [Tooltip("Восстановление жажды")]
    public float thirstRestore;

    [Tooltip("Лечение здоровья")]
    public float healthRestore;

    [Tooltip("Восстановление стамины")]
    public float staminaRestore;

    // ================= RADIATION =================

    [Header("Radiation")]

    [Tooltip("Добавляет радиацию")]
    public float radiationAdd;

    [Tooltip("Убирает радиацию")]
    public float radiationRemove;

    // ================= EFFECTS =================

    [Header("Consumable Effects")]

    [Tooltip("Кровотечение")]
    public bool causesBleeding;

    [Tooltip("Отравление")]
    public bool causesPoison;

    [Tooltip("Можно использовать")]
    public bool isConsumable = true;

    // ================= VISUAL =================

    [Header("Visual")]
    public GameObject inventoryPrefab;
    public GameObject handPrefab;
    public Sprite noteImage;

    // ================= WEAPON =================

    [Header("Weapon")]
    public RuntimeAnimatorController weaponAnimator;

    // ================= AUDIO =================

    [Header("Audio")]
    public AudioClip useSound;
}