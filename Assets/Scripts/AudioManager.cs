using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Persistent audio manager. It survives scene loads (DontDestroyOnLoad) so the music keeps playing
/// from the Main Menu into the level, and it destroys any duplicate copy so there is only ever one.
/// Music and sound effects play through separate AudioMixer groups, and both volumes are saved
/// with PlayerPrefs so they are restored the next time the game is launched.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SfxVolume";

    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [Tooltip("Plays the looping background music. Its Output is the mixer's Music group.")]
    [SerializeField] private AudioSource musicSource;
    [Tooltip("Plays one-shot sound effects. Its Output is the mixer's SFX group.")]
    [SerializeField] private AudioSource sfxSource;

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    private void Awake()
    {
        // Duplicate guard: a copy of this object loaded with a scene removes itself
        // if the original from an earlier scene is still alive.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // A duplicate that is being destroyed must not start a second copy of the music.
        if (Instance != this)
        {
            return;
        }

        // Mixer values can't be set in Awake, so the saved volumes are applied here.
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f));
        musicSource.Play();
    }

    /// <summary>Plays a sound effect if an AudioManager exists, so scenes can also be tested on their own.</summary>
    public static void PlaySound(AudioClip clip)
    {
        if (Instance != null && clip != null)
        {
            Instance.sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>Sets music volume from 0 to 1 and saves it.</summary>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
        mixer.SetFloat(MusicVolumeKey, ToDecibels(volume));
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();
    }

    /// <summary>Sets sound effect volume from 0 to 1 and saves it.</summary>
    public void SetSfxVolume(float volume)
    {
        SfxVolume = volume;
        mixer.SetFloat(SfxVolumeKey, ToDecibels(volume));
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        PlayerPrefs.Save();
    }

    /// <summary>Mixer volume is in decibels, so a 0-1 slider value is converted on a log scale.</summary>
    private static float ToDecibels(float volume)
    {
        return Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
    }
}
