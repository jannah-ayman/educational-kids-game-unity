using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public Button resetButton;
    public Button backButton;

    [Header("Confirm Popup")]
    public GameObject confirmPopup;
    public Button yesButton;
    public Button noButton;

    void Start()
    {
        // Hide popup initially
        confirmPopup.SetActive(false);

        // Load saved settings
        LoadSettings();

        // Add listeners
        musicToggle.onValueChanged.AddListener(OnMusicToggled);
        sfxToggle.onValueChanged.AddListener(OnSFXToggled);
        resetButton.onClick.AddListener(OnResetClicked);
        backButton.onClick.AddListener(OnBackClicked);

        // Popup buttons
        yesButton.onClick.AddListener(OnResetConfirmed);
        noButton.onClick.AddListener(OnResetCancelled);
    }

    void LoadSettings()
    {
        // Load from PlayerPrefs (temporary until Firebase)
        bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool sfxOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        musicToggle.isOn = musicOn;
        sfxToggle.isOn = sfxOn;

        // Apply to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicSource.mute = !musicOn;
            AudioManager.Instance.sfxSource.mute = !sfxOn;
        }
    }

    void OnMusicToggled(bool isOn)
    {
        Debug.Log("Music toggled: " + isOn);

        // Save preference
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();

        // Apply to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicSource.mute = !isOn;

            // Play test sound
            if (isOn)
            {
                AudioManager.Instance.PlayMusic();
            }
        }
    }

    void OnSFXToggled(bool isOn)
    {
        Debug.Log("SFX toggled: " + isOn);

        // Save preference
        PlayerPrefs.SetInt("SFXEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();

        // Apply to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.sfxSource.mute = !isOn;

            // Play test sound if turning on
            if (isOn && AudioManager.Instance.buttonClick != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
            }
        }
    }

    void OnResetClicked()
    {
        Debug.Log("Reset button clicked");

        // Show confirmation popup
        confirmPopup.SetActive(true);

        // Play sound
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }
    }

    void OnResetConfirmed()
    {
        Debug.Log("Reset confirmed!");

        // TODO: Clear Firebase data when TA responds
        // For now, clear PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Restore default settings
        PlayerPrefs.SetInt("MusicEnabled", 1);
        PlayerPrefs.SetInt("SFXEnabled", 1);
        PlayerPrefs.Save();

        // Hide popup
        confirmPopup.SetActive(false);

        // Show feedback (optional)
        Debug.Log("Progress reset! Going back to login...");

        // Go to login screen
        SceneLoader.Instance.LoadLogin();
    }

    void OnResetCancelled()
    {
        Debug.Log("Reset cancelled");

        // Hide popup
        confirmPopup.SetActive(false);
    }

    void OnBackClicked()
    {
        Debug.Log("Back to main menu");

        // Play sound
        if (AudioManager.Instance != null && AudioManager.Instance.buttonClick != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClick);
        }

        // Go back to main menu
        SceneLoader.Instance.LoadMainMenu();
    }
}