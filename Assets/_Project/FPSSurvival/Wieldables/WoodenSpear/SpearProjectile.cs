using UnityEngine;

public class SpearProjectile : MonoBehaviour
{
    public float damage = 40f;

    private bool hasHit = false;
    private Vector3 prevPos;

    void Start()
    {
        prevPos = transform.position;
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        Vector3 curr = transform.position;
        Vector3 delta = curr - prevPos;
        float dist = delta.magnitude;

        if (dist > 0.001f)
        {
            if (Physics.Raycast(prevPos, delta.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            {
                if (TryHit(hit.collider)) return;
            }
        }

        prevPos = curr;
    }

    void OnTriggerEnter(Collider col)
    {
        if (hasHit) return;
        TryHit(col);
    }

    void OnCollisionEnter(Collision col)
    {
        if (hasHit) return;
        TryHit(col.collider);
    }

    bool TryHit(Collider col)
    {
        AnimalHealth animal = col.GetComponentInParent<AnimalHealth>();
        if (animal != null)
        {
            hasHit = true;
            animal.TakeDamage(damage);
            Destroy(gameObject);
            return true;
        }

        EnemyBaseAI enemy = col.GetComponentInParent<EnemyBaseAI>();
        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
