using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages Firebase initialization and authentication
/// Singleton + main-thread safe
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [Header("Firebase Status")]
    public bool isFirebaseReady = false;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;

    // Events
    public event Action OnFirebaseInitialized;
    public event Action<FirebaseUser> OnUserLoggedIn;
    public event Action OnUserLoggedOut;

    // Main-thread action queue
    private static readonly Queue<Action> mainThreadActions = new Queue<Action>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue()?.Invoke();
            }
        }
    }

    void RunOnMainThread(Action action)
    {
        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    void InitializeFirebase()
    {
        Debug.Log("🔥 Initializing Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            RunOnMainThread(() =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    isFirebaseReady = true;

                    Debug.Log("✅ Firebase initialized successfully!");

                    if (auth.CurrentUser != null)
                    {
                        currentUser = auth.CurrentUser;
                        Debug.Log($"👤 User already logged in: {currentUser.Email}");
                        OnUserLoggedIn?.Invoke(currentUser);
                    }

                    OnFirebaseInitialized?.Invoke();
                }
                else
                {
                    Debug.LogError($"❌ Firebase init failed: {task.Result}");
                    isFirebaseReady = false;
                }
            });
        });
    }

    // ================= AUTH =================

    public void LoginUser(string email, string password, Action<bool, string> callback)
    {
        if (!isFirebaseReady)
        {
            callback(false, "Firebase not ready yet!");
            return;
        }

        Debug.Log($"🔐 Logging in user: {email}");

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            RunOnMainThread(() =>
            {
                if (task.IsCanceled)
                {
                    callback(false, "Login canceled");
                    return;
                }

                if (task.IsFaulted)
                {
                    callback(false, GetFirebaseErrorMessage(task.Exception));
                    return;
                }

                currentUser = task.Result.User;
                Debug.Log($"✅ Logged in: {currentUser.Email}");
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

        Debug.Log($"📝 Registering user: {email}");

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            RunOnMainThread(() =>
            {
                if (task.IsCanceled)
                {
                    callback(false, "Registration canceled");
                    return;
                }

                if (task.IsFaulted)
                {
                    callback(false, GetFirebaseErrorMessage(task.Exception));
                    return;
                }

                currentUser = task.Result.User;
                Debug.Log($"✅ Registered: {currentUser.Email}");
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
    }

    public bool IsUserLoggedIn()
    {
        return auth != null && auth.CurrentUser != null;
    }

    public FirebaseUser GetCurrentUser()
    {
        return currentUser;
    }

    string GetFirebaseErrorMessage(AggregateException exception)
    {
        if (exception == null) return "Unknown error";

        foreach (var e in exception.Flatten().InnerExceptions)
        {
            if (e is FirebaseException firebaseEx)
            {
                switch ((AuthError)firebaseEx.ErrorCode)
                {
                    case AuthError.InvalidEmail: return "Invalid email address!";
                    case AuthError.WrongPassword: return "Wrong password!";
                    case AuthError.UserNotFound: return "Account does not exist!";
                    case AuthError.EmailAlreadyInUse: return "Email already registered!";
                    case AuthError.WeakPassword: return "Password too weak (min 6 chars)";
                    case AuthError.NetworkRequestFailed: return "Network error!";
                    default: return $"Error: {(AuthError)firebaseEx.ErrorCode}";
                }
            }
        }
        return "Authentication failed!";
    }
}
