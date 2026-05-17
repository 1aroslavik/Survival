using System.Collections.Generic;
using UnityEngine;

public class BuildingDatabase : MonoBehaviour
{
    public static BuildingDatabase Instance;

    public List<BuildingData> allBuildings =
        new();

    void Awake()
    {
        Instance = this;
    }

    public BuildingData GetByID(string id)
    {
        return allBuildings.Find(
            x => x.buildingID == id
        );
    }
}