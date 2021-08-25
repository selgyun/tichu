using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;


public class AuthManager : MonoBehaviour
{
    // check
    public bool IsFirebaseReady { get; private set; }
    public bool IsSignInOnProgress { get; private set; }

    // Unity UI
    public InputField emailField;
    public InputField passwordField;
    public Button signInButton;
    public Button registerButton;

    // firebase 
    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;

    public static FirebaseUser User;
    void Start()
    {
        signInButton.interactable = false;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var result = task.Result;

            if (result != DependencyStatus.Available)
            {
                Debug.LogError(result.ToString());
                IsFirebaseReady = false;
            }
            else
            {
                IsFirebaseReady = true;

                firebaseApp = FirebaseApp.DefaultInstance;
                firebaseAuth = FirebaseAuth.DefaultInstance;
            }

            signInButton.interactable = IsFirebaseReady;
        });
    }

    public void SignIn()
    {
        if (!IsFirebaseReady || IsSignInOnProgress || User != null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInButton.interactable = false;
        
        // ContinueWith 로 나중에 바꾸기!
        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Sign in status : {task.Result}");

            IsSignInOnProgress = false;
            signInButton.interactable = true;

            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else if(task.IsCanceled)
            {
                Debug.LogError("Sign in canceled");
            }
            else
            {
                User = task.Result;
                Debug.Log(User.Email);
                SceneManager.LoadScene("Lobby");
            }
        });
    }
}
