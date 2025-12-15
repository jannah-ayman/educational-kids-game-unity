using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StarPopupManager : MonoBehaviour
{
    public static StarPopupManager Instance;

    [Header("Sprites (Assign in Inspector)")]
    public Sprite starEmpty;
    public Sprite starFull;

    // Auto-found references
    private GameObject popupPanel;
    private TMP_Text winText;
    private Image[] stars;
    private TMP_Text scoreText;
    private Button playAgainButton;
    private Button mainMenuButton;

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

        if (popupPanel != null)
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPopupInScene();

        if (popupPanel != null)
        {
            SetupPopup();
        }
    }

    void FindPopupInScene()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            Transform popup = canvas.transform.Find("StarPopup");
            if (popup != null)
            {
                popupPanel = popup.gameObject;
                Debug.Log("✅ Found StarPopup in scene!");

                // Find all child elements
                winText = popup.Find("WinText")?.GetComponent<TMP_Text>();
                scoreText = popup.Find("ScoreText")?.GetComponent<TMP_Text>();
                playAgainButton = popup.Find("PlayAgainButton")?.GetComponent<Button>();
                mainMenuButton = popup.Find("MainMenuButton")?.GetComponent<Button>();

                // Find stars container and all star images
                Transform starsContainer = popup.Find("StarsContainer");
                if (starsContainer != null)
                {
                    stars = new Image[5];
                    for (int i = 0; i < 5; i++)
                    {
                        Transform star = starsContainer.Find("Star" + (i + 1));
                        if (star != null)
                            stars[i] = star.GetComponent<Image>();
                    }
                }

                return;
            }
        }

        Debug.LogWarning("⚠️ StarPopup not found in scene!");
    }

    void SetupPopup()
    {
        // Hide popup
        popupPanel.SetActive(false);

        // Remove old listeners
        playAgainButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.RemoveAllListeners();

        // Add listeners
        playAgainButton.onClick.AddListener(PlayAgain);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    // ==================== PUBLIC METHODS ====================
    public string GetTitleText(int starCount)
    {
        switch (starCount)
        {
            case 5:
                return "Amazing!";
            case 4:
                return "Great Job!";
            case 3:
                return "Nice Work!";
            case 2:
                return "Keep Practicing!";
            case 1:
                return "Almost There!";
            case 0:
                return "Uh-Oh!";
            default:
                return "";
        }
    }

    public void ShowStars(int starCount, string message)
    {
        if (popupPanel == null)
        {
            FindPopupInScene();
            if (popupPanel != null)
                SetupPopup();
        }

        if (popupPanel == null)
        {
            Debug.LogError("❌ StarPopup not found in scene!");
            return;
        }

        starCount = Mathf.Clamp(starCount, 0, 5);

        // ⭐ Set star images
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
                stars[i].sprite = i < starCount ? starFull : starEmpty;
        }

        // ⭐ SET TITLE TEXT HERE
        if (winText != null)
            winText.text = GetTitleText(starCount);

        // Score / message text
        if (scoreText != null)
            scoreText.text = message;

        popupPanel.SetActive(true);

        if (AudioManager.Instance != null)
        {
            if (starCount <= 1)
                AudioManager.Instance.PlayLoseSound();
            else
                AudioManager.Instance.PlayWinFanfare();
        }

    }


    public void PlayAgain()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (popupPanel != null)
            popupPanel.SetActive(false);

        SceneLoader.Instance.ReloadCurrentScene();
    }

    public void GoToMainMenu()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
         
        if (popupPanel != null)
            popupPanel.SetActive(false);

        SceneLoader.Instance.LoadMainMenu();
    }
}