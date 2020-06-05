using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Firebase access side of the lobby manager part of the menu manager
/// </summary>
public class FirebaseLobbyManager : MonoBehaviour
{
    #region Singleton Structure
    public static FirebaseLobbyManager flm;

    private void Awake()
    {
        if (flm == null) flm = this;
        else if (flm != this) Destroy(this);
    }
    #endregion

    public struct Lobby
    {
        public string name;
        public int player_count;
    }

    public bool is_Hosting = false; // False if the User joined an existing lobby; True if they hosted it
    public string is_in_lobby = ""; // Stores the name of the current lobby
    public int playerID = -1;
    public List<string> playerList = new List<string>();
    public string[] playerArray = new string[4];

    /// <summary>
    /// Prints all lobbies from the database to the console
    /// </summary>
    public void PrintActiveLobbies()
    {
        RetrieveActiveLobbies(Debug.Log);
    }

    /// <summary>
    /// Retrieves lobby data from the database, parses it and calls the callback
    /// </summary>
    /// <param name="callbackFunction"></param>
    public void RetrieveActiveLobbies(System.Action<List<Lobby>> callbackFunction)
    {
        string path = "Lobbies";
        //Debug.Log("Reading lobbies from '" + path + "'");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        DatabaseReference __ref = _ref.Child(path);
        __ref.GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;
            //Debug.Log("Retrieved snapshot: " + ds.ToString()+"\nDissecting it now!");
            
            //Debug.Log("child count: "+ds.ChildrenCount);

            List<Lobby> lobbies = new List<Lobby>();
            List<DataSnapshot> to_remove = new List<DataSnapshot>();

            try
            {

                foreach (DataSnapshot _ds in ds.Children)
                {
                    if (!_ds.Key.StartsWith("lobby_"))
                    {
                        to_remove.Add(_ds);
                        continue;
                    }

                    lobbies.Add(new Lobby()
                    {
                        name = _ds.Key,
                        player_count = int.Parse(_ds.Child("players/player_count").Value.ToString())
                    });
                    //Debug.Log("Found lobby " + _ds.Key + " with " + _ds.Child("player_count").Value + " players");
                }
            }
            catch(System.Exception e)
            {
                Debug.Log(e);
            }
            
            try
            {
                callbackFunction(lobbies);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }

            // Auto-clean unwanted id strings
            FirebaseDataManager.fdm.DeleteData(__ref, ds, to_remove);
        });
    }
    
    /// <summary>
    /// Host an already authenticated Lobby. Creates a new entry for the lobby on the server with the current user as the host.
    /// CANNOT be used by anonymous users
    /// </summary>
    /// <param name="lobby_name"></param>
    /// <param name="name_authenticated"></param>
    public void HostLobby(string lobby_name, bool name_authenticated)
    {
        if (!name_authenticated) throw new System.Exception("Non-authenticated name not supported right now!");

        bool anon = false;

        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
            anon = true;

        if(!anon)
            Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser.UserId + " tries to create a lobby!");

        string lobby_name_full = "lobby_" + lobby_name;
        string UID = (!anon) ? FirebaseAuth.DefaultInstance.CurrentUser.UserId : "anonymous";

        playerID = 0;

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child("Lobbies").Push().SetValueAsync(lobby_name_full);

        //FirebaseDataManager.fdm.WriteData("Lobbies", lobby_name_full);
        FirebaseDataManager.fdm.WriteData("Lobbies/" + lobby_name_full + "/players/player_count", "" + 1);
        FirebaseDataManager.fdm.WriteData("Lobbies/" + lobby_name_full + "/players/player_0", UID);
        FirebaseDataManager.fdm.WriteData("Lobbies/" + lobby_name_full + "/ready_count", "" + 0);

        is_in_lobby = lobby_name_full;
        is_Hosting = true;

        playerList.Clear();
        playerList.Add(UID);

        // Sub to all updates
        try
        {
            SubscribeToLobbyChat(lobby_name);
            SubscribeToLobbyPlayerList(lobby_name);
            SubscribeToReadyCount(lobby_name);
            SubscribeToGameStart(lobby_name);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e);
        }

        try
        {
            SyncWeatherData((byte)0, (byte)0);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        MenuManager.mm.StartLobbyMenu_Delayed();
    }

    /// <summary>
    /// Join target existing lobby
    /// </summary>
    /// <param name="lobby_name"></param>
    /// <param name="name_authenticated"></param>
    public void JoinLobby(string lobby_name, bool name_authenticated)
    {
        if (!name_authenticated) { Debug.LogWarning("Non-authenticated name not supported right now!"); return; }
        
        if(is_in_lobby == lobby_name) { Debug.Log("Already in lobby " + lobby_name); return; }

        if (is_in_lobby != "") { Debug.Log("Leaving current lobby!"); LeaveLobby(is_in_lobby); }

        is_in_lobby = lobby_name;
        is_Hosting = false;

        string childPath = "lobby_" + lobby_name + "/players/player_count";

        //Debug.Log(childPath);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies").Child(childPath);
        
        _ref.GetValueAsync().ContinueWith(task => {
            int currentPlayerCount = int.Parse(task.Result.Value + "");

            Debug.Log("Player count is "+currentPlayerCount);

            if(currentPlayerCount >= 4)
            {
                Debug.Log("Lobby full! Aborting...");
                return;
            }

            _ref.Parent.GetValueAsync().ContinueWith(_task =>
            {
                var res = new List<DataSnapshot>(_task.Result.Children);
                               
                playerID = -1;
                
                for(int i = 0; i < 4; i++)
                {
                    bool found = false;

                    foreach(DataSnapshot ds in res)
                    {
                        if (ds.Key == "player_" + i)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) continue;

                    playerID = i;
                    break;
                    /*
                    for(int j = 0; j < 4; j++)
                    {
                        if (res[j].Key == "player_"+i)
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        playerID = i;
                        break;
                    }*/
                }

                if(playerID == -1)
                {
                    Debug.LogError("Player ID invalid, aborting!");
                    return;
                }

                currentPlayerCount++;

                _ref.SetValueAsync(currentPlayerCount);

                bool anon = false;
                if (FirebaseAuth.DefaultInstance.CurrentUser == null)
                    anon = true;
                string UID = (!anon) ? FirebaseAuth.DefaultInstance.CurrentUser.UserId : "anonymous";

                _ref.Parent.Child("player_" + playerID).SetValueAsync(UID);

                try
                {
                    // Subbing all updates
                    SubscribeToLobbyPlayerList(lobby_name);
                    SubscribeToLobbyChat(lobby_name);
                    SubscribeToReadyCount(lobby_name);
                    SubscribeToGameStart(lobby_name);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }

                MenuManager.mm.StartLobbyMenu_Delayed();
            });
        });
    }
    
    public void LeaveLobby(string lobby_name)
    {
        if(isRdy) MenuManager.mm.Ready();
        
        MenuManager.mm.lobbyButtonReady.GetComponent<Image>().color = Color.white;
        isRdy = false;


        if (lobby_name.Equals("")) lobby_name = is_in_lobby;
        if (!lobby_name.Equals(is_in_lobby)) return;

        Debug.Log("Leaving lobby " + lobby_name);
                
        // Unsubbing from all updates
        UnsubscribeFromLobbyPlayerList(lobby_name);
        UnsubscribeFromLobbyChat(lobby_name);
        UnsubscribeFromReadyCount(lobby_name);
        UnsubscribeFromGameStart(lobby_name);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies").Child("lobby_"+lobby_name);

        _ref.GetValueAsync().ContinueWith((data) => {
                       
            if (is_Hosting)
            {
                Debug.Log("Was host of lobby " + lobby_name);
                DeleteLobby(lobby_name);
            }
            else
            {
                Debug.Log("Was not host of lobby " + lobby_name + ". Retrieving player count!");

                _ref.Child("players/player_count").GetValueAsync().ContinueWith(task =>
                {
                    Debug.Log(task.Result);
                    int player_count = int.Parse(task.Result.Value + "");

                    Debug.Log("Player count is " + player_count);

                    if(player_count <= 1)
                    {
                        Debug.Log("Lobby will be empty after leaving! Thus deleting it!");
                        DeleteLobby("lobby_"+lobby_name);
                    }
                    else
                    {
                        Debug.Log("Deleting player id and decrementing player count!");
                        player_count--;

                        _ref.Child("players/player_" + playerID).RemoveValueAsync();
                        _ref.Child("players/player_count").SetValueAsync(player_count);
                    }
                });
            }
            is_in_lobby = "";
            is_Hosting = false;
        });

    }

    public void StartGame(string lobby_name)
    {
        Debug.Log("Checking conditions:\n" +
            "is_in_lobby != lobby_name: " + (is_in_lobby != lobby_name) + "\n" +
            "!is_Hosting: " + (!is_Hosting) + "\n" +
            "player count = " + player_count + "\n" +
            "rdy_count" + rdy_count + "\n" +
            "player_count != rdy_count: " + (player_count != rdy_count) + "\n" +
            "player_count == 2: " + (player_count == 2) + "\n" +
            "player_count == 4: " + (player_count == 4) + "\n" +
            "!(player_count == 2 || player_count == 4)): " + (!(player_count == 2 || player_count == 4)));

        if (is_in_lobby != lobby_name || 
            !is_Hosting || 
            player_count != rdy_count || 
            !(player_count == 2 || player_count == 4)) return;

        Debug.Log("Setting Game Started to 1");

        try
        {
            DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

            if (lobby_name.StartsWith("lobby_")) lobby_name = lobby_name.Substring("lobby_".Length);

            _ref.Child("Lobbies/lobby_" + lobby_name + "/game_started").SetValueAsync(1);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void StopGame(string lobby_name)
    {
        try
        {
            DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

            if (lobby_name.StartsWith("lobby_")) lobby_name = lobby_name.Substring("lobby_".Length);

            _ref.Child("Lobbies/lobby_" + lobby_name + "/game_started").SetValueAsync(1);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    #region Chat Subscription
    public void SubscribeToLobbyChat(string lobby_name)
    {

    }

    public void UnsubscribeFromLobbyChat(string lobby_name)
    {

    }

    public void UpdateLobbyChatTrigger(object sender, ChildChangedEventArgs args)
    {
        Debug.Log(args.Snapshot.ToString());
    }
    #endregion

    #region Player List Subscription
    public void SubscribeToLobbyPlayerList(string lobby_name)
    {
        string path = "Lobbies/lobby_" + lobby_name + "/players";

        Debug.Log("Subscribing to Player List at " + path);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(path);

        try
        {
            _ref.ChildAdded += UpdateLobbyPlayerList;
            _ref.ChildChanged += UpdateLobbyPlayerList;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        _ref.GetValueAsync().ContinueWith(task =>
        {
            try
            {
                UpdateLobbyPlayerList(null, null);
                //UpdateLobbyPlayerList_Parsed(task.Result);
            }
            catch(System.Exception e)
            {
                Debug.Log(e);
            }
        });
    }

    public void UnsubscribeFromLobbyPlayerList(string lobby_name)
    {
        string path = "Lobbies/lobby_" + lobby_name + "/players";

        Debug.Log("Unsubscribing from Player List at " + path);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(path);

        _ref.ChildAdded -= UpdateLobbyPlayerList;
        _ref.ChildChanged -= UpdateLobbyPlayerList;
    }

    public void UpdateLobbyPlayerList(object sender, ChildChangedEventArgs args)
    {
        string lobby_name = is_in_lobby;
        if (lobby_name.StartsWith("lobby_")) lobby_name = lobby_name.Substring("lobby_".Length);

        Debug.Log("Updating player list of lobby!");

        //UpdateLobbyPlayerList_Parsed(args.Snapshot);
        string path = "Lobbies/lobby_" + lobby_name + "/players";
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
        _ref.GetValueAsync().ContinueWith(task =>
        {
            var res = task.Result;

            if (!res.HasChild("player_count"))
                Debug.LogError("Player Count not found!");

            player_count = int.Parse(res.Child("player_count").Value + "");

            string[] names = new string[4];

            foreach (DataSnapshot ds in res.Children)
            {
                if (ds.Key == "player_count") continue;

                string _index = ds.Key;
                string _name = ds.Value + "";

                if (_index.StartsWith("player_")) _index = _index.Substring("player_".Length);

                int __index = int.Parse(_index);

                names[__index] = _name;
            }

            Debug.Log("Printing new names array:");

            for (int i = 0; i < 4; i++)
                Debug.Log(names[i]);



            playerArray = names;
        });
    }

    public void UpdateLobbyPlayerList_Parsed(DataSnapshot ds)
    {
        Debug.Log("Received new snapshot to update player list with (" + ds.ToString() + ")");
        Debug.Log(ds.Value);
        if (ds.Value is long)
        {
            Debug.Log("Received new player count. Updating entire player list!");
            player_count = int.Parse(ds.Value + "");

            DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies/lobby_" + is_in_lobby + "/players");
            _ref.GetValueAsync().ContinueWith(task =>
            {
                if (task.Result.Value is long) return;

                UpdateLobbyPlayerList_Parsed(task.Result);
            });
            return;
        }
        else if(ds.Value is string)
        {
            return;
        }

        Debug.Log("Detected Player List change! ("+ds.ToString()+")");

        Dictionary<string, object> d = ds.Value as Dictionary<string, object>;

        player_count = int.Parse(d["player_count"]+"");

        d.Remove("player_count");

        string newPlayerText = "";

        foreach(var e in d)
        {
            if(!playerList.Contains(e.Value+""))
                playerList.Add(e.Value+"");
            newPlayerText += "" + e.Value + "\n";
        }

        Debug.Log(newPlayerText);

        //playerList = "Players:\n"+newPlayerText;
    }
    #endregion

    #region Ready Count Subscription
    public void SubscribeToReadyCount(string lobby_name)
    {
        string path = "Lobbies/lobby_" + lobby_name + "/ready_count";

        Debug.Log("Subscribing to ready count at " + path);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).ValueChanged += ReadyCount_Changed;     
    }

    public void UnsubscribeFromReadyCount(string lobby_name)
    {
        string path = "Lobbies/lobby_" + lobby_name + "/ready_count";

        Debug.Log("Unsubscribing from ready count at " + path);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).ValueChanged -= ReadyCount_Changed;
    }

    private void ReadyCount_Changed(object sender, ValueChangedEventArgs e)
    {
        int tmp = rdy_count;
        rdy_count = int.Parse(e.Snapshot.Value + "");

        Debug.Log("Ready count changed from " + tmp + " to " + rdy_count);
        Debug.Log("Player count is " + player_count);
    }
    #endregion

    #region GameStartSubscription
    public void SubscribeToGameStart(string lobby_name)
    {
        string path = "Lobbies/lobby_" + lobby_name + "/game_started";

        Debug.Log("Subscribing to ready count at " + path);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).ValueChanged += GameStart_Changed;
    }

    public void UnsubscribeFromGameStart(string lobby_name)
    {
        string path = "Lobbies/lobby_" + lobby_name + "/game_started";

        Debug.Log("Unsubscribing from ready count at " + path);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).ValueChanged -= GameStart_Changed;
    }

    private void GameStart_Changed(object sender, ValueChangedEventArgs e)
    {
        Debug.Log("Game Start Change Detected: "+e.Snapshot.ToString());

        int gameStart = int.Parse(e.Snapshot.Value + "");
        
        if(gameStart == 1)
        {
            MenuManager.mm.StartGame_Delayed();
        }

        Debug.Log("Game Start initiated!");
    }
    #endregion

    public void DeleteLobby(string lobby_name)
    {
        Debug.Log("Deleting lobby " + lobby_name);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        DatabaseReference __ref = _ref.Child("Lobbies").Child(lobby_name);

        __ref.GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;

            FirebaseDataManager.fdm.DeleteData(__ref, ds, null);
        });

        MenuManager.mm.Update_GJTM_LobbyList();
    }

    bool delayedQuit = false;

    public void OnApplicationQuit()
    {
        if (delayedQuit) return;

        Debug.Log("Quitting Application!");
        if (is_in_lobby != "")
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Application.CancelQuit();
#pragma warning restore CS0618 // Type or member is obsolete
            LeaveLobby(is_in_lobby);
            StartCoroutine(DelayedQuit());
        }
    }

    IEnumerator DelayedQuit()
    {
        yield return new WaitForSeconds(.5f);

        delayedQuit = true;
        Application.Quit();
    }

    /// <summary>
    /// Delete all lobbies both server and client side. Will be removed in the future due to possible misuse danger
    /// </summary>
    public void WipeLobbies()
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        DatabaseReference __ref = _ref.Child("Lobbies");

        __ref.GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;
            
            FirebaseDataManager.fdm.DeleteData(__ref, ds, null);
        });
    }

    /// <summary>
    /// Automatically call a function whenever a lobby is added, changed, moved or deleted
    /// </summary>
    /// <param name="callbackFunction"></param>
    public void AutoUpdateLoop(System.Action callbackFunction)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies");

        _ref.ChildAdded += (sender, eventArgs) => 
        {
            callbackFunction();
        };

        _ref.ChildChanged += (sender, eventArgs) =>
        {
            callbackFunction();
        };

        _ref.ChildMoved += (sender, eventArgs) =>
        {
            callbackFunction();
        };
                
        _ref.ChildRemoved += (sender, eventArgs) =>
        {
            //Debug.Log(eventArgs.Snapshot.ToString());
            callbackFunction();
        };

        if (is_in_lobby != "") {
            _ref.Child(is_in_lobby).ChildRemoved += (sender, eventArgs) =>
            {
                Debug.Log("Lobby " + is_in_lobby + " got removed!");
            };
        }
    }

    public int player_count = 0;
    public int rdy_count = 0;
    public bool isRdy = false;

    public void SetReady(bool rdy)
    {
        Debug.Log("("+is_in_lobby+"); Setting ready to " + rdy);

        string tmp = (is_in_lobby.StartsWith("lobby_") ? "" : "lobby_");
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies/"+tmp+is_in_lobby);

        _ref.Child("ready_count").GetValueAsync().ContinueWith((task) =>
        {
            try
            {
                rdy_count = int.Parse(task.Result.Value.ToString());
                Debug.Log(rdy_count + " players are already ready!");
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
                rdy_count = 0;
            }

            Debug.Log(rdy_count);
            
            if(isRdy && !rdy)
            {
                rdy_count--;
                isRdy = false;
            }
            else if(!isRdy && rdy)
            {
                rdy_count++;
                isRdy = true;
            }

            if (rdy_count < 0) rdy_count = 0;

            _ref.Child("ready_count").SetValueAsync(rdy_count);/*.ContinueWith((task2) =>
            {
                if(rdy)
                {
                    _ref.Child("ready_count").ValueChanged += ReadyCountListener;
                }
                else
                {
                    _ref.Child("ready_count").ValueChanged -= ReadyCountListener;
                }
            });*/

            if (isRdy) MenuManager.mm.lobbyButtonReady.GetComponent<Image>().color = Color.red;
            else MenuManager.mm.lobbyButtonReady.GetComponent<Image>().color = Color.white;
        });

        MobileInfosManager.mim.UpdateContent();
    }
    
    void ReadyCountListener(object obj, ValueChangedEventArgs args)
    {
        int temp = rdy_count;
        rdy_count = int.Parse(args.Snapshot.Value.ToString());
        Debug.Log("Received ready count change from " + temp + " to " + rdy_count);
        Debug.Log("Player Count = " + player_count + "; Ready Count = " + rdy_count);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies/lobby_" + is_in_lobby);

        _ref.Child("player_count").GetValueAsync().ContinueWith((task) =>
        {
            player_count = int.Parse(task.Result.Value.ToString());

            Debug.Log("Player Count = " + player_count + "; Ready Count = " + rdy_count);

            if (player_count == rdy_count)
            {
                Debug.Log("Starting Game (not really, but this is where the auto-trigger would be)!");
                //MenuManager.mm.StartGame();
            }
        });

    }

    #region Manage Place Swapping

    public void SetIndex(int index)
    {
        Debug.Log("Current index is " + playerID + ". Setting it to " + index);

        string lobby = is_in_lobby;
        if (lobby.StartsWith("lobby_")) lobby = lobby.Substring("lobby_".Length);
        string path = "Lobbies/lobby_" + lobby + "/players";
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
        _ref.Child("player_" + index).SetValueAsync(uid);

        FirebaseDataManager.fdm.DeleteData(_ref.Child("player_" + playerID));

        playerID = index;
    }

    int switching = 0;
    /// <summary>
    /// Swaps current user at playerID with index and writes this to the firebase data
    /// </summary>
    /// <param name="index"></param>
    public void SwitchWithIndex(int index)
    {
        string path = "Lobbies/lobby_" + is_in_lobby + "/players";

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
        _ref.GetValueAsync().ContinueWith(task =>
        {
            var res = new List<DataSnapshot>(task.Result.Children);

            string[] values = new string[res.Count];

            for(int i = 0; i < res.Count; i++)
            {
                values[i] = res[i].Value + "";
            }

            string temp = values[playerID];
            values[playerID] = values[index];
            values[index] = temp;
            
            for(int i = 0; i < values.Length; i++)
            {
                FirebaseDataManager.fdm.WriteData(path+"/player_"+i, values[i]);
            }

            playerID = index;
        });
    }

    public void SubscribeToIndexChange()
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies/lobby_" + is_in_lobby + "/players");
        _ref.ValueChanged += HandleIndexValueChanged;
    }

    private void HandleIndexValueChanged(object sender, ValueChangedEventArgs e)
    {
        var a = e.Snapshot.Value;

        Debug.Log(a);
    }

    public void UnsubscribeFromIndexChange()
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies/lobby_" + is_in_lobby + "/players");
        _ref.ValueChanged -= HandleIndexValueChanged;
    }


    #endregion

    #region Manage Chat 
    public void UpdateChat(System.Action<List<string>> callbackFunction)
    {
        if(is_in_lobby == "")
        {
            Debug.LogError("Can't update chat while not in lobby!");
            return;
        }

        string path = "Lobbies/" + is_in_lobby + "/chat/messages";
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
        
        _ref.GetValueAsync().ContinueWith((task) =>
        {
            DataSnapshot data = task.Result;

            Debug.Log(data.ToString());
        });
    }


    #endregion

    #region Manage Weather Data 

    public byte weatherData;

    // Range per byte 0000_2 to 1010_2
    // States encoded: Humidity first 2 bits, Temp. 2nd 2 bits (from the left)

    // 0 = Pair 1
    // 1 = Pair 2

    public void SyncWeatherData(byte humidity, byte temperature)
    {
        Debug.Log("Syncing weather data");

        byte localWeatherData = (byte)((humidity << 2) + temperature);
        try
        {
            FirebaseDataManager.fdm.UpdateWeatherData(is_in_lobby, playerID, localWeatherData, FirebaseAuth.DefaultInstance.CurrentUser.UserId, _SyncWeatherData);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

    }

    void _SyncWeatherData(int[] newWeatherData)
    {
        float sum_a = 0;
        float sum_b = 0;
        int j = 0;

        for(int i = 0; i < newWeatherData.Length; i++)
        {
            Debug.Log(newWeatherData[i]);

            if (newWeatherData[i] == -1) continue;

            int[] bits = decimal.GetBits(newWeatherData[i]);

            j++;

            sum_a += bits[0] + (bits[1] << 1);
            sum_b += bits[2] + (bits[3] << 1);
        }

        sum_a /= j;
        sum_b /= j;

        weatherData = (byte)(Mathf.RoundToInt(sum_a) + (Mathf.RoundToInt(sum_b) << 2));

        MobileInfosManager.mim.SetParsedData(weatherData);
    }

    /*
    public void SetInitWeatherData(string lobby_name)
    {
        if (lobby_name.StartsWith("lobby_")) lobby_name = lobby_name.Substring("lobby_".Length);
        FirebaseDataManager.fdm.WriteData("Lobbies/lobby_"+lobby_name)
    }*/

    #endregion
}