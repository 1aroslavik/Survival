using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 25f;
    private bool hasHit = false;

    void OnTriggerEnter(Collider col)
    {
        if (hasHit) return;

        AnimalHealth health = col.GetComponentInParent<AnimalHealth>();

        if (health != null)
        {
            hasHit = true;

            Debug.Log("HIT ANIMAL");

            health.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}