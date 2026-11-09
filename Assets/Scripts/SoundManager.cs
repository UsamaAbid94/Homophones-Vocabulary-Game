using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager soundManager;
    public AudioSource soundAudioSource;
    public AudioSource musicAudioSource;
    public AudioClip buttonClickClip;

    [Header("Music")]
    public AudioClip musicClip;

    [Header("UI sound effects")]
    public AudioClip popupClip;
    public AudioClip correctClip;
    public AudioClip starClip;

    public MenuController menuController;

    private void Awake()
    {
        if (soundManager != null && soundManager != this)
        {
            Destroy(this.gameObject);
            return;//Avoid doing anything else
        }

        soundManager = this;
        DontDestroyOnLoad(this.gameObject);

        if (musicAudioSource != null && musicClip != null)
        {
            musicAudioSource.clip = musicClip;
            musicAudioSource.loop = true;
            if (!musicAudioSource.isPlaying) musicAudioSource.Play();
        }
    }

    public void ButtonClickSound()
    {
        PlayOneShot(buttonClickClip);
    }

    /// <summary>Plays an effect without cutting off whatever else is playing.</summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null && soundAudioSource != null) soundAudioSource.PlayOneShot(clip, volume);
    }

    /// <summary>Safe static helper: does nothing if there is no SoundManager (e.g. Game scene opened directly).</summary>
    public static void Sfx(AudioClip clip, float volume = 1f)
    {
        if (soundManager != null) soundManager.PlayOneShot(clip, volume);
    }

    public static void Popup() { if (soundManager != null) Sfx(soundManager.popupClip, 0.8f); }
    public static void Correct() { if (soundManager != null) Sfx(soundManager.correctClip); }
    public static void Star() { if (soundManager != null) Sfx(soundManager.starClip, 0.9f); }
}
