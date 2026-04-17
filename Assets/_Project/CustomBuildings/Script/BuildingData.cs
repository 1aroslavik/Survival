using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Building/Building Data")]
public class BuildingData : ScriptableObject
{
    public GameObject constructionPrefab;
    public GameObject finishedPrefab;

    [Header("Resources")]
    public List<ResourceRequirement> resources;
}