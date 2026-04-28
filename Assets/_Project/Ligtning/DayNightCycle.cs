using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    public Light sun;
    public Light moon;

    [Header("Durations (seconds)")]
    public float dayLength = 1200f;   // 20 минут
    public float nightLength = 600f;  // 10 минут

    [Range(0f, 1f)] public float timeOfDay;

    [Header("Intensity")]
    public float sunIntensity = 1.2f;
    public float moonIntensity = 0.3f;
    public float minNightIntensity = 0.05f;

    [Header("Ambient Colors")]
    public Color dayAmbient = new Color(0.6f, 0.6f, 0.6f);
    public Color nightAmbient = new Color(0.02f, 0.02f, 0.08f);

    void Update()
    {
        // ⏱ расчёт цикла
        float totalCycle = dayLength + nightLength;
        float dayPortion = dayLength / totalCycle;

        // движение времени (день медленнее, ночь быстрее)
        if (timeOfDay < dayPortion)
            timeOfDay += Time.deltaTime / dayLength;
        else
            timeOfDay += Time.deltaTime / nightLength;

        if (timeOfDay >= 1f)
            timeOfDay = 0f;

        // угол вращения
        float angle = timeOfDay * 360f;

        sun.transform.rotation = Quaternion.Euler(angle - 90f, 0f, 0f);
        moon.transform.rotation = Quaternion.Euler(angle + 90f, 0f, 0f);

        // 🌅 фактор освещения (плавный переход)
        float sunDot = Vector3.Dot(sun.transform.forward, Vector3.down);
        float t = Mathf.Clamp01(sunDot);

        // ☀️ солнце
        sun.enabled = true;
        sun.intensity = Mathf.Lerp(minNightIntensity, sunIntensity, t);

        // 🌙 луна
        moon.enabled = true;
        moon.intensity = Mathf.Lerp(moonIntensity, 0f, t);

        // 🌫 тени (чтобы не было шума ночью)
        sun.shadows = t > 0.1f ? LightShadows.Soft : LightShadows.None;
        moon.shadows = t < 0.3f ? LightShadows.Soft : LightShadows.None;

        // 🌌 ambient свет (очень влияет на атмосферу)
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, t);
    }
}