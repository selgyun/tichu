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
    public bool IsFirebaseReady { get; private set; }
    [Header("Login UI")]
    public InputField emailField;
    public InputField passwordField;
    public Button signInButton;
    public Button registerButton;
    public Text warningLoginText;

    [Header("Register UI")]
    public InputField usernameField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField passwordVerifyField;
    public Text warningRegisterText;

    [Header("Firebase")]
    // firebase 
    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;

    public static FirebaseUser User;
    private void Awake()
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

    public void SignInButton()
    {
        StartCoroutine(SignIn(emailField.text, passwordField.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameField.text));
    }

    private IEnumerator SignIn(string _email, string _password)
    {
        signInButton.interactable = false;

        var LoginTask = firebaseAuth.SignInWithEmailAndPasswordAsync(_email, _password);

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message; 
            signInButton.interactable = true;
        }
        else
        {
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "Login Success";

            yield return new WaitForSeconds(1.0f);

            //IsSignInOnProgress = false;
            signInButton.interactable = true;
            SceneManager.LoadScene("Lobby");
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if(_username == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordVerifyField.text)
        {
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            var RegisterTask = firebaseAuth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }

                warningRegisterText.text = message;
            }
            else
            {
                User = RegisterTask.Result;

                if (User != null)
                {
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        warningRegisterText.text = "Register Success!";
                    }
                }
            }
        }
    }
}
