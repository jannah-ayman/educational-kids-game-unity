using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToMenuButton : MonoBehaviour
{
    public void OnReturnToMenuClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        SceneLoader.Instance.LoadMainMenu();
    }
}
