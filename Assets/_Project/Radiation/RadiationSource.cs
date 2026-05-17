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

    private GameObject radiationIcon;

    void Awake()
    {
        volume = GetComponent<Volume>();

        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        if (volume != null)
            volume.weight = 0f;

        radiationIcon = GameObject.Find("RadiationIcon");

        if (radiationIcon != null)
            radiationIcon.SetActive(false);
    }

    void Update()
    {
        if (volume != null)
        {
            float targetWeight = playerInside ? 1f : 0f;

            volume.weight = Mathf.MoveTowards(
                volume.weight,
                targetWeight,
                effectSmoothSpeed * Time.deltaTime
            );
        }

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

        if (radiationIcon != null)
            radiationIcon.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        playerInside = false;
        playerStats = null;

        if (radiationIcon != null)
            radiationIcon.SetActive(false);
    }
}