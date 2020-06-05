using Facebook.MiniJSON;
using Facebook.Unity;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is pretty self explanatory. Most methods here are taken more or less directly from the official tutorials.
/// </summary>
public class FirebaseLoginManager : MonoBehaviour
{
    #region Singleton Structure
    public static FirebaseLoginManager flm;

    public void Awake()
    {
        if (flm == null) flm = this;
        else if (flm != this) Destroy(this);

        FacebookAwake();
    }
    #endregion

    FirebaseUser user;

    //Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

    public TMP_InputField email, pw;
    
    #region Facebook Login

    void FacebookAwake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            FB.ActivateApp();
        }
    }

    void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.LogError("Failed to Initialize the Facebook SDK");
        }
    }

    void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void LoginFacebook()
    {
        Debug.Log("Logging in... (Facebook)");

        var perms = new List<string>() { "public_profile", "email", "user_friends" };

        try
        {
            FB.LogInWithReadPermissions(perms, FacebookLoginCallback);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    AccessToken fb_token;
    Credential fb_cred;

    void FacebookLoginCallback(ILoginResult res)
    {
        if(FB.IsLoggedIn)
        {
            fb_token = AccessToken.CurrentAccessToken;
            fb_cred = FacebookAuthProvider.GetCredential(fb_token.TokenString);
            
            FB.API("/me?fields=name,email,friends", HttpMethod.GET, FacebookSignupCallback_ParseUser);
        }
        else
        {
            Debug.LogError("Couldn't log in! If you cancelled the login manually ignore this message!");
        }
    }

    void FacebookSignupCallback_ParseUser(IGraphResult res)
    {
        Debug.Log(res.RawResult);

        var dictionary = (Dictionary<string, object>)Json.Deserialize(res.RawResult);

        string name = dictionary["name"]+"";
        string id = dictionary["id"]+"";
        string email = dictionary["email"] + "";

        Debug.Log(email);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("FacebookIDtoUID");

        _ref.GetValueAsync().ContinueWith(task =>
        {
            var _res = task.Result;

            if(_res.HasChild(id))
            {
                Debug.Log("Found user! Logging in!");

                Login(email, id);
            }
            else
            {
                Debug.Log("Couldn't find user! Signing them up!");

                // Definition of insecure. Don't let anyone datamine this!
                SignupWithData(email, id, name, id);
            }

        });
    }

    public void FacebookLogout()
    {
        FB.LogOut();
    }

    #endregion

    public void Login()
    {
        Debug.Log("Logging in...");
        Login(email.text, pw.text);
    }

    private void Login(string username, string password)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log("User " + FirebaseAuth.DefaultInstance.CurrentUser + " is already logged in! Please log out first!");
        }

        if (username == "" || password == "" || !isEmailAddress(username))
        {
            Debug.Log("Username or password have incorrect formatting! Aborting...");
            return;
        }

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(
            username, password).
            ContinueWith(_task =>
            {
                if (_task.IsCanceled)
                {
                    Debug.LogError("Login was canceled.");
                    return;
                }
                if (_task.IsFaulted)
                {
                    Debug.LogError("Login encountered an error: " + _task.Exception);
                    return;
                }

                user = _task.Result;

                if(FirebaseAuth.DefaultInstance.CurrentUser != null)
                    LoginSuccessful(username, password);
            });
    }
    
    public void LoginAnon()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log("User " + FirebaseAuth.DefaultInstance.CurrentUser + " is already logged in! Please log out first!");
        }

        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync()
            .ContinueWith(_task =>
            {
                if (_task.IsCanceled)
                {
                    Debug.LogError("Login Anon was canceled.");
                    return;
                }
                if (_task.IsFaulted)
                {
                    Debug.LogError("Login Anon encountered an error: " + _task.Exception);
                    return;
                }

                user = _task.Result;

                if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                    LoginSuccessful("Anon", "-none-");
            });
    }
    
    public void Signup()
    {
        Debug.Log("Registering new user...");
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log("User " + FirebaseAuth.DefaultInstance.CurrentUser + " is already logged in! Please log out first!");
        }

        string username = email.text;
        string password = pw.text;

        if(username == "" || password == "" || !isEmailAddress(username))
        {
            Debug.Log("Username or password have incorrect formatting! Aborting...");
            return;
        }

        SignupWithData(username, password);
    }

    /// <summary>
    /// Signup with added facebook capability
    /// FacebookArgs are:
    /// [0] =   Username
    /// [1] =   ID
    /// [2] =   Friend [0]
    /// [n+2] = Friend [n]
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="facebookArgs"></param>
    public void SignupWithData(string username, string password, params string[] facebookArgs)
    {
        /*
        if (facebookArgs.Length > 0)
        {
            Debug.LogError("Received facebook args; Debug aborting!");
            foreach (var s in facebookArgs) Debug.LogError(s);

            return;
        }*/

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(
            username, password).
            ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Signup was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Signup encountered an error: " + task.Exception);
                    return;
                }

                SignupSuccessful(username, password, facebookArgs);
            });
    }

    public void LoginSuccessful(string username, string password)
    {
        Debug.Log("User " + username + " logged in successfuly with password " + password);

        try
        {
            MenuManager.mm.Open_MainMenu_Delayed();
            //MenuManager.mm.StartGameJoinTestMenu_Delayed();

        } catch (System.Exception e)
        {
            Debug.Log(e);
        }

        //GetComponent<FirebaseDataManager>().SetTestData();
    }

    /// <summary>
    /// Signup with added facebook capability
    /// FacebookArgs are:
    /// [0] =   Username
    /// [1] =   ID
    /// [2] =   Friend [0]
    /// [n+2] = Friend [n]
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="facebookArgs"></param>
    public void SignupSuccessful(string username, string password, params string[] facebookArgs)
    {
        Debug.Log("Registered new User " + username + " with password " + password);
        if(facebookArgs.Length > 0)
            Debug.Log("Detected facebook data. Linking it with the user now!");

        FirebaseUser fbu = FirebaseAuth.DefaultInstance.CurrentUser;

        Debug.Log(fbu.UserId);

        //Create new user profile
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Profiles");
        
        //_ref.Push().SetValueAsync(fbu.UserId).ContinueWith;

        string path = "Profiles/"+fbu.UserId+"/";

        try
        {
            FirebaseDataManager.fdm.WriteData(path + "games_won", 0 + "");
            FirebaseDataManager.fdm.WriteData(path + "games_lost", 0 + "");

            if(facebookArgs.Length >= 2)
            {
                FirebaseDataManager.fdm.WriteData(path + "username", facebookArgs[0]);
                FirebaseDataManager.fdm.WriteData(path + "facebookUser/id", facebookArgs[1]);
                FirebaseDataManager.fdm.WriteData("FacebookIDtoUID/"+facebookArgs[1], fbu.UserId);
            }
            else
            {
                FirebaseDataManager.fdm.WriteData(path + "username", username);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            Login(username, password);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void Logout()
    {
        if (FB.IsInitialized && FB.IsLoggedIn) FacebookLogout();
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("Logged out!");
    }

    private void OnApplicationQuit()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Logout();
        }
    }

    public static bool isEmailAddress(string username)
    {
        if (username.Length < 6) return false;

        int atIndex = username.IndexOf('@');

        if (atIndex == -1) return false;

        int atCount = 0;
        foreach (char c in username) if (c == '@') atCount++;
        if (atCount > 1) return false;

        string ending = username.Substring(atIndex+1);

        int dotCount = 0;
        foreach (char c in username) if (c == '@') dotCount++;
        if (dotCount > 1) return false;

        return true;
    }
}