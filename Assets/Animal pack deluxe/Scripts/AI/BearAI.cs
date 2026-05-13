using UnityEngine;

public class BearAI : AnimalBaseAI
{
    public float attackDistance = 2.5f;

    AnimalAudioController animalAudio;
    bool wasChasing;
    bool wasAttacking;

    protected override void Start()
    {
        base.Start();
        animalAudio = GetComponent<AnimalAudioController>();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool chasing   = dist < detectDistance;
        bool attacking = chasing && dist < attackDistance;

        if (chasing)
        {
            agent.speed = 2.5f;
            agent.SetDestination(player.position);

            if (!wasChasing)             animalAudio?.OnDetectPlayer();
            if (attacking && !wasAttacking) animalAudio?.OnAttack();

            SetBool("IsAttacking", attacking);
        }
        else
        {
            SetBool("IsAttacking", false);
            if (!agent.hasPath || agent.remainingDistance < 1f)
                MoveRandom();
        }

        animalAudio?.TickFootsteps(agent.velocity.magnitude);
        wasChasing   = chasing;
        wasAttacking = attacking;
        UpdateAnimation();
    }
}