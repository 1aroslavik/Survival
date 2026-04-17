using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isSprinting;

    [Header("Stamina ↔ Thirst")]
    public float thirstCostPerStamina = 0.3f;

    [Header("MAX VALUES")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxStamina = 100f;

    [Header("CURRENT")]
    public float health;
    public float hunger;
    public float thirst;
    public float stamina;

    [Header("DRAIN (per second)")]
    public float hungerDrain = 0.15f;
    public float thirstDrain = 0.35f;
    public float staminaRegen = 18f;

    [Header("Stamina Drain")]
    public float sprintStaminaCostPerSecond = 20f;

    [Header("DAMAGE")]
    public float starvationDamage = 6f;

    [Header("THRESHOLDS")]
    public float lowValue = 20f;

    bool isDead;

    void Start()
    {
        health = maxHealth;
        hunger = maxHunger;
        thirst = maxThirst;
        stamina = maxStamina;
    }

    void Update()
    {
        if (isDead) return;

        DrainNeeds();
        DrainStamina(); // 🔥 добавлено
        RegenerateStamina();
        ApplyHealthDamage();
        CheckDeath();
    }

    // ================= CORE =================

    void DrainNeeds()
    {
        hunger -= hungerDrain * Time.deltaTime;
        thirst -= thirstDrain * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
    }

    // 🔥 НОВОЕ — расход при беге
    void DrainStamina()
    {
        if (!isSprinting) return;

        if (stamina <= 0f)
        {
            stamina = 0f;
            return;
        }

        stamina -= sprintStaminaCostPerSecond * Time.deltaTime;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    void RegenerateStamina()
    {
        if (isSprinting) return;
        if (thirst <= 0f) return;
        if (stamina >= maxStamina) return;

        float multiplier = 1f;

        if (hunger <= lowValue) multiplier *= 0.5f;

        float staminaGain = staminaRegen * multiplier * Time.deltaTime;
        float thirstNeeded = staminaGain * thirstCostPerStamina;

        if (thirst < thirstNeeded)
        {
            staminaGain = thirst / thirstCostPerStamina;
            thirst = 0f;
        }
        else
        {
            thirst -= thirstNeeded;
        }

        stamina += staminaGain;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    void ApplyHealthDamage()
    {
        if (hunger <= 0 || thirst <= 0)
        {
            health -= starvationDamage * Time.deltaTime;
            health = Mathf.Clamp(health, 0, maxHealth);
        }
    }

    void CheckDeath()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;
            Debug.Log("PLAYER DEAD");
        }
    }

    // ================= PUBLIC API =================

    public bool CanUseStamina(float cost)
    {
        return stamina >= cost;
    }

    public void UseStamina(float cost)
    {
        stamina -= cost;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
    }

    public bool HasStamina()
    {
        return stamina > 0f;
    }

    public void Eat(float amount)
    {
        hunger += amount;
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
    }

    public void Drink(float amount)
    {
        thirst += amount;
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
    }

    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}