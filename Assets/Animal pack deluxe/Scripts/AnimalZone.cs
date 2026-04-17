using UnityEngine;

public class AnimalZone : MonoBehaviour
{
    public float radius = 50f;

    [Header("Water Depth")]
    public float minDepth = 1f;
    public float maxDepth = 4f;

    public AnimalType[] animals;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}