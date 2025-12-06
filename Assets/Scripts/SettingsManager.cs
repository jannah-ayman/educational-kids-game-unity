using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Music Controls")]
    public Slider musicSlider;
    public Button musicMuteButton;
    public Image musicMuteIcon;
    public Sprite musicUnmutedSprite;
    public Sprite musicMutedSprite;

    [Header("SFX Controls")]
    public Slider sfxSlider;
    public Button sfxMuteButton;
    public Image sfxMuteIcon;
    public Sprite sfxUnmutedSprite;
    public Sprite sfxMutedSprite;

    [Header("Other UI")]
    public Button resetButton;
    public Button backButton;
    public GameObject confirmPopup;
    public Button yesButton;
    public Button noButton;

    // Remember volume before mute
    private float musicVolumeBeforeMute = 1f;
    private float sfxVolumeBeforeMute = 1f;

    // Track mute state
    private bool musicIsMuted = false;
    private bool sfxIsMuted = false;

    void Start()
    {
        // Hide popup
        confirmPopup.SetActive(false);

        // Load saved settings
        LoadSettings();

        // Add slider listeners (fires when value changes)
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        // Add mute button listeners
        musicMuteButton.onClick.AddListener(OnMusicMuteClicked);
        sfxMuteButton.onClick.AddListener(OnSFXMuteClicked);

        // Other buttons
        resetButton.onClick.AddListener(OnResetClicked);
        backButton.onClick.AddListener(OnBackClicked);
        yesButton.onClick.AddListener(OnResetConfirmed);
        noButton.onClick.AddListener(OnResetCancelled);

        // Update mute button visuals
        UpdateMusicMuteButton();
        UpdateSFXMuteButton();
    }

    void LoadSettings()
    {
        // Load saved volumes (default: 1.0 = 100%)
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool savedMusicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        bool savedSFXMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;

        // Set sliders
        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;

        // Set mute states
        musicIsMuted = savedMusicMuted;
        sfxIsMuted = savedSFXMuted;

        // Remember volumes before mute
        musicVolumeBeforeMute = savedMusicVolume > 0 ? savedMusicVolume : 1f;
        sfxVolumeBeforeMute = savedSFXVolume > 0 ? savedSFXVolume : 1f;

        // Apply to AudioManager
        ApplyMusicSettings();
        ApplySFXSettings();
    }

    // ==================== MUSIC CONTROLS ====================

    void OnMusicSliderChanged(float value)
    {
        Debug.Log("Music volume: " + value);

        // If slider reaches 0, auto-mute
        if (value == 0)
        {
            musicIsMuted = true;
        }
        else
        {
            // If was muted and slider moved, unmute
            if (musicIsMuted)
            {
                musicIsMuted = false;
            }

            // Remember this volume
            musicVolumeBeforeMute = value;
        }

        // Save
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.SetInt("MusicMuted", musicIsMuted ? 1 : 0);
        PlayerPrefs.Save();

        // Apply
        ApplyMusicSettings();
        UpdateMusicMuteButton();
    }

    void OnMusicMuteClicked()
    {
        Debug.Log("Music mute button clicked");

        // Toggle mute
        musicIsMuted = !musicIsMuted;

        if (musicIsMuted)
        {
            // Muting - remember current volume, set slider to 0
            musicVolumeBeforeMute = musicSlider.value > 0 ? musicSlider.value : 1f;
            musicSlider.value = 0;
        }
        else
        {
            // Unmuting - restore previous volume
            musicSlider.value = musicVolumeBeforeMute;
        }

        // Save
        PlayerPrefs.SetInt("MusicMuted", musicIsMuted ? 1 : 0);
        PlayerPrefs.Save();

        // Apply
        ApplyMusicSettings();
        UpdateMusicMuteButton();

        // Play click sound
        PlayClickSound();
    }

    void ApplyMusicSettings()
    {
        if (AudioManager.Instance == null) return;

        if (musicIsMuted || musicSlider.value == 0)
        {
            // Muted
            AudioManager.Instance.musicSource.volume = 0;
            AudioManager.Instance.musicSource.mute = true;
        }
        else
        {
            // Set volume
            AudioManager.Instance.musicSource.volume = musicSlider.value;
            AudioManager.Instance.musicSource.mute = false;

            // Start music if not playing
            if (!AudioManager.Instance.musicSource.isPlaying && AudioManager.Instance.backgroundMusic != null)
            {
                AudioManager.Instance.PlayMusic();
            }
        }
    }

    void UpdateMusicMuteButton()
    {
        if (musicMuteIcon == null) return;

        // Change sprite based on mute state
        if (musicIsMuted || musicSlider.value == 0)
        {
            // Show muted icon
            if (musicMutedSprite != null)
                musicMuteIcon.sprite = musicMutedSprite;
            else
                musicMuteIcon.color = Color.red; // Fallback: red tint
        }
        else
        {
            // Show unmuted icon
            if (musicUnmutedSprite != null)
                musicMuteIcon.sprite = musicUnmutedSprite;
            else
                musicMuteIcon.color = Color.white; // Fallback: white
        }
    }

    // ==================== SFX CONTROLS ====================

    void OnSFXSliderChanged(float value)
    {
        Debug.Log("SFX volume: " + value);

        // Same logic as music
        if (value == 0)
        {
            sfxIsMuted = true;
        }
        else
        {
            if (sfxIsMuted)
            {
                sfxIsMuted = false;
            }
            sfxVolumeBeforeMute = value;
        }

        // Save
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.SetInt("SFXMuted", sfxIsMuted ? 1 : 0);
        PlayerPrefs.Save();

        // Apply
        ApplySFXSettings();
        UpdateSFXMuteButton();

        // Play test sound when adjusting
        if (!sfxIsMuted && value > 0)
        {
            PlayTestSound();
        }
    }

    void OnSFXMuteClicked()
    {
        Debug.Log("SFX mute button clicked");

        // Toggle mute
        sfxIsMuted = !sfxIsMuted;

        if (sfxIsMuted)
        {
            // Muting
            sfxVolumeBeforeMute = sfxSlider.value > 0 ? sfxSlider.value : 1f;
            sfxSlider.value = 0;
        }
        else
        {
            // Unmuting
            sfxSlider.value = sfxVolumeBeforeMute;
        }

        // Save
        PlayerPrefs.SetInt("SFXMuted", sfxIsMuted ? 1 : 0);
        PlayerPrefs.Save();

        // Apply
        ApplySFXSettings();
        UpdateSFXMuteButton();

        // Play test sound if unmuting
        if (!sfxIsMuted)
        {
            PlayTestSound();
        }
    }

    void ApplySFXSettings()
    {
        if (AudioManager.Instance == null) return;

        if (sfxIsMuted || sfxSlider.value == 0)
        {
            // Muted
            AudioManager.Instance.sfxSource.volume = 0;
            AudioManager.Instance.sfxSource.mute = true;
        }
        else
        {
            // Set volume
            AudioManager.Instance.sfxSource.volume = sfxSlider.value;
            AudioManager.Instance.sfxSource.mute = false;
        }
    }

    void UpdateSFXMuteButton()
    {
        if (sfxMuteIcon == null) return;

        if (sfxIsMuted || sfxSlider.value == 0)
        {
            // Show muted icon
            if (sfxMutedSprite != null)
                sfxMuteIcon.sprite = sfxMutedSprite;
            else
                sfxMuteIcon.color = Color.red;
        }
        else
        {
            // Show unmuted icon
            if (sfxUnmutedSprite != null)
                sfxMuteIcon.sprite = sfxUnmutedSprite;
            else
                sfxMuteIcon.color = Color.white;
        }
    }

    // ==================== HELPER METHODS ====================

    void PlayClickSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    void PlayTestSound()
    {
        // Play a test sound when adjusting SFX volume
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    // ==================== OTHER BUTTONS ====================

    void OnResetClicked()
    {
        confirmPopup.SetActive(true);
        PlayClickSound();
    }

    void OnResetConfirmed()
    {
        Debug.Log("Reset confirmed!");

        // Clear all data
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        confirmPopup.SetActive(false);

        // Go to login
        SceneLoader.Instance.LoadLogin();
    }

    void OnResetCancelled()
    {
        confirmPopup.SetActive(false);
        PlayClickSound();
    }

    void OnBackClicked()
    {
        Debug.Log("Back button clicked!");

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("SceneLoader.Instance is NULL!");
            return;
        }

        Debug.Log("Loading MainMenu...");
        PlayClickSound();
        SceneLoader.Instance.LoadMainMenu();
    }
}