using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SIMPLE character selection - just pick name and 1 of 3 characters
/// Add to CharacterSelection scene
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public Button character1Button;
    public Button character2Button;
    public Button confirmButton;
    public Image previewImage; // Shows selected character
    public TMP_Text errorText;

    private int selectedChar = 0;

    void Start()
    {
        character1Button.onClick.AddListener(() => SelectCharacter(0));
        character2Button.onClick.AddListener(() => SelectCharacter(1));
        confirmButton.onClick.AddListener(Confirm);

        // NEW - Find and connect return button if it exists
        //Button returnButton = GameObject.Find("ReturnToMenu")?.GetComponent<Button>();
        //if (returnButton != null)
        //{
        //    returnButton.onClick.AddListener(ReturnToMenu);
        //}

        errorText.text = "";

        if (CharacterManager.Instance != null)
        {
            if (CharacterManager.Instance.IsDataLoaded)
            {
                ApplyLoadedData();
            }
            else
            {
                CharacterManager.Instance.OnCharacterDataLoaded += ApplyLoadedData;
            }
        }
    }

    // NEW METHOD
    //void ReturnToMenu()
    //{
    //    if (AudioManager.Instance != null)
    //        AudioManager.Instance.PlayButtonClick();

    //    SceneLoader.Instance.LoadMainMenu();
    //}

    void ApplyLoadedData()
    {
        if (CharacterManager.Instance == null) return;

        nameInput.text = CharacterManager.Instance.playerName;

        selectedChar = CharacterManager.Instance.selectedCharacter;
        SelectCharacter(selectedChar);

        Debug.Log($"🟢 UI updated: {nameInput.text}, Character {selectedChar}");

        CharacterManager.Instance.OnCharacterDataLoaded -= ApplyLoadedData;
    }


    void LoadCurrentCharacter()
    {
        if (CharacterManager.Instance != null)
        {
            // Set name input to current name
            nameInput.text = CharacterManager.Instance.playerName;

            // Select current character
            selectedChar = CharacterManager.Instance.selectedCharacter;
            SelectCharacter(selectedChar);

            Debug.Log($"📝 Loaded current: {CharacterManager.Instance.playerName}, Character {selectedChar}");
        }
        else
        {
            // Fallback defaults
            nameInput.text = "Player";
            SelectCharacter(0);
        }
    }

    void SelectCharacter(int index)
    {
        if (CharacterManager.Instance == null) return;
        if (index < 0 || index >= CharacterManager.Instance.characterSprites.Length)
            index = 0;

        selectedChar = index;

        if (previewImage != null)
            previewImage.sprite = CharacterManager.Instance.characterSprites[index];

        ResetButtonColors();

        Button selectedButton = index == 0 ? character1Button : character2Button;
        selectedButton.GetComponent<Image>().color = Color.yellow;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }


    void ResetButtonColors()
    {
        character1Button.GetComponent<Image>().color = Color.white;
        character2Button.GetComponent<Image>().color = Color.white;
    }

    void Confirm()
    {
        string name = nameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            errorText.text = "Please enter a name!";
            return;
        }

        // Save to CharacterManager
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.playerName = name;
            CharacterManager.Instance.selectedCharacter = selectedChar;
            CharacterManager.Instance.SaveToFirebase();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Go to main menu
        SceneLoader.Instance.LoadMainMenu();
    }
}