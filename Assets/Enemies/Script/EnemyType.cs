using UnityEngine;

[System.Serializable]
public class EnemyType
{
    public string name;
    public GameObject prefab;

    public int maxCount = 3;

    [Header("Stats")]
    public float maxHealth = 50f;
    public float damage = 10f;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float patrolRadius = 15f;

    [Header("Detection")]
    [Tooltip("Радиус обнаружения игрока — когда тот входит в него, враг бежит и атакует.")]
    public float detectionRadius = 12f;
}
