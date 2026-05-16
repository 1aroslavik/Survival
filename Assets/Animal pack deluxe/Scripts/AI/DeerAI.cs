using UnityEngine;

public class DeerAI : AnimalBaseAI
{
    AnimalAudioController animalAudio;
    bool wasFleeing;

    protected override void Start()
    {
        base.Start();
        animalAudio = GetComponent<AnimalAudioController>();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool fleeing = false;

        if (dist < detectDistance)
        {
            if (Random.value < 0.7f)
            {
                fleeing = true;
                Vector3 dir = (transform.position - player.position).normalized;
                agent.speed = 5f;
                agent.SetDestination(transform.position + dir * 10f);
                SetBool("IsEating", false);
            }
            else
            {
                if (!wasFleeing) animalAudio?.OnDetectPlayer();
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

        if (fleeing && !wasFleeing) animalAudio?.OnFlee();
        animalAudio?.TickFootsteps(agent.velocity.magnitude);
        wasFleeing = fleeing;
        UpdateAnimation();
    }
}