using UnityEngine;

public class WaterSourceInfinite : MonoBehaviour, IWaterSource
{
    [Tooltip("Сколько воды в секунду")]
    public float drinkRate = 20f;

    public bool CanDrink()
    {
        return true;
    }

    public float GetDrinkRate()
    {
        return drinkRate;
    }
}
