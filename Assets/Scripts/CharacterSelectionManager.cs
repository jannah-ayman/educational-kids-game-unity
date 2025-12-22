using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public Button character1Button;
    public Button character2Button;
    public Button confirmButton;
    public TMP_Text errorText;

    private int selectedChar = 0;

    void Start()
    {
        errorText.text = "";

        // Hook up buttons
        character1Button.onClick.AddListener(() => SelectCharacter(0));
        character2Button.onClick.AddListener(() => SelectCharacter(1));
        confirmButton.onClick.AddListener(Confirm);

        // Load data from Firebase first
        if (CharacterManager.Instance != null)
        {
            // Subscribe to be notified when data is loaded
            CharacterManager.Instance.OnCharacterDataLoaded += ApplyLoadedData;

            // If not loaded yet, fetch it
            if (!CharacterManager.Instance.IsDataLoaded)
            {
                nameInput.text = "Loading..."; // temporary placeholder
                CharacterManager.Instance.LoadFromFirebase();
            }
            else
            {
                // Already loaded? Apply immediately
                ApplyLoadedData();
            }
        }
        else
        {
            // Fallback defaults
            nameInput.text = "Player";
            selectedChar = 0;
            UpdateButtonColors();
        }
    }

    // Apply Firebase-loaded data to UI
    void ApplyLoadedData()
    {
        if (CharacterManager.Instance == null) return;

        nameInput.text = CharacterManager.Instance.playerName;
        selectedChar = CharacterManager.Instance.selectedCharacter;
        UpdateButtonColors();

        Debug.Log($"🟢 UI updated from Firebase: {nameInput.text}, Character {selectedChar}");

        // Unsubscribe so it only runs once
        CharacterManager.Instance.OnCharacterDataLoaded -= ApplyLoadedData;
    }

    void SelectCharacter(int index)
    {
        selectedChar = index;
        UpdateButtonColors();

        // Optional: play click sound
        SettingsManager.Instance?.PlayButtonClick();
    }

    void UpdateButtonColors()
    {
        if (character1Button != null) character1Button.GetComponent<Image>().color = (selectedChar == 0) ? Color.yellow : Color.white;
        if (character2Button != null) character2Button.GetComponent<Image>().color = (selectedChar == 1) ? Color.yellow : Color.white;
    }

    void Confirm()
    {
        string name = nameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            errorText.text = "Please enter a name!";
            return;
        }

        if (CharacterManager.Instance != null)
        {
            // Save selection to Firebase
            CharacterManager.Instance.playerName = name;
            CharacterManager.Instance.selectedCharacter = selectedChar;
            CharacterManager.Instance.SaveToFirebase();
        }

        SettingsManager.Instance?.PlayButtonClick();

        // Go to main menu
        SceneManager.LoadScene("MainMenu");
    }
}
