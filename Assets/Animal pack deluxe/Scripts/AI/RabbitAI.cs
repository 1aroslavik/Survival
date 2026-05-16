using UnityEngine;

public class RabbitAI : AnimalBaseAI
{
    enum State { Idle, Wander, Panic }

    AnimalAudioController animalAudio;
    State currentState;
    float timer;

    protected override void Start()
    {
        base.Start();
        animalAudio = GetComponent<AnimalAudioController>();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < detectDistance)
            EnterPanic();

        switch (currentState)
        {
            case State.Idle:   Idle();   break;
            case State.Wander: Wander(); break;
            case State.Panic:  Panic();  break;
        }

        animalAudio?.TickFootsteps(agent.velocity.magnitude);
        UpdateAnimation();
    }

    void Idle()
    {
        agent.SetDestination(transform.position);
        SetBool("IsEating", true);

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (Random.value < 0.6f)
                SetState(State.Wander, Random.Range(1.5f, 3f));
            else
                SetState(State.Idle, Random.Range(2f, 4f));
        }
    }

    void Wander()
    {
        SetBool("IsEating", false);

        timer -= Time.deltaTime;

        if (agent.remainingDistance < 0.5f)
            MoveRandom(2f, 5f);

        if (timer <= 0)
            SetState(State.Idle, Random.Range(2f, 5f));
    }

    void Panic()
    {
        SetBool("IsEating", false);

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SetState(State.Idle, Random.Range(3f, 6f));
            return;
        }

        if (agent.remainingDistance < 1f)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            Vector3 target = transform.position + dir * Random.Range(8f, 15f);
            agent.speed = 7f;
            agent.SetDestination(target);
        }
    }

    void EnterPanic()
    {
        if (currentState == State.Panic) return;
        animalAudio?.OnFlee();
        SetState(State.Panic, Random.Range(3f, 6f));
    }

    void SetState(State newState, float newTimer)
    {
        currentState = newState;
        timer = newTimer;
    }
}