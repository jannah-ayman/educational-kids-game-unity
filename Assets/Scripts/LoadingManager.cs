using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("UI References")]
    public Text loadingText;
    public Image logo;

    private string baseText = "Loading";
    private int dotCount = 0;

    void Start()
    {
        // Start animations
        StartCoroutine(AnimateLoadingText());
        StartCoroutine(CheckLoginAndRedirect());
    }

    IEnumerator AnimateLoadingText()
    {
        // Animate dots: Loading. .. ...
        while (true)
        {
            dotCount = (dotCount + 1) % 4;
            string dots = new string('.', dotCount);
            loadingText.text = baseText + dots;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator CheckLoginAndRedirect()
    {
        // Wait 2 seconds to show logo
        yield return new WaitForSeconds(4f);

        // TODO: Check Firebase login when TA responds
        // For now, always go to Login
        bool isLoggedIn = CheckIfLoggedIn();

        if (isLoggedIn)
        {
            // User is logged in, go to main menu
            SceneLoader.Instance.LoadMainMenu();
        }
        else
        {
            // Not logged in, go to login screen
            SceneLoader.Instance.LoadLogin();
        }
    }

    bool CheckIfLoggedIn()
    {
        // TEMPORARY - returns false until Firebase ready
        // After TA responds, replace with:
        // return FirebaseAuth.DefaultInstance.CurrentUser != null;
        return false;
    }
}