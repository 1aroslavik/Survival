using System.Collections.Generic;
using UnityEngine;

public class PlayerAmbientSound : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Default background")]
    public AudioClip defaultAmbient;

    private List<SoundZone> activeZones = new List<SoundZone>();
    private SoundZone currentZone;

    void Start()
    {
        PlayDefault();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SoundZone")) return;

        SoundZone zone = other.GetComponent<SoundZone>();
        if (zone == null || zone.sound == null) return;

        activeZones.Add(zone);
        UpdateSound();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("SoundZone")) return;

        SoundZone zone = other.GetComponent<SoundZone>();
        if (zone == null) return;

        activeZones.Remove(zone);
        UpdateSound();
    }

    void UpdateSound()
    {
        if (activeZones.Count == 0)
        {
            currentZone = null;
            PlayDefault();
            return;
        }

        SoundZone newZone = activeZones[activeZones.Count - 1];

        if (currentZone == newZone) return;

        currentZone = newZone;

        audioSource.Stop();
        audioSource.clip = newZone.sound;
        audioSource.loop = true;
        audioSource.Play();
    }

    void PlayDefault()
    {
        if (defaultAmbient == null) return;

        if (audioSource.clip == defaultAmbient && audioSource.isPlaying) return;

        audioSource.Stop();
        audioSource.clip = defaultAmbient;
        audioSource.loop = true;
        audioSource.Play();

        Debug.Log("PLAY DEFAULT AMBIENT");
    }
}