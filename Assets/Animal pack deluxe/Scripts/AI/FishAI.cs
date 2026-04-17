using UnityEngine;

public class FishAI : MonoBehaviour
{
    public float speed = 2f;
    public float turnSpeed = 2f;

    private Transform zone;
    private float radius;

    private Vector3 targetPos;
    private Animator anim;

    public void Init(Transform zoneTransform, float zoneRadius)
    {
        zone = zoneTransform;
        radius = zoneRadius;

        anim = GetComponent<Animator>();

        PickNewTarget();
    }

    void Update()
    {
        if (zone == null) return;

        Vector3 dir = (targetPos - transform.position).normalized;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);

        transform.position += transform.forward * speed * Time.deltaTime;

        if (anim != null)
            anim.SetFloat("Speed", speed);

        if (Vector3.Distance(transform.position, targetPos) < 1f)
            PickNewTarget();
    }

    void PickNewTarget()
    {
        Vector2 circle = Random.insideUnitCircle * radius;

        float waterY = zone.position.y;

        targetPos = new Vector3(
            zone.position.x + circle.x,
            Random.Range(waterY - 3f, waterY - 1f),
            zone.position.z + circle.y
        );

        speed = Random.Range(1.5f, 3f);
    }
}