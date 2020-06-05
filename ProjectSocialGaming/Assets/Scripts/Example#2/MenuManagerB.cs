using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManagerB : MonoBehaviour
{
    // Used for Singleton structure to be accessible everywhere
    public static MenuManagerB mm;

    // Login Menu Root Reference
    public RectTransform loginMenu;

    // Game Join Test Menu References
    public RectTransform gameJoinTestMenu;
    public TMP_Text List_ActiveLobbies;
    public TMP_InputField InputField_LobbyID;

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
        }

        menu_auto_open = -1;
    }

    #region Open Methods
    // The following methods directly open their respective menu

    public void StartGameJoinTestMenu()
    {
        Debug.Log("Opening Lobby Finder Test Menu");

        if(FirebaseLobbyManager.flm.is_in_lobby != "")
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
        gameJoinTestMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(false);

        FirebaseLoginManager.flm.Logout();
    }

    public void StartLobbyMenu()
    {
        loginMenu.gameObject.SetActive(false);
        lobbyMenu.gameObject.SetActive(true);

        FirebaseLoginManager.flm.Logout();
    }

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

    #region Lobby Menu Methods

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
    public void DelayedAutoUpdate() {
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
        try
        {
            List_ActiveLobbies.text = str;
        } catch(System.Exception e)
        {
            //Debug.Log(e);
        }        

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
            if (("lobby_"+LID) == l.name)
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
}
