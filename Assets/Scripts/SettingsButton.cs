using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OpenSettings);
    }

    void OpenSettings()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OpenSettings();
        }
        else
        {
            Debug.LogError("SettingsManager not found! Make sure you started from Loading scene.");
        }
    }
}