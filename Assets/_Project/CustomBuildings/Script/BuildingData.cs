using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Building/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Save")]
    public string buildingID;

    public GameObject constructionPrefab;
    public GameObject finishedPrefab;

    [Header("Resources")]
    public List<ResourceRequirement> resources;
}