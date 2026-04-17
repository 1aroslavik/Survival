using UnityEngine;

public enum AnimalSpawnType
{
    Land,
    Water
}

[System.Serializable]
public class AnimalType
{
    public string name;
    public GameObject prefab;

    public AnimalSpawnType spawnType;

    public int maxCount = 5;

    [Header("Land")]
    public float speed = 3f;
    public float roamRadius = 10f;

    [Header("Water")]
    public float swimRadius = 10f;
}