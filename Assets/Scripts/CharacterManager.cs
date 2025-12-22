using Firebase.Database;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    [Header("Character Images")]
    public Sprite[] characterSprites;

    [Header("Current Selection")]
    public string playerName = "Player";
    public int selectedCharacter = 0;

    [Header("Firebase Settings")]
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
        // Wait for FirebaseManager to be ready
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.isFirebaseReady)
        {
            InitDatabaseAndLoad();
        }
        else
        {
            FirebaseManager.Instance.OnFirebaseInitialized += InitDatabaseAndLoad;
        }
    }

    void InitDatabaseAndLoad()
    {
        // Firebase is ready
        databaseRef = FirebaseDatabase.GetInstance(databaseURL).RootReference;
        LoadFromFirebase();
    }
    void OnEnable()
    { // Always try to load data when this scene is active
        if (!IsDataLoaded)
            LoadFromFirebase();
    }
    public void LoadFromFirebase()
    {
        IsDataLoaded = false;

        // Check if user is logged in
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsUserLoggedIn())
        {
            Debug.Log("⚠️ Not logged in, using defaults");
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

                        if (selectedCharacter < 0 || selectedCharacter >= characterSprites.Length)
                            selectedCharacter = 0;

                        Debug.Log($"✅ Loaded from DB: {playerName}, Character {selectedCharacter}");
                    }
                    else
                    {
                        Debug.Log("📝 No saved data, using defaults");
                    }

                    IsDataLoaded = true;
                    OnCharacterDataLoaded?.Invoke();
                });
            });
    }

    public void SaveToFirebase()
    {
        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsUserLoggedIn())
        {
            Debug.LogWarning("⚠️ Can't save - not logged in");
            return;
        }

        string userId = FirebaseManager.Instance.GetCurrentUser().UserId;

        var data = new System.Collections.Generic.Dictionary<string, object>
        {
            { "name", playerName },
            { "character", selectedCharacter }
        };

        databaseRef
            .Child("users").Child(userId)
            .SetValueAsync(data).ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ Saved: {playerName}, Character {selectedCharacter}");
                }
                else
                {
                    Debug.LogError("❌ Save failed!");
                }
            });
    }
    public void ResetDefaults()
    {
        playerName = "Player";
        selectedCharacter = 0;
        IsDataLoaded = false;
        OnCharacterDataLoaded = null; // <--- clear previous subscribers
    }

    public void LoadFromFirebase(System.Action onLoaded)
    {
        OnCharacterDataLoaded += onLoaded;
        LoadFromFirebase();
    }

    public Sprite GetCurrentCharacterSprite()
    {
        if (selectedCharacter >= 0 && selectedCharacter < characterSprites.Length)
            return characterSprites[selectedCharacter];

        return characterSprites.Length > 0 ? characterSprites[0] : null;
    }
}
