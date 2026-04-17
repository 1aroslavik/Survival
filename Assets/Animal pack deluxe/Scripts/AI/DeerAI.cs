using UnityEngine;

public class DeerAI : AnimalBaseAI
{
    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectDistance)
        {
            if (Random.value < 0.7f)
            {
                // убегает
                Vector3 dir = (transform.position - player.position).normalized;
                agent.speed = 5f;
                agent.SetDestination(transform.position + dir * 10f);

                SetBool("IsEating", false);
            }
            else
            {
                // смотрит
                agent.SetDestination(transform.position);

                Vector3 lookDir = (player.position - transform.position).normalized;
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 2f
                );

                SetBool("IsEating", false);
            }
        }
        else
        {
            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                MoveRandom();
                SetBool("IsEating", Random.value < 0.3f);
            }
        }

        UpdateAnimation();
    }
}