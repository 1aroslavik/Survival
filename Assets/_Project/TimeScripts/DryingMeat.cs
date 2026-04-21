using UnityEngine;

public class DryingMeat : MonoBehaviour
{
    [Header("Dry Time Settings")]
    public float dryerTime = 20f;   // 🪵 сушилка
    public float fireTime = 8f;     // 🔥 костёр

    public GameObject driedMeatPrefab;

    [Header("State")]
    public bool isOnDryer = false;
    public bool isOnFire = false;

    private float timer;
    private bool replaced;

    void Update()
    {
        if (replaced) return;

        // ❌ нигде не находится
        if (!isOnDryer && !isOnFire) return;

        timer += Time.deltaTime;

        float currentDryTime = isOnFire ? fireTime : dryerTime;

        if (timer >= currentDryTime)
        {
            Replace();
        }
    }

    void Replace()
    {
        replaced = true;

        GameObject dried = Instantiate(
            driedMeatPrefab,
            transform.position,
            transform.rotation,
            transform.parent
        );

        dried.transform.localScale = transform.localScale;

        Destroy(gameObject);
    }
}