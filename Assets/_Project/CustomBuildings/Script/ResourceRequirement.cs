using UnityEngine;

[System.Serializable]
public class ResourceRequirement
{
    public ResourceType type;
    public int requiredAmount;

    [HideInInspector]
    public int currentAmount;

    public GameObject buildPrefab; // что отображается при строительстве
    public GameObject dropPrefab;  // что падает при отмене
}