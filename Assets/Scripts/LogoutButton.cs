using UnityEngine;

public class LogoutButton : MonoBehaviour
{
    public void OnLogoutClicked()
    {
        Debug.Log("👋 Logout clicked");
        AudioManager.Instance.PlayButtonClick();
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.LogoutUser();
        }

        // Close popup if you want
        //gameObject.SetActive(false);

        // Go back to login screen
        SceneLoader.Instance.LoadLogin();
    }
}
