using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Volume))]
public class RadiationSource : MonoBehaviour
{
    public string targetTag = "Player";

    private Volume volume;

    void Awake()
    {
        // получаем Volume с этого же объекта
        volume = GetComponent<Volume>();

        // настраиваем коллайдер
        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;

        // выключено по умолчанию
        if (volume != null)
            volume.weight = 0f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        Debug.Log("☢️ ВОШЕЛ В ЗОНУ");

        if (volume != null)
            volume.weight = 1f; // включили эффект
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        Debug.Log("🚪 ВЫШЕЛ ИЗ ЗОНЫ");

        if (volume != null)
            volume.weight = 0f; // выключили эффект
    }
}