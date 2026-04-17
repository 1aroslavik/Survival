using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    public PlayerStats stats;

    public Image health;
    public Image hunger;
    public Image thirst;
    public Image stamina;

    void Update()
    {
        if (!stats) return;

        if (health) health.fillAmount = stats.health / stats.maxHealth;
        if (hunger) hunger.fillAmount = stats.hunger / stats.maxHunger;
        if (thirst) thirst.fillAmount = stats.thirst / stats.maxThirst;
        if (stamina) stamina.fillAmount = stats.stamina / stats.maxStamina;
    }
}
