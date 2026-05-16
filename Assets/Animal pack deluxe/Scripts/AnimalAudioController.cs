using UnityEngine;
using System.Collections;

public class AnimalAudioController : MonoBehaviour
{
    [Header("Voice")]
    public AudioClip[] idleClips;
    public AudioClip[] alertClips;   // медведь/волк: рычание при обнаружении; олень: тревожный крик
    public AudioClip[] attackClips;  // медведь, волк
    public AudioClip[] fleeClips;    // олень, заяц
    public AudioClip[] hurtClips;
    public AudioClip[] deathClips;

    [Header("Footsteps")]
    public AudioClip[] walkSteps;
    public AudioClip[] runSteps;

    [Header("Settings")]
    public float idleInterval = 8f;
    public float idleVariance = 3f;
    public float footstepWalkInterval = 0.5f;
    public float footstepRunInterval  = 0.27f;
    public float audioMaxDistance     = 25f;

    AudioSource voiceSource;
    AudioSource footSource;
    float footTimer;

    void Awake()
    {
        voiceSource = MakeSource(audioMaxDistance);
        footSource  = MakeSource(audioMaxDistance * 0.5f);
        StartCoroutine(IdleLoop());
    }

    AudioSource MakeSource(float maxDist)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.spatialBlend  = 1f;
        s.minDistance   = 2f;
        s.maxDistance   = maxDist;
        s.rolloffMode   = AudioRolloffMode.Logarithmic;
        s.playOnAwake   = false;
        s.dopplerLevel  = 0.2f;
        return s;
    }

    // ── вызывай из AI скриптов ───────────────────────────────────────────

    public void OnDetectPlayer() => PlayVoice(alertClips);
    public void OnAttack()       => PlayVoice(attackClips);
    public void OnFlee()         => PlayVoice(fleeClips);
    public void OnHurt()         => PlayVoice(hurtClips, force: true);
    public void OnDeath()
    {
        StopAllCoroutines();
        PlayVoice(deathClips, force: true);
    }

    // скорость берёшь из agent.velocity.magnitude
    public void TickFootsteps(float speed)
    {
        if (speed < 0.3f) { footTimer = 0; return; }

        bool running  = speed > 3f;
        float interval = running ? footstepRunInterval : footstepWalkInterval;
        footTimer -= Time.deltaTime;

        if (footTimer <= 0f)
        {
            footTimer = interval * Random.Range(0.85f, 1.15f);
            var clips = running ? runSteps : walkSteps;
            if (clips != null && clips.Length > 0)
            {
                footSource.pitch = Random.Range(0.88f, 1.12f);
                footSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            }
        }
    }

    // ── внутреннее ───────────────────────────────────────────────────────

    void PlayVoice(AudioClip[] clips, bool force = false)
    {
        if (clips == null || clips.Length == 0) return;
        if (!force && voiceSource.isPlaying) return;
        voiceSource.pitch = Random.Range(0.93f, 1.07f);
        voiceSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    IEnumerator IdleLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, idleInterval)); // offset чтобы не все одновременно
        while (true)
        {
            yield return new WaitForSeconds(idleInterval + Random.Range(-idleVariance, idleVariance));
            PlayVoice(idleClips);
        }
    }
}