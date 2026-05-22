using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [HideInInspector] public bool isWalking;
    [HideInInspector] public bool isSprinting;

    [Header("Stamina ↔ Thirst")]
    public float thirstCostPerStamina = 0.08f;

    // ================= MAX VALUES =================
    [Header("Death")]
    public GameObject deathScreen;
    [Header("MAX VALUES")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxStamina = 100f;

    [Header("Radiation")]
    public float radiation = 0f;
    public float maxRadiation = 100f;

    [Tooltip("Сколько здоровья теряется от максимума при 100 радиации")]
    public float radiationHealthPenalty = 70f;

    // ================= CURRENT =================

    [Header("CURRENT")]
    public float health;
    public float hunger;
    public float thirst;
    public float stamina;

    // ================= DRAIN =================

    [Header("Needs Drain (per second)")]

    // Голод уходит медленно (~1 игровой день)
    public float hungerDrain = 0.025f;

    // Жажда уходит быстрее
    public float thirstDrain = 0.045f;

    // Восстановление стамины
    public float staminaRegen = 12f;

    [Header("Stamina Drain")]
    public float sprintStaminaCostPerSecond = 9f;

    // ================= DAMAGE =================

    [Header("Damage")]
    public float starvationDamage = 4f;
    public float dehydrationDamage = 8f;

    [Header("Radiation Damage")]
    public float radiationDamageThreshold = 70f;
    public float radiationDamagePerSecond = 3f;

    // ================= THRESHOLDS =================

    [Header("Thresholds")]
    public float lowValue = 20f;

    [HideInInspector] public bool isDead;

    // =========================================================

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
        DrainStamina();
        RegenerateStamina();
        ApplyHealthDamage();
        ApplyRadiationEffects();
        ClampStats();
        CheckDeath();
    }

    // ================= CORE =================

    void DrainNeeds()
    {
        hunger -= hungerDrain * Time.deltaTime;

        // При беге жажда растет сильнее
        float thirstMultiplier = isSprinting ? 2f : 1f;

        thirst -= thirstDrain * thirstMultiplier * Time.deltaTime;
    }

    void DrainStamina()
    {
        if (!isSprinting) return;

        if (stamina <= 0f)
        {
            stamina = 0f;
            return;
        }

        float staminaCost = sprintStaminaCostPerSecond;

        // Если голод/жажда низкие — устаем быстрее
        if (hunger <= lowValue)
            staminaCost *= 1.25f;

        if (thirst <= lowValue)
            staminaCost *= 1.4f;

        stamina -= staminaCost * Time.deltaTime;
    }

    void RegenerateStamina()
    {
        if (isSprinting) return;
        if (thirst <= 0f) return;
        if (stamina >= maxStamina) return;

        float multiplier = 1f;

        if (hunger <= lowValue)
            multiplier *= 0.65f;

        if (thirst <= lowValue)
            multiplier *= 0.5f;

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
    }

    void ApplyHealthDamage()
    {
        // Голод
        if (hunger <= 0f)
        {
            health -= starvationDamage * Time.deltaTime;
        }

        // Жажда
        if (thirst <= 0f)
        {
            health -= dehydrationDamage * Time.deltaTime;
        }
    }

    void ApplyRadiationEffects()
    {
        // Ограничение максимального HP
        float currentMaxHealth = GetCurrentMaxHealth();

        if (health > currentMaxHealth)
        {
            health = Mathf.MoveTowards(
                health,
                currentMaxHealth,
                15f * Time.deltaTime
            );
        }

        // Урон от сильной радиации
        if (radiation >= radiationDamageThreshold)
        {
            health -= radiationDamagePerSecond * Time.deltaTime;
        }
    }

    float GetCurrentMaxHealth()
    {
        float hpPenalty =
            (radiation / maxRadiation) * radiationHealthPenalty;

        return Mathf.Clamp(
            maxHealth - hpPenalty,
            10f,
            maxHealth
        );
    }

    void ClampStats()
    {
        float currentMaxHealth = GetCurrentMaxHealth();

        health = Mathf.Clamp(health, 0, currentMaxHealth);

        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        radiation = Mathf.Clamp(radiation, 0, maxRadiation);
    }

    void CheckDeath()
    {
        if (health <= 0 && !isDead)
        {
            isDead = true;

            Debug.Log("PLAYER DEAD");

            // показать окно смерти
            if (deathScreen != null)
            {
                deathScreen.SetActive(true);
            }

            // остановить игру
            Time.timeScale = 0f;

            // показать курсор
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
    }

    public bool HasStamina()
    {
        return stamina > 0f;
    }

    public void Eat(float amount)
    {
        hunger += amount;
    }

    public void Drink(float amount)
    {
        thirst += amount;
    }

    public void Heal(float amount)
    {
        health += amount;

        // нельзя хилиться выше лимита от радиации
        health = Mathf.Clamp(
            health,
            0,
            GetCurrentMaxHealth()
        );
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
    }

    // ================= RADIATION =================

    public void AddRadiation(float amount)
    {
        radiation += amount;
    }

    public void RemoveRadiation(float amount)
    {
        radiation -= amount;
    }

    public float GetCurrentMaxHP()
    {
        return GetCurrentMaxHealth();
    }
}