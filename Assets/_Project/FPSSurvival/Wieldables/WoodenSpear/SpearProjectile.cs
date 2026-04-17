using UnityEngine;

public class SpearProjectile : MonoBehaviour
{
    public float damage = 40f;

    void OnTriggerEnter(Collider col)
    {
        AnimalHealth animal = col.GetComponentInParent<AnimalHealth>();

        if (animal != null)
        {
            animal.TakeDamage(damage);
            Destroy(gameObject); // 🔥 копьё исчезает
        }
    }
}