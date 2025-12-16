using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

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
        // Find popup in current scene
        FindPopupInScene();

        if (settingsPopup != null)
        {
            SetupPopup();
        }
    }
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
        Debug.Log("🔍 Searching for SettingsPopup...");

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        Debug.Log($"Found {canvases.Length} canvases in scene");

        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Checking canvas: {canvas.name}");

            // List all children of this canvas
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Debug.Log($"  - Child {i}: {canvas.transform.GetChild(i).name}");
            }

            Transform popup = canvas.transform.Find("SettingsPopUp");
            if (popup != null)
            {
                settingsPopup = popup.gameObject;
                Debug.Log("✅ Found SettingsPopup in scene!");

                // Find all child elements
                musicSlider = popup.Find("MusicSlider")?.GetComponent<Slider>();
                sfxSlider = popup.Find("SFXSlider")?.GetComponent<Slider>();
                musicMuteButton = popup.Find("MusicMuteButton")?.GetComponent<Button>();
                sfxMuteButton = popup.Find("SFXMuteButton")?.GetComponent<Button>();
                closeButtonTop = popup.Find("CloseButton")?.GetComponent<Button>();
                closeButtonBottom = popup.Find("CharacterButton")?.GetComponent<Button>();

                // Debug what was found
                Debug.Log($"MusicSlider: {(musicSlider != null ? "✅" : "❌")}");
                Debug.Log($"SFXSlider: {(sfxSlider != null ? "✅" : "❌")}");
                Debug.Log($"CloseButton: {(closeButtonTop != null ? "✅" : "❌")}");
                Debug.Log($"CharacterButton: {(closeButtonBottom != null ? "✅" : "❌")}");

                // Find icons (Image components inside buttons)
                if (musicMuteButton != null)
                    musicMuteIcon = musicMuteButton.transform.GetChild(0)?.GetComponent<Image>();
                if (sfxMuteButton != null)
                    sfxMuteIcon = sfxMuteButton.transform.GetChild(0)?.GetComponent<Image>();

                return;
            }
        }

        Debug.LogError("❌ SettingsPopup not found in scene! Add SettingsPopup prefab to Canvas.");
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
        closeButtonTop.onClick.AddListener(CloseSettings);
        closeButtonBottom.onClick.AddListener(GoToCharacterSelection);

        // Update visuals
        UpdateMusicMuteIcon();
        UpdateSFXMuteIcon();
    }

    // ==================== POPUP CONTROL ====================

    public void OpenSettings()
    {
        // If popup is null, try finding it again
        if (settingsPopup == null)
        {
            Debug.Log("⚠️ Popup was null, searching again...");
            FindPopupInScene();

            if (settingsPopup != null)
            {
                SetupPopup();
            }
        }

        if (settingsPopup != null)
        {
            settingsPopup.SetActive(true);
            PlayClickSound();
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
            PlayClickSound();
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

        PlayClickSound();

        if (SceneLoader.Instance != null)
        {
            Debug.Log("✅ Loading CharacterSelection scene");
            SceneLoader.Instance.LoadCharacterSelection();
        }
        else
        {
            Debug.LogError("❌ SceneLoader.Instance is NULL!");
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

        bool muted = musicIsMuted || musicSlider.value == 0f;

        musicMuteIcon.sprite = muted ? musicMutedSprite : musicUnmutedSprite;
        musicMuteIcon.color = Color.white; // keep sprite color normal
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

        bool muted = sfxIsMuted || sfxSlider.value == 0f;

        sfxMuteIcon.sprite = muted ? sfxMutedSprite : sfxUnmutedSprite;
        sfxMuteIcon.color = Color.white;
    }


    // ==================== HELPER ====================

    void PlayClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }
}