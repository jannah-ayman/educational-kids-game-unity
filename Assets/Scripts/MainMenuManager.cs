using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Game Buttons")]
    public Button memoryMatchButton;
    public Button puzzleGameButton;
    public Button mathGameButton;
    public Button settingsButton;
    public Button logoutButton;
    public Image characterAvatar;

    void Start()
    {
        // Add button listeners with sound
        memoryMatchButton.onClick.AddListener(() => {
            PlayClick();
            SceneLoader.Instance.LoadMemoryMatch();
        });

        puzzleGameButton.onClick.AddListener(() => {
            PlayClick();
            SceneLoader.Instance.LoadPuzzleGame();
        });

        mathGameButton.onClick.AddListener(() => {
            PlayClick();
            SceneLoader.Instance.LoadMathGame();
        });

        settingsButton.onClick.AddListener(OpenSettings);
        logoutButton.onClick.AddListener(Logout);

        // TODO: Load character avatar from Firebase when character selection is done
    }

    void PlayClick()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void OpenSettings()
    {
        PlayClick();
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OpenSettings();
    }

    void Logout()
    {
        PlayClick();

        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.LogoutUser();
        }

        SceneLoader.Instance.LoadLogin();
    }
}