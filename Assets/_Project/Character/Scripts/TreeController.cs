using UnityEngine;
using System.Collections;

public class TreeController : MonoBehaviour
{
    [Header("Cut")]
    public int hitsToCut = 5;
    int hits;

    [Header("Refs")]
    public Transform pivot;
    public GameObject trunk;

    [Header("Logs")]
    public GameObject logPrefab;
    public int logsCount = 3;

    [Header("Fall")]
    public float fallDuration = 1.1f;

    bool falling;
    Transform player;

    void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    // ─────────────────────────────
    public void Hit()
    {
        if (falling) return;

        hits++;

        if (hits >= hitsToCut)
            StartCoroutine(FallRoutine());
    }

    // ─────────────────────────────
    IEnumerator FallRoutine()
    {
        falling = true;

        // 🔴 ВАЖНО: ОТ игрока
        Vector3 fallDir = Vector3.forward;

        if (player)
        {
            fallDir = pivot.position - player.position;
            fallDir.y = 0f;
            fallDir.Normalize();
        }

        Quaternion startRot = pivot.rotation;
        Quaternion targetRot =
            Quaternion.LookRotation(fallDir) *
            Quaternion.Euler(90f, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallDuration;
            pivot.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        pivot.rotation = targetRot;

        SpawnLogs(fallDir);
    }

    // ─────────────────────────────
    void SpawnLogs(Vector3 fallDir)
    {
        Vector3 startPos = trunk.transform.position;
        Quaternion rot = Quaternion.LookRotation(fallDir);

        trunk.SetActive(false);

        float logLength = trunk.transform.localScale.y * 0.6f;

        for (int i = 0; i < logsCount; i++)
        {
            Vector3 pos =
                startPos +
                fallDir * (i * logLength);

            GameObject log = Instantiate(logPrefab, pos, rot);

            Rigidbody rb = log.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.mass = 35f;
                rb.linearDamping = 5f;
                rb.angularDamping = 8f;

                rb.constraints =
                    RigidbodyConstraints.FreezeRotationX |
                    RigidbodyConstraints.FreezeRotationY;
            }
        }
    }
}
