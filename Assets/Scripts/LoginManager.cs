using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text errorText;
    public Button loginButton;
    public Button registerButton;

    bool isProcessing = false;

    void Start()
    {
        errorText.text = "";

        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);

        SetButtons(false);
        errorText.text = "Connecting...";

        if (FirebaseManager.Instance.isFirebaseReady)
        {
            OnFirebaseReady();
        }
        else
        {
            FirebaseManager.Instance.OnFirebaseInitialized += OnFirebaseReady;
        }
    }

    void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
            FirebaseManager.Instance.OnFirebaseInitialized -= OnFirebaseReady;
    }

    void OnFirebaseReady()
    {
        errorText.text = "";
        SetButtons(true);
    }

    public void OnLoginClicked()
    {
        if (isProcessing) return;

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (!Validate(email, password)) return;

        isProcessing = true;
        SetButtons(false);
        errorText.text = "Logging in...";

        FirebaseManager.Instance.LoginUser(email, password, OnAuthResult);
    }

    public void OnRegisterClicked()
    {
        if (isProcessing) return;

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (!Validate(email, password)) return;

        isProcessing = true;
        SetButtons(false);
        errorText.text = "Creating account...";

        FirebaseManager.Instance.RegisterUser(email, password, OnAuthResult);
    }

    void OnAuthResult(bool success, string message)
    {
        isProcessing = false;

        if (success)
        {
            errorText.text = "";
            SceneLoader.Instance.LoadCharacterSelection();
        }
        else
        {
            if (message.Contains("exist"))
                errorText.text = "User does not exist. Please register first.";
            else
                errorText.text = message;

            SetButtons(true);
        }

    }

    bool Validate(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Fill all fields!";
            return false;
        }

        if (!email.Contains("@"))
        {
            errorText.text = "Invalid email!";
            return false;
        }

        if (password.Length < 6)
        {
            errorText.text = "Password too short!";
            return false;
        }

        return true;
    }

    void SetButtons(bool value)
    {
        loginButton.interactable = value;
        registerButton.interactable = value;
    }
}
