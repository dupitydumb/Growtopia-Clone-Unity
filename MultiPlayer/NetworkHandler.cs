using Mirror;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Storage;
public class NetworkHandler : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField ipAddressInput;
    //Add log for network messages
    public LoginData loginData;
    NetworkManager manager;
    FirebaseAuth auth;
    FirebaseStorage storage;
    FirebaseUser user;
    public GameObject world;
    
    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        manager = GetComponent<NetworkManager>();
        Application.logMessageReceived += HandleLog; // Subscribe to log messages
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Optionally, filter for errors only
        if (type == LogType.Error || type == LogType.Exception)
        {
            LogUI.instance.AddLog(logString); // Add the error log to your LogUI
            // If you want to include stack trace information:
            // LogUI.instance.AddLog(logString + "\n" + stackTrace);
        }
    }
    void Start()
    {
        // LogUI.instance.AddLog("NetworkHandler Start");
        // LogUI.instance.AddLog("Network Address: " + manager.networkAddress);
        // if (Application.internetReachability == NetworkReachability.NotReachable)
        // {
        //     LogUI.instance.AddLog("No internet connection.");
        // }
        // else
        // {
        //     LogUI.instance.AddLog("Internet connection available.");
        // }
        // //Log The Player IP public IP
        StartCoroutine(GetPublicIP());


        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Firebase is ready for use
                LogUI.instance.AddLog("Firebase is ready for use");
            } else {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    public void Login(string email, string password) {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
            CheckForLoginData(newUser.UserId);

        });
    }

    public void SignUp(string email, string password, string username)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // User has been created successfully
            FirebaseUser newUser = task.Result.User;

            // Now, set the username as the user's display name
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = username,
            };
            newUser.UpdateUserProfileAsync(profile).ContinueWith(profileTask => {
                if (profileTask.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (profileTask.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + profileTask.Exception);
                    return;
                }

                // Profile updated successfully
                Debug.LogFormat("User signed up and profile updated successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);

                //Create local storage for the user
                var storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
                var userStorageRef = storage.GetReference($"users/{newUser.UserId}");
                userStorageRef.PutBytesAsync(new byte[1]).ContinueWith((task) => {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError(task.Exception.ToString());
                        // Handle the error...
                    }
                    else
                    {
                        // File uploaded successfully
                        Debug.Log("Created user storage.");
                    }
                });
            });
        });
        SetLoginData(auth.CurrentUser.UserId, username, email, password);
        //When sign up is successful, set the login data
        Login(email, password);
    }

    public void SetLoginData(string userId, string username, string email, string password)
    {
        // Create a new LoginData object
        LoginData loginData = new LoginData
        {
            username = username,
            email = email,
            password = password,
            lastLogin = DateTime.Now.ToString(),
            lastLogout = "",
            lastIP = ""
        };

        // Serialize the LoginData object to a JSON string
        string loginDataJson = JsonUtility.ToJson(loginData);

        // Save the JSON string to the user's storage space
        var storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        var userStorageRef = storage.GetReference($"users/{userId}/loginData.json");

        byte[] loginDataBytes = System.Text.Encoding.UTF8.GetBytes(loginDataJson);
        userStorageRef.PutBytesAsync(loginDataBytes).ContinueWith((task) => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError(task.Exception.ToString());
                // Handle the error...
            }
            else
            {
                // File uploaded successfully
                Debug.Log("Saved login data to user storage.");
                PlayerPrefs.SetString("userId", userId);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("email", email);
                PlayerPrefs.SetString("password", password);
            }
        });
    }
    private void CheckForLoginData(string userId)
    {
        Debug.Log("Checking for login data...");
        var storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
        var userStorageRef = storage.GetReference($"users/{userId}/loginData.json");

        userStorageRef.GetBytesAsync(1024 * 1024).ContinueWith(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError(task.Exception.ToString());
                SetLoginData(userId, auth.CurrentUser.DisplayName, auth.CurrentUser.Email, auth.CurrentUser.Email);
                LogUI.instance.AddLog("No login data found. Created new login data.");
                // Handle the error...
            }
            else
            {
                // File downloaded successfully
                string loginDataJson = System.Text.Encoding.UTF8.GetString(task.Result);
                loginData = JsonUtility.FromJson<LoginData>(loginDataJson);
                var username = loginData.username;
                LogUI.instance.AddLog($"Loaded login data for {username}.");
            }
        });
    }
    public void OnLoginButtonClicked() 
    {
        string username = emailInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        Login(email, password);
    }

    public void OnSignUpButtonClicked()
    {
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        SignUp(email, password, username);
    }
    void AuthStateChanged(object sender, EventArgs eventArgs) 
    {
        if (auth.CurrentUser != null) {
            // User is logged in
            Debug.Log("User is logged in");
        } else {
            // User is logged out
            Debug.Log("User is logged out");
        }
    }

    public IEnumerator GetPublicIP()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://icanhazip.com");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            LogUI.instance.AddLog("Public IP: " + request.downloadHandler.text);
        }
        else
        {
            LogUI.instance.AddLog("Error getting public IP: " + request.error);
        }

        //Log User login data
        if (auth.CurrentUser != null)
        {
            LogUI.instance.AddLog("User ID: " + auth.CurrentUser.UserId);
        }

    }

    // Call this method when the player submits their IP address
    public void ConnectToPlayerIP(string ipAddress)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            manager.networkAddress = ipAddress;
            LogUI.instance.AddLog($"Attempting to connect to: {ipAddress}");
            manager.StartClient(); // Start the client with the new IP address
        }
        else
        {
            LogUI.instance.AddLog("IP Address is empty.");
        }
    }

    public void HostWithIPAddress(string ipAddress)
    {
        if (!string.IsNullOrEmpty(ipAddress))
        {
            manager.networkAddress = ipAddress;
            LogUI.instance.AddLog($"Hosting at: {ipAddress}");
            manager.StartHost(); // Start hosting with the specified IP address
        }
        else
        {
            LogUI.instance.AddLog("IP Address is empty. Cannot host game.");
        }
    } 

    public void StartServer()
    {
        StartCoroutine(TryToJoinOrHost());
    }
    public IEnumerator TryToJoinOrHost()
    {
        string ipAddress = ipAddressInput.text;
        bool attemptToConnect = true;
        manager.networkAddress = ipAddress;

        // Attempt to start a client to connect to the host
        LogUI.instance.AddLog($"Attempting to connect to: {ipAddress}");
        manager.StartClient();

        // Give it some time to attempt a connection
        yield return new WaitForSeconds(3);

        // Check if the client is connected
        if (manager.isNetworkActive)
        {
            // Successfully connected to a game
            LogUI.instance.AddLog($"Joined game at: {ipAddress}");
            attemptToConnect = false;
        }
        else
        {
            // Failed to connect, stop the client
            LogUI.instance.AddLog($"Failed to join game at: {ipAddress}");
            manager.StopClient();
        }

        // If connection attempt failed, start hosting a game
        if (attemptToConnect)
        {
            LogUI.instance.AddLog($"No game found at: {ipAddress}. Hosting a new game.");
            manager.StartHost();
        }
    }
}

[Serializable]
public class LoginData
{
    public string username;
    public string email;
    public string password;
    public string lastLogin;
    public string lastLogout;
    public string lastIP;
}