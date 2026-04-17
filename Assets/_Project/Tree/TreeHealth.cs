using System.Collections;
using System.Linq;
using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    int currentHealth;

    [Header("Segments")]
    public GameObject[] choppingSegments;
    int choppedCount = 0;

    [Header("Logs")]
    public GameObject logPrefab;
    public int logsCount = 4;

    bool fallen = false;

    void Awake()
    {
        currentHealth = maxHealth;

        // авто-сбор сегментов
        if (choppingSegments == null || choppingSegments.Length == 0)
        {
            Transform segRoot = transform.Find("_ChoppingSegments");

            if (segRoot != null)
            {
                choppingSegments = segRoot
                    .GetComponentsInChildren<Transform>(true)
                    .Where(t => t != segRoot)
                    .Select(t => t.gameObject)
                    .ToArray();
            }
        }
    }

    public void Hit(Vector3 hitterPosition)
    {
        if (fallen) return;

        currentHealth--;

        MakeHole(hitterPosition);

        if (currentHealth <= 0)
            Fall(hitterPosition);
    }

    void Fall(Vector3 hitterPosition)
    {
        if (fallen) return;
        fallen = true;

        // отключаем collider root
        Collider rootCol = GetComponent<Collider>();
        if (rootCol != null)
            rootCol.enabled = false;

        Transform falling = transform.Find("FallingTree");
        Transform segments = transform.Find("_ChoppingSegments");

        if (falling == null) return;

        if (segments != null)
            segments.gameObject.SetActive(false);

        // отделяем падающую часть
        falling.SetParent(null);

        // находим нижнюю точку
        Collider col = falling.GetComponent<Collider>();
        Vector3 pivot = col.bounds.center;
        pivot.y = col.bounds.min.y;

        // направление падения
        Vector3 dir = (falling.position - hitterPosition).normalized;
        dir.y = 0;
        if (dir == Vector3.zero)
            dir = falling.forward;

        // запускаем падение
        StartCoroutine(FallRoutine(falling, pivot, dir));
    }

    IEnumerator FallRoutine(Transform tree, Vector3 pivot, Vector3 dir)
    {
        float speed = 120f;
        float angle = 0f;

        Vector3 axis = Vector3.Cross(Vector3.up, dir).normalized;

        while (angle < 90f)
        {
            float step = speed * Time.deltaTime;
            tree.RotateAround(pivot, axis, step);
            angle += step;
            yield return null;
        }

        // включаем физику после падения
        Rigidbody rb = tree.gameObject.AddComponent<Rigidbody>();
        rb.mass = 200;

        // 🔥 ЗАПУСК ЛОМАНИЯ
        StartCoroutine(BreakAfterFall(tree.gameObject));
    }

    IEnumerator BreakAfterFall(GameObject fallingObj)
    {
        yield return new WaitForSeconds(1.2f); // даём упасть

        BreakIntoLogs(fallingObj);
    }

    void BreakIntoLogs(GameObject fallingObj)
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
    }

    void MakeHole(Vector3 hitPoint)
    {
        if (choppedCount >= choppingSegments.Length)
            return;

        int index = -1;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < choppingSegments.Length; i++)
        {
            if (!choppingSegments[i].activeSelf)
                continue;

            float dist = Vector3.Distance(
                choppingSegments[i].transform.position,
                hitPoint
            );

            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        if (index != -1)
        {
            choppingSegments[index].SetActive(false);
            choppedCount++;
        }
    }
}