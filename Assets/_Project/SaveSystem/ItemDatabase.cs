using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    public List<ItemData> allItems = new();

    void Awake()
    {
        Instance = this;
    }

    public ItemData GetByID(string id)
    {
        return allItems.Find(x => x.itemID == id);
    }
}