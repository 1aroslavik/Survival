using UnityEngine;

[System.Serializable]
public class EnemyType
{
    public string name;
    public GameObject prefab;

    public int maxCount = 3;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float patrolRadius = 15f;
}
