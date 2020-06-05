using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Meine Ideen oder Ansätze:
 * -removeFriend() methode
 * */

public class MenuManagerK : MonoBehaviour
{
    // Used for Singleton structure to be accessible everywhere
    public static MenuManagerK mm;

    // Login Menu Root Reference
    public RectTransform loginMenu;

    //Main Menu Root Reference
    public RectTransform mainMenu;

    //Result Menu Root Reference
    public RectTransform ResultMenu;

    //otherProfile Menu Root Reference
    public RectTransform otherProfileMenu;

    //ownProfile Menu Root Reference
    public RectTransform ownProfileMenu;

    //friendlist Menu Root Reference
    public RectTransform friendListMenu;

    //Nemesis Menu Root Reference
    public RectTransform nemesisMenu;

    //leaderboard Menu root Reference
    public RectTransform leaderboardMenu;


    // Lobby Menu Root Reference
    public RectTransform lobbyMenu;

    /// <summary>
    /// Init Singleton, update the Lobby List and start the Lobby Menu Update Loop.
    /// The first update call in this function is probably not necessary; I will remove it at a later point
    /// </summary>
    private void Awake()
    {
        if (mm == null) mm = this;
        else if (mm != this) Destroy(this);

        Update_GJTM_LobbyList();

        FirebaseLobbyManager.flm.AutoUpdateLoop(Update_GJTM_LobbyList);
    }

    #region Open Menu Methods
    /*############### Main Methods for opening menues will be located here ###############*/

    int menu_auto_open = -1;

    /// <summary>
    /// Trigger for delayed menu openings. Makes opening menues from callback functions possible
    /// </summary>
    private void Update()
    {
        if (menu_auto_open == -1) return;

        switch (menu_auto_open)
        {
            case 0:
                StartLoginMenu();
                break;
            case 1:
                StartGameJoinTestMenu();
                break;
            case 2:
                StartLobbyMenu();
                break;
            case 3:
                Open_MainMenuFromLogin();
                break;
            case 4:
                Open_OwnProfile();
                break;
        }

        menu_auto_open = -1;
    }

    #region Menu Methods
    // The following methods directly open their respective menu

    // Login Menu

    #region Login Menu
    public void Open_MainMenuFromLogin()
    {
        loginMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    public void Open_MainMenu()
    {

    }

    public void Open_MainMenu_Delayed()
    {

    }

    public void Open_MainMenuFromLogin_Delayed()
    {
        Debug.Log("Opening Main Menu with delay");

        menu_auto_open = 3;
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
    #endregion

    // Main Menu

    #region Main Menu
    int sublistStatus = 0;  // Used to determine if the Friend/Nemesis List or Leaderboard have been opened
                            // 0 = Main Menu
                            // 1 = Friend List
                            // 2 = Nemesis List
                            // 3 = Leaderboard

    public void Return_ToMainMenu(RectTransform latest)
    {
        latest.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    public void Open_OwnProfile()
    {
        mainMenu.gameObject.SetActive(false);
        ownProfileMenu.gameObject.SetActive(true);
    }

    public void Open_OwnProfile_Delayed()
    {
        Debug.Log("Delayed Profile opening");
        //ownProfileMenu.gameObject.SetActive(true);
        
        menu_auto_open = 4;
    }

    public void Open_FriendList()
    {
        sublistStatus = 1;
        mainMenu.gameObject.SetActive(false);
        friendListMenu.gameObject.SetActive(true);
    }

    public void Open_FriendList_Delayed()
    {

    }

    public void Open_NemesisList()
    {
        sublistStatus = 2;
        mainMenu.gameObject.SetActive(false);
        nemesisMenu.gameObject.SetActive(true);
    }

    public void Open_NemesisList_Delayed()
    {

    }

    public void Open_Leaderboard()
    {
        sublistStatus = 3;
        mainMenu.gameObject.SetActive(false);
        leaderboardMenu.gameObject.SetActive(true);
    }

    public void Open_Leaderboard_Delayed()
    {

    }

    #endregion

    // Stats/Profile (own)
    public void Return_OwnProfile()
    {
        ownProfileMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // Stats/Profile (other Players)
    public void Return_OtherProfile()
    {
        switch (sublistStatus)
        {
            case 1:
                //Return to Friend List
                otherProfileMenu.gameObject.SetActive(false);
                friendListMenu.gameObject.SetActive(true);
                break;
            case 2:
                //Return to Nemesis List
                otherProfileMenu.gameObject.SetActive(false);
                nemesisMenu.gameObject.SetActive(true);
                break;
            case 3:
                //Return to Leaderboard
                otherProfileMenu.gameObject.SetActive(false);
                leaderboardMenu.gameObject.SetActive(true);
                break;
            default:
                //Return to Main Menu
                otherProfileMenu.gameObject.SetActive(false);
                mainMenu.gameObject.SetActive(true);
                break;
        }
    }

    // Friends List
    public void Return_FriendList()
    {
        friendListMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // TODO: Another parameter might be more useful
    public void Open_FriendProfile(int id)
    {
        friendListMenu.gameObject.SetActive(false);
        otherProfileMenu.gameObject.SetActive(true);
    }

    public void Open_FriendProfile_Delayed(List<string> data)
    {

    }

    // Nemesis List
    public void Return_NemesisList()
    {
        nemesisMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // TODO: Another parameter might be more useful
    public void Open_NemesisProfile(int id)
    {
        nemesisMenu.gameObject.SetActive(false);
        otherProfileMenu.gameObject.SetActive(true);
    }

    public void Open_NemesisProfile_Delayed(List<string> data)
    {

    }

    // Leaderboard
    public void Return_Leaderboard()
    {
        leaderboardMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // TODO: Another parameter might be more useful
    public void Open_LeaderboardProfile(int id)
    {
        leaderboardMenu.gameObject.SetActive(false);
        otherProfileMenu.gameObject.SetActive(true);
    }

    public void Open_LeaderboardProfile_Delayed(List<string> data)
    {

    }

    // Lobby Finder
    public void Open_Lobby()
    {
        HostOrJoinLobby();
    }

    public void Open_Lobby_Delayed()
    {

    }

    #region Lobby Menu Methods

    // Game Join Test Menu References

    [Header("Lobby Menu")]
    public RectTransform gameJoinTestMenu;
    public TMP_Text List_ActiveLobbies;
    public TMP_InputField InputField_LobbyID;

    /* An overall note on the data/method structure used here:
     * 
     * The database structure can be found in assignment02\Wichtiges\Guidelines\firebase_database_structures.json
     * 
     * The overall method structure for database accesses is relatively simple:
     *  1. A public method is called normally (via buttons or another script)
     *  2. The public method calls an async function usually located within the FirebaseLoginManager with a callback function (most of the time a local private functon) as additional parameter
     *  3. The async function accesses the firebase database functionality and retrievees data from a certain path.
     *  4. The async function then parses the retrieved data and calls the callback function with the parsed data as a parameter
     *  5. The callback function uses the data to apply changes to the menu
     */

    bool GJTM_LobbyList_delay = false;

    /// <summary>
    /// Update the lobby list
    /// </summary>
    public void Update_GJTM_LobbyList()
    {
        FirebaseLobbyManager.flm.RetrieveActiveLobbies(_Update_GJTM_LobbyList);
        StartCoroutine(_Update_GJTM_LobbyList_Delay());
    }

    /// <summary>
    /// Equal to Update_GJTM_LobbyList but with a .5 sec delay
    /// </summary>
    public void DelayedAutoUpdate()
    {
        StartCoroutine(_DelayedAutoUpdate());
    }

    /// <summary>
    /// Delays the update, see above for details
    /// </summary>
    /// <returns></returns>
    public IEnumerator _DelayedAutoUpdate()
    {
        yield return new WaitForSeconds(.5f);

        Update_GJTM_LobbyList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lobbies"></param>
    void _Update_GJTM_LobbyList(List<FirebaseLobbyManager.Lobby> lobbies)
    {
        string str = "Active Lobbies:\n\tName\t\t\t\t|\tPlayer Count";

        foreach (FirebaseLobbyManager.Lobby l in lobbies)
        {
            str += "\n\t" + l.name + "\t\t\t|\t" + l.player_count;
        }

        //Due to callback issues this error will ALWAYS be thrown so no need to worry, just ignore it!
#pragma warning disable CS0168 // Variable is declared but never used
        try
        {
            List_ActiveLobbies.text = str;
        }
        catch (System.Exception e)
        {
            //Debug.Log(e);
        }
#pragma warning restore CS0168 // Variable is declared but never used

        GJTM_LobbyList_delay = true;
    }

    /// <summary>
    /// Concats a " " to the lobby string to force unity to update it
    /// On-data-changed event seems to make this unnessecary so it's kind of deprecated and will likely be removed in the future
    /// </summary>
    /// <returns></returns>
    IEnumerator _Update_GJTM_LobbyList_Delay()
    {
        yield return new WaitUntil(() => GJTM_LobbyList_delay);
        List_ActiveLobbies.text += " ";
        GJTM_LobbyList_delay = false;
        yield break;
    }

    /// <summary>
    /// Clear all lobbies from the database. This method will be removed in the future as it is a safety hazard. Instead lobbies will be automatically deleted once there are no players in it.
    /// </summary>
    public void WipeLobbies()
    {
        FirebaseLobbyManager.flm.WipeLobbies();
        DelayedAutoUpdate();
    }

    /// <summary>
    /// Host or Join a lobby based on it's id. Automatically syncs with firebase and checks if the lobby already exists. If it does the user will automatically join, if it doesn't the server hosts a new lobby with the player as the host.
    /// </summary>
    public void HostOrJoinLobby()
    {
        FirebaseLobbyManager.flm.RetrieveActiveLobbies(HostOrJoinLobby);
        DelayedAutoUpdate();
    }

    /// <summary>
    /// Host or Join Lobby callback. NEVER call this directly unless you know exactly what you're doing!
    /// </summary>
    /// <param name="lobbies"></param>
    void HostOrJoinLobby(List<FirebaseLobbyManager.Lobby> lobbies)
    {
        string LID = InputField_LobbyID.text;
        Debug.Log("Checking lobbies for occurence of " + LID);

        foreach (FirebaseLobbyManager.Lobby l in lobbies)
        {
            if (("lobby_" + LID) == l.name)
            {
                Debug.Log("Found lobby! Joining...");
                FirebaseLobbyManager.flm.JoinLobby(LID, true);
                return;
            }
        }

        Debug.Log("Couldn't find lobby! Creating it...");
        FirebaseLobbyManager.flm.HostLobby(LID, true);
    }
    #endregion

    // Lobby
    public void Open_MobileInfos_Popup()
    {

    }

    public void Ready()
    {

    }

    public void StartGame()
    {

    }

    // Mobile Infos
    public void Close_MobileInfos_Popup()
    {

    }

    // Game
    public void EndGame()
    {

    }

    private void Open_ResultMenu()
    {

    }

    private void Open_ResultMenu_Delayed()
    {

    }

    // Result
    public void Open_LobbyMenu_Result()
    {
        // Call existing callbacks if necessary
    }

    public void Open_LobbyFinder_Result()
    {
        // Call existing callbacks if necessary
    }

    public void Open_MainMenu_Result()
    {
        // Call existing callbacks if necessary
    }

    #region Deprecated
    public void StartGameJoinTestMenu()
    {
        Debug.Log("Opening Lobby Finder Test Menu");

        if (FirebaseLobbyManager.flm.is_in_lobby != "")
        {
            FirebaseLobbyManager.flm.LeaveLobby(FirebaseLobbyManager.flm.is_in_lobby);
        }

        loginMenu.gameObject.SetActive(false);
        gameJoinTestMenu.gameObject.SetActive(true);
    }

    public void StartLoginMenu()
    {
        Debug.Log("Opening Login Menu");

        FirebaseLobbyManager.flm.OnApplicationQuit();

        loginMenu.gameObject.SetActive(true);
        mainMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(false);

        FirebaseLoginManager.flm.Logout();
    }

    public void StartLobbyMenu()
    {
        lobbyMenu.gameObject.SetActive(true);
        mainMenu.gameObject.SetActive(false);
    }

    public void LeaveLobbyMenu()
    {
        mainMenu.gameObject.SetActive(true);
        lobbyMenu.gameObject.SetActive(false);

        FirebaseLobbyManager.flm.LeaveLobby(FirebaseLobbyManager.flm.is_in_lobby);
    }
    #endregion
    #endregion

    #region Delayed Open Methods
    // The following methods open their respective menu indirectly after up to a single frame delay

    public void StartLoginMenu_Delayed()
    {
        Debug.Log("Opening Login Menu with delay");

        menu_auto_open = 0;
    }

    public void StartGameJoinTestMenu_Delayed()
    {
        Debug.Log("Opening Lobby Finder Test Menu with delay");

        menu_auto_open = 1;
    }

    public void StartLobbyMenu_Delayed()
    {
        Debug.Log("Opening Lobby Menu with delay");

        menu_auto_open = 2;
    }
    #endregion

    #endregion
}
