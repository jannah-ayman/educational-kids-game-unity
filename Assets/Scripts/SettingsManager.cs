using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

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
    public AudioClip loseSound;

    [Header("References (Auto-Found)")]
    private GameObject settingsPopup;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Button musicMuteButton;
    private Button sfxMuteButton;
    private Image musicMuteIcon;
    private Image sfxMuteIcon;
    private Button closeButtonTop;
    private Button closeButtonBottom;

    [Header("Sprites (Assign in Inspector)")]
    public Sprite musicUnmutedSprite;
    public Sprite musicMutedSprite;
    public Sprite sfxUnmutedSprite;
    public Sprite sfxMutedSprite;

    private bool musicIsMuted = false;
    private bool sfxIsMuted = false;
    private float musicPreviousVolume = 1f;
    private float sfxPreviousVolume = 1f;

    void Awake()
    {
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
        FindPopupInScene();

        if (settingsPopup != null)
        {
            SetupPopup();
        }
        // Load saved volumes
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayButtonClick() => sfxSource.PlayOneShot(buttonClick);
    public void PlayCorrect() => sfxSource.PlayOneShot(correctSound);
    public void PlayWrong() => sfxSource.PlayOneShot(wrongSound);
    public void PlayCardFlip() => sfxSource.PlayOneShot(cardFlip);
    public void PlayPuzzleSnap() => sfxSource.PlayOneShot(puzzleSnap);
    public void PlayWinFanfare() => sfxSource.PlayOneShot(winFanfare);
    public void PlayLoseSound() => sfxSource.PlayOneShot(loseSound);


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPopupInScene();

        if (settingsPopup != null)
        {
            SetupPopup();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void FindPopupInScene()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            Transform popup = canvas.transform.Find("SettingsPopUp");
            if (popup != null)
            {
                settingsPopup = popup.gameObject;

                musicSlider = popup.Find("MusicSlider")?.GetComponent<Slider>();
                sfxSlider = popup.Find("SFXSlider")?.GetComponent<Slider>();
                musicMuteButton = popup.Find("MusicMuteButton")?.GetComponent<Button>();
                sfxMuteButton = popup.Find("SFXMuteButton")?.GetComponent<Button>();
                closeButtonTop = popup.Find("CloseButton")?.GetComponent<Button>();
                closeButtonBottom = popup.Find("CharacterButton")?.GetComponent<Button>();

                if (musicMuteButton != null)
                    musicMuteIcon = musicMuteButton.transform.GetChild(0)?.GetComponent<Image>();
                if (sfxMuteButton != null)
                    sfxMuteIcon = sfxMuteButton.transform.GetChild(0)?.GetComponent<Image>();

                return;
            }
        }
    }
    void SetupPopup()
    {
        // Hide popup
        settingsPopup.SetActive(false);

        // Remove old listeners (important when scene changes)
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        musicMuteButton.onClick.RemoveAllListeners();
        sfxMuteButton.onClick.RemoveAllListeners();
        closeButtonTop.onClick.RemoveAllListeners();
        closeButtonBottom.onClick.RemoveAllListeners();

        // Load volumes
        if (SettingsManager.Instance != null)
        {
            musicSlider.value = SettingsManager.Instance.musicSource.volume;
            sfxSlider.value = SettingsManager.Instance.sfxSource.volume;

            musicPreviousVolume = musicSlider.value > 0 ? musicSlider.value : 1f;
            sfxPreviousVolume = sfxSlider.value > 0 ? sfxSlider.value : 1f;
        }

        // Add listeners
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicMuteButton.onClick.AddListener(ToggleMusicMute);
        sfxMuteButton.onClick.AddListener(ToggleSFXMute);
        closeButtonTop.onClick.AddListener(CloseSettings);
        closeButtonBottom.onClick.AddListener(GoToCharacterSelection);

        // Update visuals
        UpdateMusicMuteIcon();
        UpdateSFXMuteIcon();
    }

    public void OpenSettings()
    {
        // If popup is null, try finding it again
        if (settingsPopup == null)
        {
            FindPopupInScene();

            if (settingsPopup != null)
            {
                SetupPopup();
            }
        }

        if (settingsPopup != null)
        {
            settingsPopup.SetActive(true);
            PlayButtonClick();
        }
        else
        {
            Debug.LogError("❌ SettingsPopup STILL null after re-search!");
        }
    }
    public void CloseSettings()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
            PlayButtonClick();
        }
        else
        {
            Debug.LogError("❌ settingsPopup is NULL!");
        }
    }

    public void GoToCharacterSelection()
    {
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(false);
        }

        PlayButtonClick();

        SceneManager.LoadScene("CharacterSelection");

    }
    void OnMusicVolumeChanged(float volume)
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.musicSource.volume = volume;
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
        if (SettingsManager.Instance == null) return;

        musicIsMuted = !musicIsMuted;

        if (musicIsMuted)
        {
            musicPreviousVolume = SettingsManager.Instance.musicSource.volume;
            SettingsManager.Instance.musicSource.volume = 0f;
        }
        else
        {
            SettingsManager.Instance.musicSource.volume = musicPreviousVolume;
        }

        musicSlider.value = SettingsManager.Instance.musicSource.volume;
        PlayerPrefs.SetFloat("MusicVolume", SettingsManager.Instance.musicSource.volume);

        UpdateMusicMuteIcon();
        PlayButtonClick();
    }

    void UpdateMusicMuteIcon()
    {
        if (musicMuteIcon == null) return;

        bool muted = musicIsMuted || musicSlider.value == 0f;

        musicMuteIcon.sprite = muted ? musicMutedSprite : musicUnmutedSprite;
        musicMuteIcon.color = Color.white; // keep sprite color normal
    }

    void OnSFXVolumeChanged(float volume)
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.sfxSource.volume = volume;
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
            PlayButtonClick();
    }

    void ToggleSFXMute()
    {
        if (SettingsManager.Instance == null) return;

        sfxIsMuted = !sfxIsMuted;

        if (sfxIsMuted)
        {
            sfxPreviousVolume = SettingsManager.Instance.sfxSource.volume;
            SettingsManager.Instance.sfxSource.volume = 0f;
        }
        else
        {
            SettingsManager.Instance.sfxSource.volume = sfxPreviousVolume;
        }

        sfxSlider.value = SettingsManager.Instance.sfxSource.volume;
        PlayerPrefs.SetFloat("SFXVolume", SettingsManager.Instance.sfxSource.volume);

        UpdateSFXMuteIcon();

        if (!sfxIsMuted)
            PlayButtonClick();
    }

    void UpdateSFXMuteIcon()
    {
        if (sfxMuteIcon == null) return;

        bool muted = sfxIsMuted || sfxSlider.value == 0f;

        sfxMuteIcon.sprite = muted ? sfxMutedSprite : sfxUnmutedSprite;
        sfxMuteIcon.color = Color.white;
    }

}