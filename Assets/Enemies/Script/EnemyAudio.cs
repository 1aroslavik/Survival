using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Вешается на того же врага что и EnemyBaseAI.
/// Требует: AudioSource на том же объекте.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Звуки шагов")]
    [Tooltip("3-5 вариантов — будут играть по очереди рандомно")]
    public AudioClip[] footstepClips;

    [Header("Звуки бега")]
    [Tooltip("2-3 варианта — играют когда враг бежит к игроку")]
    public AudioClip[] runClips;

    [Header("Звуки атаки")]
    [Tooltip("2-3 варианта удара/рыка при атаке")]
    public AudioClip[] attackClips;

    [Header("Звук обнаружения игрока")]
    [Tooltip("1-2 звука — играет когда враг агрится (переходит в Chase)")]
    public AudioClip[] alertClips;

    [Header("Звуки получения урона")]
    [Tooltip("2-3 варианта — играет когда врагу наносят урон")]
    public AudioClip[] hurtClips;

    [Header("Звуки смерти")]
    [Tooltip("1-2 варианта — играет при гибели")]
    public AudioClip[] deathClips;

    [Header("Ambient / стрёмные звуки")]
    [Tooltip("3-5 звуков — дыхание, бормотание, скрежет. Играют периодически.")]
    public AudioClip[] ambientClips;

    [Header("Настройки шагов")]
    [Tooltip("Секунд между шагами при ходьбе")]
    public float walkStepInterval = 0.55f;
    [Tooltip("Секунд между шагами при беге")]
    public float runStepInterval = 0.32f;
    [Tooltip("Минимальная скорость агента чтобы считаться движущимся")]
    public float moveThreshold = 0.1f;
    [Tooltip("Скорость выше этой = бег (обычно равно runSpeed в AI)")]
    public float runThreshold = 3f;

    [Header("Настройки ambient")]
    public float ambientMinInterval = 6f;
    public float ambientMaxInterval = 18f;

    [Header("Громкость (0-1)")]
    [Range(0f, 1f)] public float footstepVolume = 0.6f;
    [Range(0f, 1f)] public float runVolume = 0.7f;
    [Range(0f, 1f)] public float attackVolume = 1f;
    [Range(0f, 1f)] public float alertVolume = 1f;
    [Range(0f, 1f)] public float hurtVolume = 0.9f;
    [Range(0f, 1f)] public float deathVolume = 1f;
    [Range(0f, 1f)] public float ambientVolume = 0.5f;

    // -------------------------------------------------------
    private AudioSource audioSource;
    private NavMeshAgent agent;

    private float stepTimer = 0f;
    private float ambientTimer;
    private bool isDead = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();

        // Принудительно включаем 3D звук — без этого зональность не работает
        audioSource.spatialBlend = 1f;

        // Первый ambient через случайное время — у разных врагов разное смещение
        ambientTimer = Random.Range(ambientMinInterval, ambientMaxInterval);
    }

    void Update()
    {
        if (isDead) return;

        HandleFootsteps();
        HandleAmbient();
    }

    // --- Шаги / бег по velocity агента ---
    void HandleFootsteps()
    {
        if (agent == null) return;

        float speed = agent.velocity.magnitude;
        if (speed < moveThreshold) return; // стоит — тишина

        stepTimer -= Time.deltaTime;
        if (stepTimer > 0f) return;

        bool running = speed >= runThreshold;

        if (running)
        {
            PlayRandom(runClips, runVolume);
            stepTimer = runStepInterval;
        }
        else
        {
            PlayRandom(footstepClips, footstepVolume);
            stepTimer = walkStepInterval;
        }
    }

    // --- Периодические стрёмные ambient звуки ---
    void HandleAmbient()
    {
        ambientTimer -= Time.deltaTime;
        if (ambientTimer > 0f) return;

        PlayRandom(ambientClips, ambientVolume);
        ambientTimer = Random.Range(ambientMinInterval, ambientMaxInterval);
    }

    // -------------------------------------------------------
    // Публичные методы — вызываются из EnemyBaseAI
    // -------------------------------------------------------

    /// <summary>Враг засёк игрока и переходит в Chase</summary>
    public void PlayAlert()
    {
        PlayRandom(alertClips, alertVolume);
    }

    /// <summary>Враг начинает атаку</summary>
    public void PlayAttack()
    {
        PlayRandom(attackClips, attackVolume);
    }

    /// <summary>Враг получил урон</summary>
    public void PlayHurt()
    {
        PlayRandom(hurtClips, hurtVolume);
    }

    /// <summary>Враг умер</summary>
    public void PlayDeath()
    {
        isDead = true; // останавливаем шаги и ambient
        PlayRandom(deathClips, deathVolume);
    }

    // -------------------------------------------------------
    // Утилита
    // -------------------------------------------------------

    /// <summary>Играет случайный клип. PlayOneShot = несколько звуков могут играть одновременно</summary>
    void PlayRandom(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip, volume);
    }

    // Показывает радиус слышимости в редакторе (жёлтая сфера)
    void OnDrawGizmosSelected()
    {
        AudioSource src = GetComponent<AudioSource>();
        if (src == null) return;
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, src.maxDistance);
    }
}