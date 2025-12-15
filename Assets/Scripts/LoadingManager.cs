using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public TMP_Text loadingText;

    string baseText = "Connecting";
    int dotCount = 0;
    bool isConnecting = true;

    void Start()
    {
        StartCoroutine(AnimateDots());
        StartCoroutine(LoadingFlow());
    }

    IEnumerator AnimateDots()
    {
        while (isConnecting)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator LoadingFlow()
    {
        loadingText.text = "Starting...";
        yield return new WaitForSeconds(0.5f);

        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.isFirebaseReady)
        {
            baseText = "Connecting";
            yield return new WaitForSeconds(0.2f);
        }

        isConnecting = false;
        loadingText.text = "Checking login...";
        yield return new WaitForSeconds(0.5f);

        if (FirebaseManager.Instance.IsUserLoggedIn())
        {
            Debug.Log("✅ Auto-login success");
            SceneLoader.Instance.LoadMainMenu();
        }
        else
        {
            Debug.Log("❌ Not logged in");
            SceneLoader.Instance.LoadLogin();
        }
    }
}
