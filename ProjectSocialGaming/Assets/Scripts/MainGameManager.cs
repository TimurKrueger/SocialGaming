using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MainGameManager : MonoBehaviour
{
    [System.Serializable]
    public class Tileset
    {
        public List<TileScriptableObject> tiles = new List<TileScriptableObject>();

        public static int TileListSorter(TileScriptableObject a, TileScriptableObject b)
        {
            return b.difficulty.CompareTo(a.difficulty);
        }
    }

    public Tileset tileset_ice;
    public Tileset tileset_earth;
    public Tileset tileset_fire;

    public List<TileScriptableObject> tiles = new List<TileScriptableObject>();

    public Transform playerPrefab;

    //[HideInInspector]
    //public Hashtable tileTable = new Hashtable();

    [Header("Game Holders")]
    public Transform environmentHolder;
    public Transform tileHolder;

    public Player p;

    public float lowerBorder = -5.0f;   

    public float worldSpeed = 1.0f;

    public Tileset currentTileset = null;

    [Header("Builder Buttons")]
    public RectTransform[] button_tile = new RectTransform[4];
    public RectTransform[] button_tile_preview_holder = new RectTransform[4];

    public static MainGameManager mgm;

    private void Awake()
    {
        if (mgm == null) mgm = this;
        else if (mgm != this) Destroy(this);

        //tileset_ice.tiles.Sort(Tileset.TileListSorter);
        //tileset_earth.tiles.Sort(Tileset.TileListSorter);
        //tileset_fire.tiles.Sort(Tileset.TileListSorter);

        /*
        foreach (var t in tiles)
        {
            if (!tileTable.ContainsKey(t.GetHashCode())) tileTable.Add(t.GetHashCode(), t);
        }*/
    }

    private void Start()
    {
        //InitGame(2, 2, "a");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(p.transform.position.x - 20f, lowerBorder, 0f), new Vector3(p.transform.position.x + 20f, lowerBorder, 0f));
    }

    int playerID;       // Localized for pair; Always 0 or 1!
    int pairID;         // Always 0 or 1!
    string lobby_name;  // String for database access! Doesn't have to contain lobby_ ...
    long score = 0L;

    string path;

    bool gameIsRunning = false;

    float worldSpeedBackup = 1.0f;

    public void InitGame(int pID, int pC, string lobby_name, int tileset)
    {
        currentTileset = (tileset == 0) ? tileset_ice : (tileset == 1) ? tileset_earth : tileset_fire;

        worldSpeedBackup = worldSpeed;
        cleanupStarted = false;
        gameIsRunning = true;

        if (lobby_name.StartsWith("lobby_")) lobby_name.TrimStart("lobby_".ToCharArray());

        this.lobby_name = lobby_name;

        string path = "Games/game_" + lobby_name;

        switch (pC)
        {
            case 2:
                if (pID == 1)
                {
                    path += "/pair_0";
                    playerID = 0;
                    pairID = 0;
                    AddTileListener(path, null);
                    MenuManager.mm.Game_Setup_Runner();
                }
                else if (pID == 2)
                {
                    path += "/pair_0";
                    playerID = 1;
                    pairID = 0;
                    AddPlayerListener(path, null);
                    MenuManager.mm.Game_Setup_Builder();
                }
                else
                {
                    Debug.LogError("Player ID invalid! Must be 1 or 2 for this player count but was " + pID);
                    MenuManager.mm.AbortGameStart();
                    return;
                }
                break;
            case 4:
                switch (pID)
                {
                    case 0:
                        path += "/pair_0";
                        playerID = 0;
                        pairID = 0;
                        AddTileListener(path, null);
                        MenuManager.mm.Game_Setup_Runner();
                        break;
                    case 1:
                        path += "/pair_1";
                        playerID = 1;
                        pairID = 1;
                        AddPlayerListener(path, null);
                        MenuManager.mm.Game_Setup_Builder();
                        break;
                    case 2:
                        path += "/pair_1";
                        playerID = 0;
                        pairID = 1;
                        AddTileListener(path, null);
                        MenuManager.mm.Game_Setup_Runner();
                        break;
                    case 3:
                        path += "/pair_0";
                        playerID = 1;
                        pairID = 0;
                        AddPlayerListener(path, null);
                        MenuManager.mm.Game_Setup_Builder();
                        break;
                    default:
                        Debug.LogError("Player ID invalid! Must be 1,2,3 or 4 for this player count but was " + pID);
                        MenuManager.mm.AbortGameStart();
                        return;
                }
                break;
            default:
                Debug.LogError("Player count is invalid! Must be 2 or 4 but was " + pC);
                MenuManager.mm.AbortGameStart();
                return;
        }

        CreateTile(-1, -9f, 0);
        CreateTile(-1, -6f, 0);
        CreateTile(-1, -3f, 0);
        CreateTile(-1, 0f, 0);
        CreateTile(-1, 3f, 0);
        CreateTile(-1, 6f, 0);
        CreateTile(-1, 9f, 0);

        p = Instantiate(playerPrefab).GetComponent<Player>();
        p.controlledLocally = (playerID == 0) ? true : false;
        p.transform.SetParent(environmentHolder);
        p.transform.localPosition = new Vector3(-2f, 2f, 0f);

        GetComponent<Parallaxing>().Activate(tileset, (playerID == 0) ? false : true);
    }

    private void FixedUpdate()
    {
        if (gameIsRunning)
        {
            foreach (GameObject go in activeTiles)
            {
                go.transform.position += new Vector3(-worldSpeed, 0f, 0f);
            }

            if (playerActionBuffer.Count > 0)
            {
                playerActionBuffer[0].Perform(p);
                playerActionBuffer.RemoveAt(0);
            }

            if (blockBuffer.Count > 0)
            {
                RetrieveTile(blockBuffer[0]);
                blockBuffer.RemoveAt(0);
            }

            if (blockBufferMain.Count > 0)
            {
                blockPref bp = blockBufferMain[0];
                CreateTile(bp.id, bp.pos_x, bp.tile_id);
                blockBufferMain.RemoveAt(0);
            }

            if (p.transform.position.y < lowerBorder && playerID == 0)
            {
                KillRunner();
            }
        }
    }

    #region Tile Management
    public void AddTileListener(string path, System.Action<int[]> callback)
    {
        this.path = path + "/level_data/blocks";

        Debug.Log("Adding local Listener to path " + this.path + "!");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(this.path);

        /*
        _ref.ChildRemoved += (sender, eventArgs) =>
        {
            Debug.Log("Child got removed!");
        };*/
        Debug.Log(_ref.Parent);
        try
        {
            _ref.ChildAdded += HandleTileAdded;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        /*
        try
        {
            _ref.ChildChanged += HandleTileChanged;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        try
        {
            _ref.ChildRemoved += HandleTileDeleted;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }*/

        Debug.Log("Added handle for updates");
    }

    public List<int> blockBuffer = new List<int>();
    public List<blockPref> blockBufferMain = new List<blockPref>();

    [System.Serializable]
    public struct blockPref
    {
        public int id;
        public float pos_x;
        public int tile_id;
    }


    private void HandleTileAdded(object sender, ChildChangedEventArgs args)
    {
        var cs = args.Snapshot.Children;

        List<DataSnapshot> dss = new List<DataSnapshot>();

        Debug.Log(args.Snapshot.Key);
        Debug.Log(args.Snapshot.Reference.Parent);

        int id = 0;

        foreach (DataSnapshot ds in cs)
        {
            Debug.Log("+" + ds.Key);
            Debug.Log("+" + ds.Value);

            if (ds.Key == "id")
            {
                id = int.Parse(ds.Value + "");
            }
        }
        
        Debug.Log(id);

        blockBuffer.Add(id);

        if (args.DatabaseError != null)
        {
            Debug.LogError("Database Error: " + args.DatabaseError.ToString());
            return;
        }

        try
        {
            //float[] temp = SnapshotToTile(args.Snapshot);
            //CreateTile((int)temp[0], temp[1], (int)temp[2]);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void RetrieveTile(int id)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Games/game_" + lobby_name + "/pair_" + pairID + "/level_data/blocks/block_" + id);

        _ref.GetValueAsync().ContinueWith(task =>
        {
            Debug.Log(task.Result.ToString());

            int _id = int.Parse(task.Result.Child("id").Value+"");
            float _pos_x = float.Parse(task.Result.Child("pos_x").Value+"");
            int _tile_id = int.Parse(task.Result.Child("tile_id").Value+"");

            blockBufferMain.Add(new blockPref()
            {
                id = _id,
                pos_x = _pos_x,
                tile_id = _tile_id
            });

        });
    }

    private void HandleTileChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database Error: " + args.DatabaseError.ToString());
            return;
        }

        try
        {
            float[] temp = SnapshotToTile(args.Snapshot);
            MoveTile((int)temp[0], temp[1]);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void HandleTileDeleted(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database Error: " + args.DatabaseError.ToString());
            return;
        }

        try
        {
            float[] temp = SnapshotToTile(args.Snapshot);
            DeleteTile((int)temp[0]);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public float[] SnapshotToTile(DataSnapshot ds)
    {
        Debug.Log("Parsing Tile Snapshot data (" + ds.ToString() + "; " + ds.Value.ToString() + ")");

        Dictionary<string, DataSnapshot> di = ds.Value as Dictionary<string, DataSnapshot>;

        Debug.Log(di.Count);

        foreach (var a in di)
        {
            Debug.Log(a.Key + "; " + a.Value);
        }

        string str_id = ds.Child("id").Value.ToString();
        string str_pos_x = ds.Child("pos_x").Value.ToString();
        string str_tile_x = ds.Child("tile_id").Value.ToString();

        int id = int.Parse(str_id);
        float pos_x = float.Parse(str_pos_x);
        int tile_id = int.Parse(str_tile_x);

        return new float[] { id, pos_x, tile_id };
    }


    int[] actualButtonIndices = new int[4];

    public void RerollTiles()
    {
        Debug.Log("Rerolling Tiles!");

        int c = currentTileset.tiles.Count;
        
        for(int i = 0; i < actualButtonIndices.Length; i++)
        {
            int v = UnityEngine.Random.Range(0, c);
            actualButtonIndices[i] = v;

            TileScriptableObject tile = currentTileset.tiles[v];

            Destroy(button_tile_preview_holder[i].gameObject);

            button_tile_preview_holder[i] = new GameObject("ContentHolder").AddComponent<RectTransform>();

            button_tile_preview_holder[i].SetParent(button_tile[i]);

            List<Transform> toInstantiate = GetAllChildren(tile.prefab);

            foreach(Transform t in toInstantiate)
            {
                if (t.GetComponent<SpriteRenderer>() == null || t.GetComponent<SpriteRenderer>().sprite == null) continue;
                
                RectTransform content = new GameObject("Content_" + t.name).AddComponent<RectTransform>();

                content.SetParent(button_tile_preview_holder[i].transform);
                content.gameObject.AddComponent<SpriteRenderer>().sprite = t.GetComponent<SpriteRenderer>().sprite;

                content.position = new Vector3(button_tile_preview_holder[i].position.x + 17.5f, button_tile_preview_holder[i].position.y + 7.5f - (4.77f * i), button_tile_preview_holder[i].position.z);
                content.localScale = t.lossyScale * 2f;
            }            
        }

        Debug.Log("Finished Rerolling!");
    }

    List<Transform> GetAllChildren(Transform t)
    {
        if (t.childCount == 0) return new List<Transform>() { t };

        List<Transform> list = new List<Transform>();

        for(int i = 0; i < t.childCount; i++)
        {
            list.AddRange(GetAllChildren(t.GetChild(i)));
        }

        list.Add(t);

        return list;
    }

    public List<GameObject> activeTiles = new List<GameObject>();

    public void CreateNextTile(int type)
    {
        float x = -100f;
        int next_id = 0;

        foreach (GameObject go in activeTiles)
        {
            if (go.transform.position.x > x)
            {
                x = go.transform.position.x;
            }
            int id = int.Parse(go.name);
            if (id > next_id) next_id = id;
        }

        CreateTile(next_id + 1, x + 2f, type);

        RerollTiles();
    }

    public void CreateTile(int id, float pos_x, int tile_id)
    {
        if (!gameIsRunning) return;

        Debug.Log("Creating tile " + tile_id + " at " + pos_x + " with id " + id);

        TileScriptableObject tile = currentTileset.tiles[tile_id]; //(TileScriptableObject)tiles[tile_id];

        GameObject go = Instantiate(tile.prefab).gameObject;

        go.transform.parent = tileHolder;
        go.transform.position = new Vector3(pos_x, 0f, 0f);
        go.name = id + "";

        activeTiles.Add(go);

        string path = "Games/game_" + lobby_name + "/pair_" + pairID + "/level_data/blocks/block_" + id;

        if (playerID == 1)
        {
            Debug.Log("Uploading tile data to " + path);

            FirebaseDataManager.fdm.WriteData(path + "/id", id + "");
            FirebaseDataManager.fdm.WriteData(path + "/pos_x", pos_x + "");
            FirebaseDataManager.fdm.WriteData(path + "/tile_id", tile_id + "");
        }
    }

    public void MoveTile(int id, float new_x)
    {
        Debug.Log("Moving tile with id " + id + " to pos " + new_x);

        foreach (GameObject go in activeTiles)
        {
            if (go.name == "" + id)
            {
                go.transform.position = new Vector3(new_x, 0f, 0f);
                return;
            }
        }
    }

    public void DeleteTile(int id)
    {
        Debug.Log("Deleting tile " + id);

        if (id >= 0)
        {
            GameObject g = null;
            foreach (GameObject go in activeTiles)
            {
                if (go.name == "" + id)
                {
                    g = go;
                    break;
                }
            }
            if (g == null) return;

            Destroy(g);
        }
        else
        {
            while (activeTiles.Count > 0)
            {
                GameObject go = activeTiles[0];
                Destroy(go);
                activeTiles.RemoveAt(0);
            }
        }
    }
    #endregion

    #region Player Management
    public void QueueAction(int id)
    {
        string path = "Games/game_" + lobby_name + "/pair_" + pairID + "/player/Action_" + (UnityEngine.Random.Range(0, 100000000));
        string data = id + "";

        FirebaseDataManager.fdm.WriteData(path, data);
    }

    private void AddPlayerListener(string path, System.Action<int[]> callback)
    {
        path += "/player";
        this.path = path;

        RerollTiles();

        Debug.Log("Adding local Listener to path " + path + "!");

        DatabaseReference _ref_base = FirebaseDatabase.DefaultInstance.RootReference.Child(path);

        DatabaseReference _ref_player = _ref_base;

        // No add Listener required

        // Change Listener handles input transfer
        try
        {
            _ref_player.ChildAdded += HandlePlayerAction;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        // Delete Listener handles player death
        try
        {
            //_ref_player.ChildRemoved += HandleTileDeleted;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        Debug.Log("Added handle for updates");

        AddPlayerDeathListener(path);
    }

    List<PlayerAction> playerActionBuffer = new List<PlayerAction>();

    private void HandlePlayerAction(object sender, ChildChangedEventArgs args)
    {
        //Debug.Log("Test");
        if (args.DatabaseError != null)
        {
            Debug.LogError("Database Error: " + args.DatabaseError.ToString());
            return;
        }

        try
        {
            int id = int.Parse(args.Snapshot.Value.ToString());
            playerActionBuffer.Add(new PlayerAction(id, args.Snapshot.Reference));
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public class PlayerAction
    {
        DatabaseReference dref;
        int id = -1;

        public PlayerAction(int id, DatabaseReference dref)
        {
            this.id = id;
            this.dref = dref;
        }

        public void Perform(Player p)
        {
            if (id == 0)
                p.Jump();
            else if (id == 1)
                p.Move(false);
            else if (id == 2)
                p.Move(true);

            // FirebaseDataManager.fdm.DeleteData(dref);
        }
    }
    
    public void AddPlayerDeathListener(string path)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Games/game_" + lobby_name + "/pair_" + pairID + "/result");
        _ref.ValueChanged += HandlePlayerDeathValueChanged;
    }

    private void HandlePlayerDeathValueChanged(object sender, ValueChangedEventArgs e)
    {
        Debug.Log("Player Death Value Changed to " + e.Snapshot.Value);
        if (e.Snapshot.Value+"" == "") return;

        try
        {
            long v = long.Parse(e.Snapshot.Value+"");

            if (v != -1L) EndGame();
        }
        catch (System.Exception _e)
        {
            Debug.LogError(e.Snapshot.Value+": "+_e);
        }
    }

    #endregion

    #region End Game and Cleanup Methods
    bool cleanupStarted = false;

    public void KillRunner()
    {
        if (cleanupStarted) return;

        Debug.Log("Killing Runner!");
        long score = p.score;
        FirebaseDataManager.fdm.WriteData("Games/game_" + lobby_name + "/pair_" + pairID + "/result", score+"");

        EndGame();
    }

    public void EndGame()
    {
        if (cleanupStarted) return;

        Debug.Log("Cleaning up game!");
        cleanupStarted = true;
        gameIsRunning = false;

        /*
        while (tileHolder.childCount > 0)
        {
            Destroy(tileHolder.GetChild(0).gameObject);
        }*/

        
        if (playerID == 0)
        {
            RemoveTileListener();
            //Destroy(p.gameObject);
        }
        else
        {
            RemovePlayerListener();
            /*
            while (tileHolder.childCount > 0)
            {
                Destroy(tileHolder.GetChild(0).gameObject);
            }*/

        }
        Debug.Log("Cleaned up game!");

        GetComponent<Parallaxing>().Deactivate();
        /*
        if(FirebaseLobbyManager.flm.is_Hosting)
        {
            StartCoroutine(WipeData(1.0f));
        }*/

        MenuManager.mm.EndGame();
        ResultMenuManager.rmm.CalculateRank(score);
    }

    IEnumerator WipeData(float delay)
    {
        if (delay > 0.0f)
            yield return new WaitForSecondsRealtime(delay);

        FirebaseDataManager.fdm.DeleteData(FirebaseDatabase.DefaultInstance.RootReference.Child("Games/game_"+lobby_name+"/pair_"+pairID));
    }

    public void ClearRemainingGameObjects()
    {
        Debug.Log("Here is where tiles and player will get wiped!");
        
        try
        {
            Destroy(tileHolder.gameObject);

            tileHolder = new GameObject("Tiles").transform;
            tileHolder.parent = environmentHolder;

            activeTiles.Clear();
            worldSpeed = worldSpeedBackup;

            Destroy(p.gameObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Games/game_" + lobby_name);

        if(_ref != null) FirebaseDataManager.fdm.DeleteData(_ref);
    }

    public void RemoveTileListener()
    {
        Debug.Log("Removing local Listener from path " + this.path + "!");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child(this.path);
        
        try
        {
            _ref.ChildAdded -= HandleTileAdded;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        Debug.Log("Removed handle for updates");
        /*
        while (tileHolder.childCount > 0)
        {
            Destroy(tileHolder.GetChild(0).gameObject);
        }*/

        //Debug.Log("Cleaned up game!");
    }

    public void RemovePlayerListener()
    {
        Debug.Log("Removing local Listener from path " + path + "!");

        DatabaseReference _ref_base = FirebaseDatabase.DefaultInstance.RootReference.Child(path);

        DatabaseReference _ref_player = _ref_base;

        // No add Listener required

        // Change Listener handles input transfer
        try
        {
            _ref_player.ChildAdded -= HandlePlayerAction;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Games/game_" + lobby_name + "/pair_" + pairID + "/result");
        _ref.ValueChanged -= HandlePlayerDeathValueChanged;

        Debug.Log("Removed handle for updates");


        //Destroy(p.gameObject);
    }
    #endregion

}