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
    public AudioClip starCollect;

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
        // Load settings
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        musicSource.mute = !musicEnabled;
        sfxSource.mute = !sfxEnabled;

        // Play background music if available
        if (backgroundMusic != null)
        {
            PlayMusic();
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

    public void PlayStarCollect()
    {
        PlaySFX(starCollect);
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }
}