using UnityEngine;

public class BearAI : AnimalBaseAI
{
    public float attackDistance = 2.5f;

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectDistance)
        {
            agent.speed = 2.5f;
            agent.SetDestination(player.position);

            if (dist < attackDistance)
                SetBool("IsAttacking", true);
            else
                SetBool("IsAttacking", false);
        }
        else
        {
            SetBool("IsAttacking", false);

            if (!agent.hasPath || agent.remainingDistance < 1f)
                MoveRandom();
        }

        UpdateAnimation();
    }
}