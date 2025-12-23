using Firebase.Database;
using UnityEngine;
using System.Collections.Generic;
public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public string playerName = "Player";
    public int selectedCharacter = 0;

    public string databaseURL = "https://educational-kids-game-un-4ef4d-default-rtdb.firebaseio.com";

    private DatabaseReference databaseRef;
    public bool IsDataLoaded { get; private set; } = false;
    public System.Action OnCharacterDataLoaded;

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
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.isFirebaseReady)
        {
            LoadFromFirebase();
        }
        else
        {
            FirebaseManager.Instance.OnFirebaseInitialized += LoadFromFirebase;
        }
    }

    public void LoadFromFirebase()
    {
        databaseRef = FirebaseDatabase.GetInstance(databaseURL).RootReference;

        IsDataLoaded = false;

        if (!FirebaseManager.Instance.IsUserLoggedIn())
        {
            IsDataLoaded = true;
            OnCharacterDataLoaded?.Invoke();
            return;
        }

        string userId = FirebaseManager.Instance.GetCurrentUser().UserId;

        databaseRef
            .Child("users").Child(userId)
            .GetValueAsync().ContinueWith(task =>
            {
                FirebaseManager.RunOnMainThread(() =>
                {
                    if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    {
                        var snapshot = task.Result;

                        playerName = snapshot.Child("name").Value?.ToString() ?? "Player";

                        string charStr = snapshot.Child("character").Value?.ToString() ?? "0";
                        int.TryParse(charStr, out selectedCharacter);

                        if (selectedCharacter < 0 || selectedCharacter >= 2)
                            selectedCharacter = 0;

                        Debug.Log($"Loaded from DB: {playerName}, Character {selectedCharacter}");
                    }
                    else
                    {
                        Debug.Log("No saved data, using defaults");
                    }

                    IsDataLoaded = true;
                    OnCharacterDataLoaded?.Invoke();
                });
            });
    }
    public void SaveToFirebase()
    {
        if (!FirebaseManager.Instance.IsUserLoggedIn())
        {
            Debug.LogWarning("Can't save - not logged in");
            return;
        }

        string userId = FirebaseManager.Instance.GetCurrentUser().UserId;

        var data = new Dictionary<string, object>
        {
            { "name", playerName },
            { "character", selectedCharacter }
        };

        databaseRef
            .Child("users").Child(userId)
            .SetValueAsync(data);
    }
    public void ResetDefaults()
    {
        playerName = "Player";
        selectedCharacter = 0;
        IsDataLoaded = false;
        OnCharacterDataLoaded = null; 
    }
}