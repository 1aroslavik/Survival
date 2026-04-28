using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    public float radius = 40f;

    public EnemyType[] enemies;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
