using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this for TextMeshPro

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Popup")]
    public GameObject settingsPopup;
    public Button closeButtonTop;
    public Button closeButtonBottom;

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

    private bool musicIsMuted = false;
    private bool sfxIsMuted = false;
    private float musicPreviousVolume = 1f;
    private float sfxPreviousVolume = 1f;

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
        // Hide popup initially
        settingsPopup.SetActive(false);

        // Load saved volumes
        if (AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.musicSource.volume;
            sfxSlider.value = AudioManager.Instance.sfxSource.volume;

            musicPreviousVolume = musicSlider.value > 0 ? musicSlider.value : 1f;
            sfxPreviousVolume = sfxSlider.value > 0 ? sfxSlider.value : 1f;
        }

        // Add listeners
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicMuteButton.onClick.AddListener(ToggleMusicMute);
        sfxMuteButton.onClick.AddListener(ToggleSFXMute);

        closeButtonTop.onClick.AddListener(CloseSettings);           // ← Just closes
        closeButtonBottom.onClick.AddListener(GoToCharacterSelection); // ← Goes to character selection

        // Update button visuals
        UpdateMusicMuteIcon();
        UpdateSFXMuteIcon();
    }

    // ==================== POPUP CONTROL ====================
    public void OpenSettings()
    {
        settingsPopup.SetActive(true);
        PlayClickSound();
    }

    public void CloseSettings()
    {
        // Top right X button - just closes popup
        settingsPopup.SetActive(false);
        PlayClickSound();
    }

    public void GoToCharacterSelection()
    {
        // Bottom button - goes to character selection
        settingsPopup.SetActive(false);
        PlayClickSound();

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadCharacterSelection();
        }
    }

    // ==================== MUSIC ====================

    void OnMusicVolumeChanged(float volume)
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);

        if (volume == 0f)
        {
            musicIsMuted = true;
        }
        else
        {
            musicIsMuted = false;
            musicPreviousVolume = volume;
        }

        UpdateMusicMuteIcon();
    }

    void ToggleMusicMute()
    {
        if (AudioManager.Instance == null) return;

        musicIsMuted = !musicIsMuted;

        if (musicIsMuted)
        {
            musicPreviousVolume = AudioManager.Instance.musicSource.volume;
            AudioManager.Instance.musicSource.volume = 0f;
        }
        else
        {
            AudioManager.Instance.musicSource.volume = musicPreviousVolume;
        }

        musicSlider.value = AudioManager.Instance.musicSource.volume;
        PlayerPrefs.SetFloat("MusicVolume", AudioManager.Instance.musicSource.volume);

        UpdateMusicMuteIcon();
        PlayClickSound();
    }

    void UpdateMusicMuteIcon()
    {
        if (musicMuteIcon == null) return;

        if (musicIsMuted || musicSlider.value == 0f)
        {
            musicMuteIcon.color = Color.red; // Temporary
            // When sprites ready: musicMuteIcon.sprite = musicMutedSprite;
        }
        else
        {
            musicMuteIcon.color = Color.white; // Temporary
            // When sprites ready: musicMuteIcon.sprite = musicUnmutedSprite;
        }
    }

    // ==================== SFX ====================

    void OnSFXVolumeChanged(float volume)
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);

        if (volume == 0f)
        {
            sfxIsMuted = true;
        }
        else
        {
            sfxIsMuted = false;
            sfxPreviousVolume = volume;
        }

        UpdateSFXMuteIcon();

        if (volume > 0)
            PlayClickSound();
    }

    void ToggleSFXMute()
    {
        if (AudioManager.Instance == null) return;

        sfxIsMuted = !sfxIsMuted;

        if (sfxIsMuted)
        {
            sfxPreviousVolume = AudioManager.Instance.sfxSource.volume;
            AudioManager.Instance.sfxSource.volume = 0f;
        }
        else
        {
            AudioManager.Instance.sfxSource.volume = sfxPreviousVolume;
        }

        sfxSlider.value = AudioManager.Instance.sfxSource.volume;
        PlayerPrefs.SetFloat("SFXVolume", AudioManager.Instance.sfxSource.volume);

        UpdateSFXMuteIcon();

        if (!sfxIsMuted)
            PlayClickSound();
    }

    void UpdateSFXMuteIcon()
    {
        if (sfxMuteIcon == null) return;

        if (sfxIsMuted || sfxSlider.value == 0f)
        {
            sfxMuteIcon.color = Color.red;
        }
        else
        {
            sfxMuteIcon.color = Color.white;
        }
    }

    // ==================== HELPER ====================

    void PlayClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
}