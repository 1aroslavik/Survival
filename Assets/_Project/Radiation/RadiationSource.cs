using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Volume))]
public class RadiationSource : MonoBehaviour
{
    [Header("Target")]
    public string targetTag = "Player";

    [Header("Radiation")]
    public float radiationPerSecond = 4f;

    [Tooltip("Насколько быстро появляется эффект")]
    public float effectSmoothSpeed = 2f;

    private Volume volume;
    private bool playerInside;

    private PlayerStats playerStats;

    void Awake()
    {
        volume = GetComponent<Volume>();

        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        if (volume != null)
            volume.weight = 0f;
    }

    void Update()
    {
        // Плавное включение/выключение эффекта
        if (volume != null)
        {
            float targetWeight = playerInside ? 1f : 0f;

            volume.weight = Mathf.MoveTowards(
                volume.weight,
                targetWeight,
                effectSmoothSpeed * Time.deltaTime
            );
        }

        // Накопление радиации
        if (playerInside && playerStats != null)
        {
            playerStats.AddRadiation(
                radiationPerSecond * Time.deltaTime
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        playerInside = true;

        playerStats = other.GetComponent<PlayerStats>();

        Debug.Log("☢️ ВОШЕЛ В РАДИАЦИОННУЮ ЗОНУ");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        playerInside = false;
        playerStats = null;

        Debug.Log("🚪 ВЫШЕЛ ИЗ РАДИАЦИОННОЙ ЗОНЫ");
    }
}