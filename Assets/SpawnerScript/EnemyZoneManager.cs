using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Родительский менеджер зон спавна врагов.
/// Держит на карте фиксированное количество активных зон (targetActiveZones).
/// Когда игрок зачищает зону (все враги мертвы) — через replaceDelay она удаляется,
/// и вместо неё появляется новая в другой точке (из anchorPoints или случайной).
///
/// Настройка:
/// 1. Создай префаб EnemySpawnZone со всеми настройками (spawnCount, префабы врагов, радиусы).
///    Важно: в префабе зоны поставь respawnAllDeadCooldown = -1, чтобы она сама не респавнила —
///    этим занимается менеджер.
/// 2. Назначь его в поле zoneTemplate.
/// 3. Либо расставь Transform'ы-якоря по карте и закинь их в anchorPoints,
///    либо просто задай randomAreaRadius — менеджер сам найдёт место.
/// </summary>
public class EnemyZoneManager : MonoBehaviour
{
    [Header("Zone Template")]
    [Tooltip("Префаб с компонентом EnemySpawnZone. Все его настройки будут применяться к создаваемым зонам.")]
    public EnemySpawnZone zoneTemplate;

    [Header("Active Zones")]
    [Tooltip("Сколько зон одновременно держим на карте.")]
    public int targetActiveZones = 4;
    [Tooltip("Задержка перед заменой зачищенной зоны на новую.")]
    public float replaceDelay = 5f;

    [Header("Placement — Anchor Points (приоритет)")]
    [Tooltip("Возможные точки спавна зон. Менеджер берёт случайные свободные. Можно оставить пустым.")]
    public List<Transform> anchorPoints = new List<Transform>();

    [Header("Placement — Random Area (fallback / если anchorPoints пуст)")]
    public float randomAreaRadius = 200f;
    public LayerMask groundMask = ~0;
    public int randomPlacementAttempts = 20;

    [Header("Constraints")]
    [Tooltip("Минимальная дистанция между центрами зон.")]
    public float minDistanceBetweenZones = 40f;
    [Tooltip("Минимальная дистанция до игрока при появлении новой зоны (чтобы не спавнилась на глазах).")]
    public float minDistanceFromPlayer = 50f;
    public string playerTag = "Player";

    readonly List<EnemySpawnZone> activeZones = new List<EnemySpawnZone>();
    readonly Dictionary<EnemySpawnZone, float> clearedAt = new Dictionary<EnemySpawnZone, float>();
    readonly HashSet<Transform> usedAnchors = new HashSet<Transform>();
    Transform player;

    void Start()
    {
        AcquirePlayer();
        for (int i = 0; i < targetActiveZones; i++) TryCreateZone();
    }

    void Update()
    {
        if (player == null) AcquirePlayer();

        for (int i = activeZones.Count - 1; i >= 0; i--)
        {
            var z = activeZones[i];
            if (z == null) { activeZones.RemoveAt(i); continue; }
            if (z.IsCleared && !clearedAt.ContainsKey(z))
                clearedAt[z] = Time.time;
        }

        if (clearedAt.Count > 0)
        {
            var toRemove = new List<EnemySpawnZone>();
            foreach (var kv in clearedAt)
                if (Time.time - kv.Value >= replaceDelay) toRemove.Add(kv.Key);

            foreach (var z in toRemove)
            {
                clearedAt.Remove(z);
                RemoveZone(z);
            }
        }

        int guard = 0;
        while (activeZones.Count < targetActiveZones && guard++ < targetActiveZones)
        {
            if (!TryCreateZone()) break;
        }
    }

    void AcquirePlayer()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null) player = go.transform;
    }

    bool TryCreateZone()
    {
        if (zoneTemplate == null) return false;
        if (!TryFindPosition(out Vector3 pos, out Transform anchor)) return false;

        var inst = Instantiate(zoneTemplate, pos, Quaternion.identity, transform);
        inst.gameObject.SetActive(true);
        activeZones.Add(inst);
        if (anchor != null) usedAnchors.Add(anchor);
        return true;
    }

    void RemoveZone(EnemySpawnZone zone)
    {
        activeZones.Remove(zone);

        Transform freed = null;
        foreach (var a in usedAnchors)
        {
            if (a != null && zone != null && Vector3.Distance(a.position, zone.transform.position) < 0.5f)
            {
                freed = a;
                break;
            }
        }
        if (freed != null) usedAnchors.Remove(freed);

        if (zone != null) Destroy(zone.gameObject);
    }

    bool TryFindPosition(out Vector3 pos, out Transform usedAnchor)
    {
        var free = new List<Transform>();
        foreach (var a in anchorPoints)
            if (a != null && !usedAnchors.Contains(a)) free.Add(a);

        Shuffle(free);
        foreach (var a in free)
        {
            if (IsPositionValid(a.position))
            {
                pos = a.position;
                usedAnchor = a;
                return true;
            }
        }

        for (int i = 0; i < randomPlacementAttempts; i++)
        {
            Vector2 c = Random.insideUnitCircle * randomAreaRadius;
            Vector3 candidate = transform.position + new Vector3(c.x, 0f, c.y);
            Vector3 rayOrigin = candidate + Vector3.up * 100f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 300f, groundMask, QueryTriggerInteraction.Ignore))
            {
                candidate.y = hit.point.y;
                if (IsPositionValid(candidate))
                {
                    pos = candidate;
                    usedAnchor = null;
                    return true;
                }
            }
        }

        pos = Vector3.zero;
        usedAnchor = null;
        return false;
    }

    bool IsPositionValid(Vector3 p)
    {
        if (player != null && Vector3.Distance(p, player.position) < minDistanceFromPlayer) return false;
        foreach (var z in activeZones)
        {
            if (z == null) continue;
            if (Vector3.Distance(p, z.transform.position) < minDistanceBetweenZones) return false;
        }
        return true;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, randomAreaRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, randomAreaRadius);

        if (anchorPoints != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var a in anchorPoints)
            {
                if (a == null) continue;
                Gizmos.DrawWireSphere(a.position, 2f);
            }
        }
    }
}
