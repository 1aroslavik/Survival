using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // PLAYER

    public float posX;
    public float posY;
    public float posZ;

    public float health;
    public float hunger;
    public float thirst;
    public float stamina;
    public float radiation;

    // INVENTORY

    public List<InventoryItemSave> inventory =
        new List<InventoryItemSave>();
    // BUILDINGS
    public List<BuildingSave> buildings =
    new List<BuildingSave>();
}