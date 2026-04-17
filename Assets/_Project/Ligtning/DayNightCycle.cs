using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;

    [Range(0, 24)]
    public float timeOfDay = 12f;

    public float dayDuration = 300f; // 5 минут сутки

    public Gradient lightColor;

    public AnimationCurve intensityCurve;

    void Update()
    {
        timeOfDay += Time.deltaTime * (24f / dayDuration);

        if (timeOfDay >= 24f)
            timeOfDay = 0f;

        UpdateSun();
    }

    void UpdateSun()
    {
        float normalizedTime = timeOfDay / 24f;

        // вращение солнца
        float angle = normalizedTime * 360f - 90f;
        sun.transform.rotation = Quaternion.Euler(angle, 170f, 0);

        // цвет
        sun.color = lightColor.Evaluate(normalizedTime);

        // интенсивность (очень важно!)
        sun.intensity = intensityCurve.Evaluate(normalizedTime) * 100000f;
    }
}