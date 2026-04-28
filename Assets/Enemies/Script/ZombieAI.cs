using UnityEngine;

public class ZombieAI : EnemyBaseAI
{
    protected override void Start()
    {
        base.Start();

        maxHealth = 80f;
        currentHealth = maxHealth;

        damage = 15f;
        runSpeed = 3f;
    }
}