using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using Firebase.Auth;
using TMPro;
using UnityEngine.UI;
using Facebook.MiniJSON;

public class FacebookHandler : MonoBehaviour
{
    private FirebaseAuth mAuth;

    public Text userName;
    public Text userID;
    public Image profilePicture;

    public int score;

    public Text FriendsText;
    public Text FriendsID;

    //----------------------------------------------------------------------------
    // Facebook Initialization
    //----------------------------------------------------------------------------

    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialisiert die Facebook App (SDK)
            FB.Init(InitCallback, OnHideUnity);
            txt += "Inited FB\n";
            res.text = txt;
        }
        else
        {
            // Bereits initialisiert, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void Start()
    {
        if(profilePicture)
            profilePicture.enabled = false;
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            txt += "Initialized the Facebook SDK";
            res.text += txt;
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
            txt += "Failed to Initialize the Facebook SDK";
            res.text += txt;
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }



    //----------------------------------------------------------------------------
    // Facebook Login and Logout
    //----------------------------------------------------------------------------
    /*
    bool temp = false;
    Credential c;
    void Update()
    {
        if (temp)
        {
            Debug.Log("Temp trigger");
            accessToken(c);
            temp = false;
        }
    }
    */

    string txt = "";

    public void FacebookLogin()
    {
        Debug.Log("Login Start");

        // User wird beim Login gefragt ob er diese 3 Informationen mit uns teilen möchte
        var perms = new List<string>() { "public_profile", "email", "user_friends" };

        txt = "";
        // Rufe LoginWithReadPermissions auf und gebe permissions und callback mit
        try
        {
            FB.LogInWithReadPermissions(perms, FacebookOnLogin);
        }
        catch (System.Exception e)
        {
            txt += e;
        }
        res.text = txt;
    }

    public void FacebookLogout()
    {
        FB.LogOut();
    }



    public TextMeshProUGUI res;

    private void FacebookOnLogin(ILoginResult result)
    {
        Debug.Log("Login Callback;"+FB.IsLoggedIn);
        try
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("a");
                if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                {
                    Debug.Log("User " + FirebaseAuth.DefaultInstance.CurrentUser + " is already logged in! Please log out first!");
                }

                // AccessToken class hat alle session details
                AccessToken token = AccessToken.CurrentAccessToken;

                Debug.Log("b");

                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;

                Debug.Log("Token 1: "+token + "\nToken 2:" + aToken + "\nToken 3:" + userID);

                if(userID) userID.text = result.AccessToken.UserId;

                Debug.Log(result.AccessToken.UserId);
                //Credential credential = FacebookAuthProvider.GetCredential(token.TokenString);
                //c = credential;
                //temp = true;

                mAuth = FirebaseAuth.DefaultInstance;

                Credential credential = FacebookAuthProvider.GetCredential(token.TokenString);

                Debug.Log(token.ToString()+"\n\n\n"+token.TokenString);
                //FirebaseAuth.DefaultInstance.CurrentUser.LinkWithCredentialAsync(credential);
                
               // FB.API
              //  FirebaseLoginManager.flm.Signup(token.)

                /*
                FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWith(task => {
                    Debug.Log("Continue (" + task.IsCanceled + ";" + task.IsFaulted + ";" + task.IsCompleted+")");
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithCredentialAsync was canceled.");
                        txt += "SignInWithCredentialAsync was canceled.";
                        res.text = txt;
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.LogError((task == null) ? null : task);
                        Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                        txt += "SignInWithCredentialAsync encountered an error: " + task.Exception;
                        res.text = txt;
                        return;
                    }

                    res.text = txt;

                    Firebase.Auth.FirebaseUser newUser = task.Result;

                    txt += "User signed in successfully: " + newUser.DisplayName + "(" + newUser.UserId + ")";
                    res.text = txt;

                    FirebaseLoginManager.flm.LoginSuccessful();

                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);
                    
                    res.text = txt;
                    // Nach dem Login sollen alle Nutzerdaten aktualisiert werden
                    GetUserData();

                    foreach (string perm in token.Permissions)
                    {
                        Debug.Log(perm);
                    }
                });*/
            }
            else
            {
                txt += "\nLogin failed!";
                Debug.Log("Login Failed");
                return;
            }
            Debug.Log("Facebook Login succesful!");
            res.text = txt;
        }
        catch (System.Exception e)
        {
            txt += e;
            Debug.LogError(e);
        }
    }

    //----------------------------------------------------------------------------
    // Facebook <-> Firebase Connection
    //----------------------------------------------------------------------------

    public void accessToken(AccessToken accessToken_)
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        Firebase.Auth.Credential credential =
            Firebase.Auth.FacebookAuthProvider.GetCredential(accessToken_.TokenString);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });

        /*
        if (!FB.IsLoggedIn)
        {
            return;
        }
        Debug.Log("Jumped into accesToken method");
        
        try
        {
            Debug.Log("Checkpoint 1");
            
            auth.SignInWithCredentialAsync(c).ContinueWith(task =>
            {
                Debug.Log("Checkpoint 2");
                
                //Debug.Log(task.Result.ToString());
                
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }
                Debug.Log("Checkpoint 3");
                
                FirebaseUser newUser = task.Result;

                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);

                Debug.Log("Checkpoint 4");
                    
            });
        } catch(System.Exception e)
        {
            Debug.LogError(e);
        }
    }*/
    }

    //----------------------------------------------------------------------------
    // Facebook Graph API Zugriffe
    //----------------------------------------------------------------------------

    public void GetUserData()
    {
        // Facebook Namen auslesen
        FB.API("/me?fields=name", HttpMethod.GET, UserNameCallback);

        // Facebook Profilfoto auslesen
        FB.API("/me/picture?height=200&width=200", HttpMethod.GET, ProfilePictureCallback);
    }

    public void UserNameCallback(IResult result)
    {
        if(result.Error == null)
        {
            userName.text = "Hallo " + result.ResultDictionary["name"] + "!";
        }
        else
        {
            print(result.Error);
        }
    }

    public void ProfilePictureCallback(IGraphResult result)
    {
        if(result.Texture != null)
        {
            profilePicture.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 200, 200), new Vector2());
            profilePicture.enabled = true;
        }
    }

    // Veraltet, seit v3.12 Graph API nicht mehr möglich
    /*
    // Methode um persönlichen Score hochzuladen
    public void PostScore(int score)
    {
        var scoreData = new Dictionary<string, string>();
        scoreData["score"] = score.ToString();

        FB.API("/me/scores", HttpMethod.POST, PostScoreCallback, scoreData);
    }

    public void PostScoreCallback(IResult result)
    {
        print("Highscore übertragen: " + result.RawResult);
    }
    */

    // Method for getting all players who play this game
    public void GetFriendsPlayingThisGame()
    {
        string query = "/me/taggable_friends";
        FB.API(query, HttpMethod.GET, result =>
        {
            if(result.Error == null)
            {
                var dictionary = (Dictionary<string, object>)Json.Deserialize(result.RawResult);
                var friendsList = (List<object>)dictionary["data"];
                FriendsText.text = string.Empty;
                foreach (var dict in friendsList)
                    FriendsText.text += ((Dictionary<string, object>)dict)["total_count"];
            }
            else
            {
                print(result.Error);
            }
        });
    }

    // Method for getting player id's from players who play this game
    public void GetFacebookUserID()
    {
        string query = "/me/friends";
        FB.API(query, HttpMethod.GET, result =>
        {
            var dictionary = (Dictionary<string, object>)Json.Deserialize(result.RawResult);
            var friendsList = (List<object>)dictionary["data"];
            FriendsID.text = string.Empty;
            foreach (var dict in friendsList)
                FriendsID.text += ((Dictionary<string, object>)dict)["id"];
        });
    }
}