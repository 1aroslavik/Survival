using UnityEngine;

public class DryingMeat : MonoBehaviour
{
    public float dryTime = 20f;
    public GameObject driedMeatPrefab;

    [Header("State")]
    public bool isOnDryer = false; // 🔥 ГЛАВНОЕ

    private float timer;
    private bool replaced;

    void Update()
    {
        // ❗ ЕСЛИ НЕ НА СУШИЛКЕ — НЕ СУШИМ
        if (!isOnDryer) return;

        if (replaced) return;

        timer += Time.deltaTime;

        if (timer >= dryTime)
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