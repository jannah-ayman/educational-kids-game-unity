using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Collections.Generic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [Header("Firebase Status")]
    public bool isFirebaseReady = false;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    public event Action OnFirebaseInitialized;
    public event Action<FirebaseUser> OnUserLoggedIn;
    public event Action OnUserLoggedOut;

    private static readonly Queue<Action> mainThreadActions = new Queue<Action>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
                mainThreadActions.Dequeue()?.Invoke();
        }
    }

    public static void RunOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            RunOnMainThread(() =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    isFirebaseReady = true;

                    if (auth.CurrentUser != null)
                    {
                        currentUser = auth.CurrentUser;
                        OnUserLoggedIn?.Invoke(currentUser);
                    }

                    OnFirebaseInitialized?.Invoke();
                }
                else
                {
                    isFirebaseReady = false;
                }
            });
        });
    }

    public void LoginUser(string email, string password, Action<bool, string> callback)
    {
        if (!isFirebaseReady)
        {
            callback(false, "Firebase not ready yet!");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            RunOnMainThread(() =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    // Friendly error instead of raw message
                    callback(false, "Wrong email or password.");
                    return;
                }

                currentUser = task.Result.User;
                OnUserLoggedIn?.Invoke(currentUser);
                callback(true, "Login successful!");
            });
        });
    }

    public void RegisterUser(string email, string password, Action<bool, string> callback)
    {
        if (!isFirebaseReady)
        {
            callback(false, "Firebase not ready yet!");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            RunOnMainThread(() =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    callback(false, "Failed to register. Check your email or password.");
                    return;
                }

                currentUser = task.Result.User;
                OnUserLoggedIn?.Invoke(currentUser);
                callback(true, "Registration successful!");
            });
        });
    }

    public void LogoutUser()
    {
        if (auth != null)
        {
            auth.SignOut();
            currentUser = null;
            OnUserLoggedOut?.Invoke();
        }
        CharacterManager.Instance?.ResetDefaults();
    }

    public bool IsUserLoggedIn() => auth != null && auth.CurrentUser != null;

    public FirebaseUser GetCurrentUser() => currentUser;
}
