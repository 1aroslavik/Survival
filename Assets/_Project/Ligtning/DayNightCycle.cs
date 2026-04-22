using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public Light moon;

    [Header("Durations (seconds)")]
    public float dayLength = 1200f;   // 20 минут
    public float nightLength = 600f;  // 10 минут

    [Range(0f, 1f)] public float timeOfDay;

    public float sunIntensity = 100f;
    public float moonIntensity = 5f;

    [Header("Anti Dark")]
    public float minNightIntensity = 0.2f;

    void Update()
    {
        float totalCycle = dayLength + nightLength;

        // ⏱ движение времени (разная скорость)
        float dayPortion = dayLength / totalCycle;

        if (timeOfDay < dayPortion)
        {
            // ☀️ ДЕНЬ (медленнее)
            timeOfDay += Time.deltaTime / dayLength;
        }
        else
        {
            // 🌙 НОЧЬ (быстрее)
            timeOfDay += Time.deltaTime / nightLength;
        }

        if (timeOfDay >= 1f) timeOfDay = 0f;

        float angle = timeOfDay * 360f;

        // вращение
        sun.transform.rotation = Quaternion.Euler(angle - 90f, 0f, 0f);
        moon.transform.rotation = Quaternion.Euler(angle + 90f, 0f, 0f);

        bool isDay = timeOfDay < dayPortion;

        if (isDay)
        {
            // ☀️ день
            sun.enabled = true;
            sun.intensity = sunIntensity;
            sun.shadows = LightShadows.Soft;

            moon.enabled = false;
            moon.shadows = LightShadows.None;
        }
        else
        {
            // 🌙 ночь
            moon.enabled = true;
            moon.intensity = Mathf.Max(moonIntensity, minNightIntensity);
            moon.shadows = LightShadows.Soft;

            // слабый фон от солнца (анти-чернота)
            sun.enabled = true;
            sun.intensity = minNightIntensity;
            sun.shadows = LightShadows.None;
        }
    }
}