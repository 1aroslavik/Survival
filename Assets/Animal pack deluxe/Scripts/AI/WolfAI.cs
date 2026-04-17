using UnityEngine;

public class WolfAI : AnimalBaseAI
{
    public float attackDistance = 2f;

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectDistance)
        {
            agent.speed = 4f;
            agent.SetDestination(player.position);

            SetBool("IsHowls", dist > 5f);

            if (dist < attackDistance)
                SetBool("IsAttacking", true);
            else
                SetBool("IsAttacking", false);
        }
        else
        {
            SetBool("IsAttacking", false);
            SetBool("IsHowls", false);

            if (!agent.hasPath || agent.remainingDistance < 1f)
                MoveRandom();
        }

        UpdateAnimation();
    }
}