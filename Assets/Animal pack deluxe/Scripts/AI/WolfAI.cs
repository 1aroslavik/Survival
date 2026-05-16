using UnityEngine;

public class WolfAI : AnimalBaseAI
{
    public float attackDistance = 2f;

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
            agent.speed = 4f;
            agent.SetDestination(player.position);

            if (!wasChasing)             animalAudio?.OnDetectPlayer();
            if (attacking && !wasAttacking) animalAudio?.OnAttack();

            SetBool("IsHowls", dist > 5f);
            SetBool("IsAttacking", attacking);
        }
        else
        {
            SetBool("IsAttacking", false);
            SetBool("IsHowls", false);
            if (!agent.hasPath || agent.remainingDistance < 1f)
                MoveRandom();
        }

        animalAudio?.TickFootsteps(agent.velocity.magnitude);
        wasChasing   = chasing;
        wasAttacking = attacking;
        UpdateAnimation();
    }
}