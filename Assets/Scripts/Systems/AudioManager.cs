// =============================================================================
// AudioManager.cs  |  Scripts/Systems
// WaifuGarden — Phase 0
// Singleton audio controller. THE ONLY CLASS allowed to call AudioSource.Play().
// All other systems call: AudioManager.Instance.PlaySFX("sound_id")
// Requires TWO AudioSource components on its GameObject (add both in Inspector).
//   AudioSource [0] = SFX (one-shot, non-looping)
//   AudioSource [1] = Music / Ambient (looping)
// =============================================================================

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Library")]
    [Tooltip("Assign the single SoundLibrary ScriptableObject asset here.")]
    public SoundLibrary SoundLibrary;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var sources = GetComponents<AudioSource>();
        _sfxSource   = sources.Length >= 1 ? sources[0] : gameObject.AddComponent<AudioSource>();
        _musicSource = sources.Length >= 2 ? sources[1] : gameObject.AddComponent<AudioSource>();

        _sfxSource.playOnAwake   = false;
        _musicSource.playOnAwake = false;
        _musicSource.loop        = true;

        SoundLibrary?.Initialize();
    }

    // ---- Public API ---------------------------------------------------------

    /// <summary>Play a one-shot SFX by sound ID. Silent fail if ID not found or clip is null.</summary>
    public void PlaySFX(string soundID)
    {
        if (!SoundLibrary) return;
        var clip = SoundLibrary.GetClip(soundID);
        if (clip) _sfxSource.PlayOneShot(clip);
    }

    /// <summary>Play a one-shot SFX directly from an AudioClip reference.</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip) _sfxSource.PlayOneShot(clip);
    }

    /// <summary>Start a music track. Stops any current music first.</summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!clip) return;
        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();
    }

    /// <summary>Stop music / ambient immediately.</summary>
    public void StopMusic() => _musicSource.Stop();

    /// <summary>Start an ambient loop (e.g. rain ambience). Uses the music channel.</summary>
    public void PlayAmbient(AudioClip clip) => PlayMusic(clip, true);

    /// <summary>Stop ambient sound. Called by EventManager when an event ends.</summary>
    public void StopAmbient() => StopMusic();
}
