using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // Used for Singleton structure to be accessible everywhere
    public static MenuManager mm;

    public TextMeshProUGUI versionText;

    // Login Menu Root Reference
    [Header("Login Menu")]
    public RectTransform loginMenu;

    [Header("Main Menu")]
    public RectTransform mainMenu;
    public RectTransform infosMenu;

    [Header("Result Menu")]
    public RectTransform resultMenu;
    public TMP_Text scoreText;

    //otherProfile Menu Root Reference
    [Header("Other Profile Menu")]
    public RectTransform otherProfileMenu;

    //ownProfile Menu Root Reference
    [Header("Own Profile Menu")]
    public RectTransform ownProfileMenu;

    //friendlist Menu Root Reference
    [Header("Friend List Menu")]
    public RectTransform friendListMenu;

    //Nemesis Menu Root Reference
    [Header("Nemesis List Menu")]
    public RectTransform nemesisMenu;

    //leaderboard Menu root Reference
    [Header("Leaderboard Menu")]
    public RectTransform leaderboardMenu;

    // Lobby Menu Root Reference
    [Header("Lobby Menu")]
    public RectTransform lobbyMenu;

    public TextMeshProUGUI lobbyNameDisplay;
    public TextMeshProUGUI lobbyReadyCountDisplay;
    
    //Buttons
    public Button lobbyButtonMobileInfos;
    public Button lobbyButtonChat;
    public Button lobbyButtonSwitchRole;
    public Button lobbyButtonReady;
    public Button lobbyButtonStart;
    public Button lobbyButtonLeave;

    public Button[] lobbyButtonSwapIndices;

    //2 & 4 player panels
    public RectTransform lobbyPanel2Players;
    public RectTransform lobbyPanel4Players;

    //Player name texts version 2 players;
    public TextMeshProUGUI lobbyTextP1Name_2P;
    public TextMeshProUGUI lobbyTextP2Name_2P;

    //Player name texts version 4 players
    public TextMeshProUGUI lobbyTextP1Name;
    public TextMeshProUGUI lobbyTextP2Name;
    public TextMeshProUGUI lobbyTextP3Name;
    public TextMeshProUGUI lobbyTextP4Name;


    //Chat 
    public RectTransform lobbyChatPopup;
    public TextMeshProUGUI lobbyChat;
    public TMP_InputField lobbyChatInputField;


    // Game Root Reference
    [Header("Game References")]
    public RectTransform gameMenu;

    public Camera runnerCam;
    public Camera builderCam;

    public RectTransform builderInterface;

    [Header("Lobby Menu")]
    public RectTransform gameJoinTestMenu;
    public TMP_Text List_ActiveLobbies;
    public TMP_InputField InputField_LobbyID;

    [Header("Mobile Infos")]
    public RectTransform mobileInfos;

    /// <summary>
    /// Init Singleton, update the Lobby List and start the Lobby Menu Update Loop.
    /// The first update call in this function is probably not necessary; I will remove it at a later point
    /// </summary>
    private void Awake()
    {
        Debug.Log(System.DateTime.UtcNow.Ticks);

        if (mm == null) mm = this;
        else if (mm != this) Destroy(this);

        if(versionText)
            versionText.text = "v" + Application.version + "\nMade with Unity v" + Application.unityVersion;

        //Update_GJTM_LobbyList();

        StartCoroutine(Launch_FLM_AutoUpdateLoop());
    }

    IEnumerator Launch_FLM_AutoUpdateLoop()
    {
        yield return new WaitUntil(() => FirebaseLobbyManager.flm != null);
        Update_GJTM_LobbyList();
        FirebaseLobbyManager.flm.AutoUpdateLoop(Update_GJTM_LobbyList);
    }

    #region Open Menu Methods
    /*############### Main Methods for opening menues will be located here ###############*/

    /// <summary>
    /// Indicates which menu will be opened during the next update step:
    /// <para/>0: Login
    /// <para/>1: Lobby Finder
    /// <para/>2: Lobby
    /// <para/>3: Game
    /// <para/>4: Own Profile
    /// <para/>5: Friend List
    /// <para/>6: Nemesis List
    /// <para/>20: Main Menu (from Login)
    /// </summary>
    int menu_auto_open = -1;
    bool firstStepAfterEnter = false;

    /// <summary>
    /// Trigger for delayed menu openings. Makes opening menues from callback functions possible
    /// </summary>
    private void Update()
    {
        // Update Player List
        if (FirebaseLobbyManager.flm.is_in_lobby != "")
        {
            if(firstStepAfterEnter)
            {
                if(FirebaseLobbyManager.flm.is_Hosting)
                {
                    lobbyButtonStart.interactable = true;
                    lobbyButtonStart.gameObject.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    lobbyButtonStart.interactable = false;
                    lobbyButtonStart.gameObject.GetComponent<Image>().color = Color.gray;
                }

                lobbyButtonChat.interactable = true;
                lobbyButtonLeave.interactable = true;
                lobbyButtonMobileInfos.interactable = true;
                lobbyButtonReady.interactable = true;
                lobbyButtonSwitchRole.interactable = true;
            }

            string lobbyName = FirebaseLobbyManager.flm.is_in_lobby;

            if (lobbyName.StartsWith("lobby_")) lobbyName = lobbyName.Substring("lobby_".Length);
            lobbyNameDisplay.text = "Lobby: " + lobbyName;

            int rdy_count = FirebaseLobbyManager.flm.rdy_count;
            lobbyReadyCountDisplay.text = "Ready: " + rdy_count;

            string[] currentPlayers = FirebaseLobbyManager.flm.playerArray;

            if (currentPlayers == null || currentPlayers.Length != 4)
            {
                Debug.LogError("Couldn't assign current player list!");
                return;
            }

            /*
            Debug.Log(currentPlayers.Length);
            for (int i = 0; i < currentPlayers.Length; i++)
                Debug.Log(currentPlayers[i]);*/

            if (currentPlayers[2] == null && currentPlayers[3] == null)
            {
                lobbyPanel2Players.gameObject.SetActive(true);
                lobbyPanel4Players.gameObject.SetActive(false);

                if (currentPlayers[0] != null) lobbyTextP1Name_2P.text = currentPlayers[0];
                else lobbyTextP1Name_2P.text = "Player 1 is missing";
                if (currentPlayers[1] != null) lobbyTextP2Name_2P.text = currentPlayers[1];
                else lobbyTextP2Name_2P.text = "Player 2 is missing";
            }
            else
            {
                lobbyPanel2Players.gameObject.SetActive(false);
                lobbyPanel4Players.gameObject.SetActive(true);

                if (currentPlayers[0] != null) lobbyTextP1Name.text = currentPlayers[0];
                else lobbyTextP1Name.text = "Player 1 is missing";
                if (currentPlayers[1] != null) lobbyTextP2Name.text = currentPlayers[1];
                else lobbyTextP2Name.text = "Player 2 is missing";
                if (currentPlayers[2] != null) lobbyTextP3Name.text = currentPlayers[2];
                else lobbyTextP3Name.text = "Player 3 is missing";
                if (currentPlayers[3] != null) lobbyTextP4Name.text = currentPlayers[3];
                else lobbyTextP4Name.text = "Player 4 is missing";
            }


            /*            List<string> currentPlayerList = FirebaseLobbyManager.flm.playerList;

                        if (currentPlayerList.Count <= 2)
                        {
                            lobbyPanel2Players.gameObject.SetActive(true);
                            lobbyPanel4Players.gameObject.SetActive(false);

                            if (currentPlayerList.Count >= 1) lobbyTextP1Name_2P.text = currentPlayerList[0];
                            else lobbyTextP1Name_2P.text = "Player 1 missing!";
                            if (currentPlayerList.Count >= 2) lobbyTextP2Name_2P.text = currentPlayerList[1];
                            else lobbyTextP2Name_2P.text = "Player 2 missing!";
                        }
                        else
                        {
                            lobbyPanel2Players.gameObject.SetActive(false);
                            lobbyPanel4Players.gameObject.SetActive(true);

                            if (currentPlayerList.Count >= 1) lobbyTextP1Name.text = currentPlayerList[0];
                            else lobbyTextP1Name.text = "Player 1 missing!";
                            if (currentPlayerList.Count >= 2) lobbyTextP2Name.text = currentPlayerList[1];
                            else lobbyTextP2Name.text = "Player 2 missing!";
                            if (currentPlayerList.Count >= 3) lobbyTextP3Name.text = currentPlayerList[2];
                            else lobbyTextP2Name.text = "Player 3 missing!";
                            if (currentPlayerList.Count >= 4) lobbyTextP4Name.text = currentPlayerList[3];
                            else lobbyTextP3Name.text = "Player 4 missing!";
                        }*/

            /*
            if (currentPlayerList != lobbyPlayerList.text)
                lobbyPlayerList.text = currentPlayerList;*/
        }

        // Open Menu Delayed
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
                StartGame();
                break;
            case 4:
                Open_OwnProfile();
                break;
            case 5:
                Open_FriendList();
                break;
            case 6:
                Open_NemesisList();
                break;
            case 7:
                Open_Leaderboard();
                break;
            case 8:
                Open_ResultMenu();
                break;
            case 20:
                Open_MainMenu();
                break;
        }

        menu_auto_open = -1;
    }

    #region Menu Methods
    // The following methods directly open their respective menu

    // Login Menu

    #region Login Menu
    public void Open_MainMenu()
    {
        loginMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    public void Open_MainMenu_Delayed()
    {
        menu_auto_open = 20;
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
    #endregion

    // Main Menu

    #region Main Menu
    int sublistStatus = 0;  // Used to determine if the Friend/Nemesis List or nemesis have been opened
                            // 0 = Main Menu
                            // 1 = Friend List
                            // 2 = Nemesis List
                            // 3 = Leaderboard

    // Open Own Profile
    public void Open_OwnProfile()
    {
        mainMenu.gameObject.SetActive(false);
        ownProfileMenu.gameObject.SetActive(true);
        OwnProfileManager.opm.UpdateValues();
    }

    public void Open_OwnProfile_Delayed()
    {
        menu_auto_open = 4;
    }

    // Open Friend List
    public void Open_FriendList()
    {
        sublistStatus = 1;

        mainMenu.gameObject.SetActive(false);
        friendListMenu.gameObject.SetActive(true);
        FriendlistMenuManager.fmm.LoadFriends();
    }

    public void Open_FriendList_Delayed()
    {
        menu_auto_open = 5;
    }

    // Open Nemesis List
    public void Open_NemesisList()
    {
        sublistStatus = 2;

        mainMenu.gameObject.SetActive(false);
        nemesisMenu.gameObject.SetActive(true);
        NemesisMenuManager.nmm.LoadNemesis();
    }

    public void Open_NemesisList_Delayed()
    {
        menu_auto_open = 6;
    }

    // Open Leaderboard
    public void Open_Leaderboard()
    {
        sublistStatus = 3;

        mainMenu.gameObject.SetActive(false);
        leaderboardMenu.gameObject.SetActive(true);

        GetComponent<LeaderboardMenuManager>().LoadLeaderboard();
    }

    public void Open_Leaderboard_Delayed()
    {
        menu_auto_open = 7;
    }
    
    public void Open_LoginMenu()
    {
        mainMenu.gameObject.SetActive(false);
        loginMenu.gameObject.SetActive(true);
        
        FirebaseLobbyManager.flm.OnApplicationQuit();
        FirebaseLoginManager.flm.Logout();
    }

    public void Open_Infos()
    {
        mainMenu.gameObject.SetActive(false);
        infosMenu.gameObject.SetActive(true);
    }

    public void Close_Infos()
    {
        infosMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    #endregion

    // Stats/Profile (own)

    #region Own Profile
    public void Return_OwnProfile()
    {
        ownProfileMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
    #endregion

    // Stats/Profile (other Players)

    #region Other Profile
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

    public void Refresh_OtherProfile()
    {

    }
    #endregion

    // Friends List

    #region Friend List
    public void Return_FriendList()
    {
        friendListMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // TODO: Another parameter might be more useful
    public void Open_FriendProfile(string uid)
    {
        friendListMenu.gameObject.SetActive(false);
        otherProfileMenu.gameObject.SetActive(true);

        OtherProfileManager.opm.UpdateValues(uid);
    }

    public void Open_FriendProfile_Delayed(List<string> data)
    {

    }
    #endregion

    // Nemesis List

    #region Nemesis List
    public void Return_NemesisList()
    {
        nemesisMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // TODO: Another parameter might be more useful
    public void Open_NemesisProfile(string uid)
    {
        nemesisMenu.gameObject.SetActive(false);
        otherProfileMenu.gameObject.SetActive(true);

        OtherProfileManager.opm.UpdateValues(uid);
    }

    public void Open_NemesisProfile_Delayed(List<string> data)
    {

    }
    #endregion

    // Leaderboard

    #region Leaderboard

    public void Return_Leaderboard()
    {
        leaderboardMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }

    // TODO: Another parameter might be more useful

    public void Open_LeaderboardProfile(string uid)
    {
        leaderboardMenu.gameObject.SetActive(false);
        otherProfileMenu.gameObject.SetActive(true);

        OtherProfileManager.opm.UpdateValues(uid);
    }

    public void Open_LeaderboardProfile_Delayed(List<string> data)
    {

    }

    #endregion

    // Lobby Finder

    #region Lobby Finder
    public void Open_Lobby()
    {
        HostOrJoinLobby();
    }

    public void Open_Lobby_Delayed()
    {
        
    }

    public void Return_LobbyFinder()
    {
        gameJoinTestMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
    #endregion

    // Lobby Menu 

    #region Lobby Menu
               
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
                firstStepAfterEnter = true;
                return;
            }
        }

        Debug.Log("Couldn't find lobby! Creating it...");
        FirebaseLobbyManager.flm.HostLobby(LID, true);
        firstStepAfterEnter = true;
    }
    #endregion

    // Lobby

    #region Lobby
    //int popupCount = 0;

    public void Open_MobileInfos_Popup()
    {
        //if (popupCount > 0) return;

        //popupCount++;
        mobileInfos.gameObject.SetActive(true);

        MobileInfosManager.mim.UpdateContent();
    }

    bool ready = false;

    public void Ready(RectTransform rt)
    {
        //if (popupCount > 0) return;

        if (ready) rt.GetComponent<Image>().color = Color.white;
        else rt.GetComponent<Image>().color = Color.red;

        Ready();
    }

    public void Ready()
    {
        ready = !ready;
        FirebaseLobbyManager.flm.SetReady(ready);
    }

    public void StartGameForParty()
    {
        //if (popupCount > 0) return;

        try
        {
            Debug.Log("Starting game (1)!");

            FirebaseLobbyManager.flm.StartGame(FirebaseLobbyManager.flm.is_in_lobby);

        } catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting game (2)!");

        //if (popupCount > 0) Close_MobileInfos_Popup();

        lobbyMenu.gameObject.SetActive(false);
        gameMenu.gameObject.SetActive(true);

        int PID = FirebaseLobbyManager.flm.playerID + 1;
        string lobby_name = FirebaseLobbyManager.flm.is_in_lobby;
        if (lobby_name.StartsWith("lobby_")) lobby_name = lobby_name.Substring("lobby_".Length);

        Debug.Log("Initializing game with parameters: ID = " + PID + "; PC = " + FirebaseLobbyManager.flm.player_count + "; LN = " + lobby_name);

        byte weather = FirebaseLobbyManager.flm.weatherData;
        int tileset = MobileInfosManager.WeatherToTileSet(weather);

        MainGameManager.mgm.InitGame(PID, FirebaseLobbyManager.flm.player_count, lobby_name, tileset);
    }

    public void Update_Lobby_PlayerList()
    {
        Debug.LogError("Not implemented yet!");
    }

    public void Update_Lobby_PlayerList(string data)
    {
        Debug.LogError("Should be obsolete!");
        //lobbyPlayerList.text = data;
    }
    
    public void Update_Lobby_Chat(string data)
    {
        Debug.Log("Obtained new Lobby Chat Info: " + data);

    }

    int selectedNew = 0;

    public void StartSwapIndicesSubRoutine()
    {
        //if (popupCount > 0) return;

        Debug.Log("Swapping Indices");

        switch(FirebaseLobbyManager.flm.player_count)
        {
            case 0:
                return;
            case 1:
                FirebaseLobbyManager.flm.SetIndex((FirebaseLobbyManager.flm.playerID == 0) ? 1 : 0);
                return;
            case 2:
                FirebaseLobbyManager.flm.SwitchWithIndex((FirebaseLobbyManager.flm.playerID == 0) ? 1 : 0);
                return;
        }

        foreach (Button b in lobbyButtonSwapIndices)
        {
            b.interactable = true;
        }

        StartCoroutine(SwapIndicesSubRoutine());
    }

    public void SetSelectedNew(int i)
    {
        selectedNew = i;
    }

    IEnumerator SwapIndicesSubRoutine()
    {
        yield return new WaitUntil(() => selectedNew != 0);

        if (selectedNew == FirebaseLobbyManager.flm.playerID)
            selectedNew = 0;

        int tmp = selectedNew - 1;
        selectedNew = 0;

        FirebaseLobbyManager.flm.SwitchWithIndex(tmp);
    }
    #endregion

    // Mobile Infos

    #region Mobile Infos
    public void Close_MobileInfos_Popup()
    {
        Debug.Log("Closing Mobile Infos Popup");
        //popupCount--;
        //if (popupCount < 0) popupCount = 0;

        mobileInfos.gameObject.SetActive(false);
    }
    #endregion

    // Game

    #region Game
    public void EndGame()
    {
        Open_ResultMenu_Delayed();     
    }

    private void Open_ResultMenu()
    {
        gameMenu.gameObject.SetActive(false);
        resultMenu.gameObject.SetActive(true);

        MainGameManager.mgm.ClearRemainingGameObjects();
    }

    private void Open_ResultMenu_Delayed()
    {
        menu_auto_open = 8;
    }

    public void Game_Setup_Builder()
    {
        builderCam.gameObject.SetActive(true);
        runnerCam.gameObject.SetActive(false);

        builderInterface.gameObject.SetActive(true);
    }

    public void Game_Setup_Runner()
    {
        runnerCam.gameObject.SetActive(true);
        builderCam.gameObject.SetActive(false);

        builderInterface.gameObject.SetActive(false);
    }

    public void AbortGameStart()
    {
        gameMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
    #endregion

    // Result

    #region Result Menu
    public void Open_LobbyMenu_Result()
    {
        FirebaseLobbyManager.flm.SetReady(false);

        // Call existing callbacks if necessary
        if (FirebaseLobbyManager.flm.is_in_lobby == "")
        {
            Debug.LogError("Error! Lobby not found! Opening Lobby Finder instead!");
            Open_LobbyFinder_Result();
            return;
        }

        if(FirebaseLobbyManager.flm.is_Hosting)
        {
            FirebaseLobbyManager.flm.StopGame(FirebaseLobbyManager.flm.is_in_lobby);
        }

        lobbyMenu.gameObject.SetActive(true);
        resultMenu.gameObject.SetActive(false);
    }

    public void Open_LobbyFinder_Result()
    {
        if (FirebaseLobbyManager.flm.is_in_lobby != "")
            FirebaseLobbyManager.flm.LeaveLobby(FirebaseLobbyManager.flm.is_in_lobby);

        // Call existing callbacks if necessary
        gameJoinTestMenu.gameObject.SetActive(true);
        resultMenu.gameObject.SetActive(false);
    }

    public void Open_MainMenu_Result()
    {
        if(FirebaseLobbyManager.flm.is_in_lobby != "")
            FirebaseLobbyManager.flm.LeaveLobby(FirebaseLobbyManager.flm.is_in_lobby);
        
        // Call existing callbacks if necessary
        mainMenu.gameObject.SetActive(true);
        resultMenu.gameObject.SetActive(false);
    }
    #endregion

    #region Deprecated
    public void StartGameJoinTestMenu()
    {
        Debug.Log("Opening Lobby Finder Test Menu");

        if(FirebaseLobbyManager.flm.is_in_lobby != "")
        {
            FirebaseLobbyManager.flm.LeaveLobby(FirebaseLobbyManager.flm.is_in_lobby);
        } 

        mainMenu.gameObject.SetActive(false);
        gameJoinTestMenu.gameObject.SetActive(true);     
    }
    
    public void StartLoginMenu()
    {
        Debug.Log("Opening Login Menu");

        FirebaseLobbyManager.flm.OnApplicationQuit();

        loginMenu.gameObject.SetActive(true);
        gameJoinTestMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(false);

        FirebaseLoginManager.flm.Logout();
    }

    public void StartLobbyMenu()
    {
        lobbyMenu.gameObject.SetActive(true);
        gameJoinTestMenu.gameObject.SetActive(false);
    }

    public void LeaveLobbyMenu()
    {
        gameJoinTestMenu.gameObject.SetActive(true);
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

    public void StartGame_Delayed()
    {
        Debug.Log("Opening Start Game with delay");

        menu_auto_open = 3;
    }
    #endregion

    #endregion

}
