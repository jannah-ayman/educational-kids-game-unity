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

        character1Button.onClick.AddListener(() => SelectCharacter(0));
        character2Button.onClick.AddListener(() => SelectCharacter(1));
        confirmButton.onClick.AddListener(Confirm);

        nameInput.text = CharacterManager.Instance.playerName;
        selectedChar = CharacterManager.Instance.selectedCharacter;
        UpdateButtonColors();
    }

    void SelectCharacter(int index)
    {
        selectedChar = index;
        UpdateButtonColors();

        SettingsManager.Instance?.PlayButtonClick();
    }

    void UpdateButtonColors()
    {
        character1Button.GetComponent<Image>().color = (selectedChar == 0) ? Color.yellow : Color.white;
        character2Button.GetComponent<Image>().color = (selectedChar == 1) ? Color.yellow : Color.white;
    }

    void Confirm()
    {
        SettingsManager.Instance?.PlayButtonClick();

        string name = nameInput.text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            errorText.text = "Please enter a name!";
            return;
        }

        CharacterManager.Instance.playerName = name;
        CharacterManager.Instance.selectedCharacter = selectedChar;
        CharacterManager.Instance.SaveToFirebase();
 
        SceneManager.LoadScene("MainMenu");
    }
}
