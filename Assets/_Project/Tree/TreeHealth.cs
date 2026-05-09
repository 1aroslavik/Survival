using System.Collections;
using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    int currentHealth;
    [Header("Stump")]
    public int stumpHealth = 5;

    bool stumpDestroyed = false;

    [Header("HP Indicator")]
    public GameObject[] hpSegments;
    public Material healthyMaterial;
    public Material damagedMaterial;
    [Header("Effects")]
    public AudioClip breakSound;
    public AudioClip fallImpactSound;

    public GameObject fallEffect;
    [Header("Logs")]
    public GameObject logPrefab;
    public int logsCount = 4;

    [Header("References")]
    public Transform fallingTree;

    [Header("Regrow")]
    public float regrowTime = 6000f;

    TreeReplacer replacer;
    Vector3 treePosition;

    bool fallen = false;

    void Awake()
    {
        currentHealth = maxHealth;

        // 🔥 поиск FallingTree
        if (fallingTree == null)
        {
            foreach (Transform t in GetComponentsInChildren<Transform>(true))
            {
                if (t.name.Contains("FallingTree"))
                {
                    fallingTree = t;
                    break;
                }
            }
        }

        // 🔥 поиск сегментов
        Transform hpRoot = transform.Find("hp");

        if (hpRoot != null)
        {
            hpSegments = new GameObject[hpRoot.childCount];

            for (int i = 0; i < hpRoot.childCount; i++)
            {
                hpSegments[i] = hpRoot.GetChild(i).gameObject;
            }

            // сортировка по имени (важно!)
            System.Array.Sort(hpSegments, (a, b) => a.name.CompareTo(b.name));
        }

        replacer = FindObjectOfType<TreeReplacer>();
        treePosition = transform.position;
    }

    public void Hit(Vector3 hitterPosition)
{
    // 🌲 дерево
    if (!fallen)
    {
        currentHealth--;

        UpdateHPVisual();

        if (currentHealth <= 0)
        {
            HideIndicator();
            Fall(hitterPosition);
        }

        return;
    }

    // 🪵 пень
    if (!stumpDestroyed)
    {
        stumpHealth--;

        Debug.Log("STUMP HIT: " + stumpHealth);

        if (stumpHealth <= 0)
        {
            DestroyStump();
        }
    }
}

    void UpdateHPVisual()
    {
        if (hpSegments == null || hpSegments.Length == 0) return;

        int damagedCount = maxHealth - currentHealth;

        for (int i = 0; i < hpSegments.Length; i++)
        {
            Renderer r = hpSegments[i].GetComponent<Renderer>();
            if (r == null) continue;

            if (i < damagedCount)
                r.material = damagedMaterial;
            else
                r.material = healthyMaterial;
        }
    }

    void HideIndicator()
    {
        foreach (var seg in hpSegments)
        {
            seg.SetActive(false);
        }
    }

    void Fall(Vector3 hitterPosition)
    {
        if (fallen) return;
        fallen = true;
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(
                breakSound,
                transform.position,
                1f
            );
        }
        if (fallingTree == null) return;

        Collider rootCol = GetComponent<Collider>();
        if (rootCol != null)
            rootCol.enabled = false;

        fallingTree.SetParent(null);

        GameObject obj = fallingTree.gameObject;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.mass = 120f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationY;

        Vector3 dir = (fallingTree.position - hitterPosition).normalized;
        dir.y = 0;

        if (dir == Vector3.zero)
            dir = transform.forward;

        rb.AddForce(dir * 8f, ForceMode.Impulse);

        StartCoroutine(WaitUntilStop(obj));
    }

    IEnumerator WaitUntilStop(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        yield return new WaitForSeconds(0.3f);

        while (true)
        {
            if (rb.linearVelocity.magnitude < 0.3f)
            {
                SpawnFallImpact(obj.transform.position);

                BreakIntoLogs(obj);
                yield break;
            }

            yield return null;
        }
    }
    void SpawnFallImpact(Vector3 pos)
    {
        // 💥 пыль
        if (fallEffect != null)
        {
            Instantiate(fallEffect, pos, Quaternion.identity);
        }

        // 🔊 удар о землю
        if (fallImpactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                fallImpactSound,
                pos,
                1f
            );
        }
    }
    public void BreakIntoLogs(GameObject fallingObj)
    {
        Vector3 startPos = fallingObj.transform.position;
        Vector3 direction = fallingObj.transform.up;

        float spacing = 1.2f;

        for (int i = 0; i < logsCount; i++)
        {
            Vector3 pos = startPos + direction * (i * spacing);
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, direction);

            Instantiate(logPrefab, pos, rot);
        }

        Destroy(fallingObj);
        StartCoroutine(RegrowTree());
    }
    
    void DestroyStump()
    {
        stumpDestroyed = true;

        // можно добавить эффект
        if (fallEffect != null)
        {
            Instantiate(fallEffect, transform.position, Quaternion.identity);
        }

        // звук
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(
                breakSound,
                transform.position,
                1f
            );
        }

        StopAllCoroutines();

        Destroy(gameObject);
    }
    IEnumerator RegrowTree()
    {
        yield return new WaitForSeconds(regrowTime);

        if (stumpDestroyed)
            yield break;

        Destroy(gameObject);

        if (replacer != null)
        {
            replacer.RestoreTree(treePosition);
        }
    }

}