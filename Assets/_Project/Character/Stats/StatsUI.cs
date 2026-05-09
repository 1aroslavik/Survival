using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public PlayerStats stats;

    [Header("Bars")]
    public Image health;
    public Image hunger;
    public Image thirst;
    public Image stamina;

    [Header("Texts")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI hungerText;
    public TextMeshProUGUI thirstText;
    public TextMeshProUGUI staminaText;

    void Update()
    {
        if (!stats) return;

        // HP
        if (health)
            health.fillAmount = stats.health / stats.maxHealth;

        if (healthText)
            healthText.text = $"{Mathf.RoundToInt(stats.health)}/{Mathf.RoundToInt(stats.maxHealth)}";

        // Hunger
        if (hunger)
            hunger.fillAmount = stats.hunger / stats.maxHunger;

        if (hungerText)
            hungerText.text = $"{Mathf.RoundToInt(stats.hunger)}/{Mathf.RoundToInt(stats.maxHunger)}";

        // Thirst
        if (thirst)
            thirst.fillAmount = stats.thirst / stats.maxThirst;

        if (thirstText)
            thirstText.text = $"{Mathf.RoundToInt(stats.thirst)}/{Mathf.RoundToInt(stats.maxThirst)}";

        // Stamina
        if (stamina)
            stamina.fillAmount = stats.stamina / stats.maxStamina;

        if (staminaText)
            staminaText.text = $"{Mathf.RoundToInt(stats.stamina)}/{Mathf.RoundToInt(stats.maxStamina)}";
    }
}