using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioClip buttonClick;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip cardFlip;
    public AudioClip puzzleSnap;
    public AudioClip winFanfare;
    //public AudioClip starCollect;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load settings FIRST
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        // Load volume (NEW - add this)
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        // Apply volumes
        musicSource.volume = musicMuted ? 0 : musicVolume;
        sfxSource.volume = sfxMuted ? 0 : sfxVolume;

        musicSource.mute = musicMuted;
        sfxSource.mute = sfxMuted;

        // FORCE play music
        if (backgroundMusic != null && !musicMuted)
        {
            PlayMusic();
            Debug.Log("Music started! Volume: " + musicSource.volume);
        }
        else
        {
            Debug.LogWarning("Music NOT playing. Muted: " + musicMuted + ", Clip exists: " + (backgroundMusic != null));
        }
    }

    public void PlayMusic()
    {
        if (backgroundMusic == null) return;

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    // Helper methods for common sounds
    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayCorrect()
    {
        PlaySFX(correctSound);
    }

    public void PlayWrong()
    {
        PlaySFX(wrongSound);
    }

    public void PlayCardFlip()
    {
        PlaySFX(cardFlip);
    }

    public void PlayPuzzleSnap()
    {
        PlaySFX(puzzleSnap);
    }

    public void PlayWinFanfare()
    {
        PlaySFX(winFanfare);
    }

    //public void PlayStarCollect()
    //{
    //    PlaySFX(starCollect);
    //}

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }
}