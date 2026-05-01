using System.Collections;
using UnityEngine;

public class TreeHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    int currentHealth;

    [Header("Logs")]
    public GameObject logPrefab;
    public int logsCount = 4;

    [Header("References")]
    public Transform fallingTree; // перетащи сюда FallingTree

    [Header("Fall Settings")]
    public float pushForce = 8f;     // сила толчка
    public float breakDelay = 2.2f;  // через сколько ломать на бревна

    bool fallen = false;

    void Awake()
    {
        currentHealth = maxHealth;

        // fallback-поиск
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

        if (fallingTree == null)
            Debug.LogError("[Tree] FallingTree not assigned!");
    }

    public void Hit(Vector3 hitterPosition)
    {
        if (fallen) return;

        currentHealth--;

        if (currentHealth <= 0)
            Fall(hitterPosition);
    }

    void Fall(Vector3 hitterPosition)
    {
        if (fallen) return;
        fallen = true;

        if (fallingTree == null) return;

        // отключаем root collider
        Collider rootCol = GetComponent<Collider>();
        if (rootCol != null)
            rootCol.enabled = false;

        // отделяем падающую часть
        fallingTree.SetParent(null);

        // Rigidbody
        Rigidbody rb = fallingTree.gameObject.AddComponent<Rigidbody>();
        rb.mass = 200f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // ищем collider для низа
        Collider col = fallingTree.GetComponentInChildren<Collider>();
        if (col == null)
        {
            Debug.LogError("[Tree] No collider on FallingTree!");
            return;
        }

        // 🔥 ТОЧКА ОПОРЫ (низ ствола)
        Vector3 pivot = col.bounds.center;
        pivot.y = col.bounds.min.y;

        // создаём якорь (невидимый)
        GameObject anchor = new GameObject("TreePivot");
        anchor.transform.position = pivot;

        Rigidbody anchorRb = anchor.AddComponent<Rigidbody>();
        anchorRb.isKinematic = true;

        // 🔥 шарнир — как в The Forest
        HingeJoint joint = fallingTree.gameObject.AddComponent<HingeJoint>();
        joint.connectedBody = anchorRb;

        // локальная точка шарнира
        joint.anchor = fallingTree.InverseTransformPoint(pivot);

        // ось вращения (перпендикуляр к направлению удара)
        Vector3 dir = (fallingTree.position - hitterPosition).normalized;
        dir.y = 0;

        if (dir == Vector3.zero)
            dir = fallingTree.forward;

        Vector3 axis = Vector3.Cross(Vector3.up, dir).normalized;
        joint.axis = axis;

        // 🔥 толчок
        rb.AddForce(dir * pushForce, ForceMode.Impulse);

        StartCoroutine(BreakAfterFall(fallingTree.gameObject, anchor));
    }

    IEnumerator BreakAfterFall(GameObject fallingObj, GameObject anchor)
    {
        yield return new WaitForSeconds(breakDelay);

        BreakIntoLogs(fallingObj);

        Destroy(anchor);
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
}