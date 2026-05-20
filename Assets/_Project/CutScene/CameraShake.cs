using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Обычная тряска (полёт)")]
    public float amplitude = 0.05f;
    public float frequency = 3f;

    [Header("Тряска при краше")]
    public float crashAmplitude = 0.4f;
    public float crashFrequency = 8f;

    private Vector3 startPos;
    private bool isCrashing = false;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float currentAmplitude = isCrashing ? crashAmplitude : amplitude;
        float currentFrequency = isCrashing ? crashFrequency : frequency;

        float x = (Mathf.PerlinNoise(Time.time * currentFrequency, 0f) - 0.5f) * currentAmplitude;
        float y = (Mathf.PerlinNoise(0f, Time.time * currentFrequency) - 0.5f) * currentAmplitude;

        transform.localPosition = startPos + new Vector3(x, y, 0f);
    }

    // Вызови это из CutsceneManager при краше
    public void TriggerCrashShake()
    {
        isCrashing = true;
    }
}