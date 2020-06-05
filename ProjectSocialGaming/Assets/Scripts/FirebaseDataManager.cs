using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Prime class for Database access
/// </summary>
public class FirebaseDataManager : MonoBehaviour
{
    #region Singleton Structure
    public static FirebaseDataManager fdm;

    private void Awake()
    {
        if (fdm == null) fdm = this;
        else if (fdm != this) Destroy(this);
    }
    #endregion

    /// <summary>
    /// Init the firebase database access. Also from the main tutorial.
    /// </summary>
    void Start()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://projectsocialgaming.firebaseio.com/");
        FirebaseApp.DefaultInstance.SetEditorP12FileName("projectsocialgaming-a3dbdaf34751.p12");
        FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("main-user-105@projectsocialgaming.iam.gserviceaccount.com");
        FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
    }

    /// <summary>
    /// Deprecated test method. Will be removed soon.
    /// </summary>
    public void SetTestData()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("Loging in Anon for test!");
            FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                WriteData("nemesis/user/score", "100");

                Debug.Log("Signing out Anon");
                FirebaseAuth.DefaultInstance.SignOut();
            });

            return;
        }
        else
            WriteData("nemesis/user/name", "Hi");

        return;

        //Debug.Log("Sending test data to db");

        //DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        //string test = "Hi. This is a test!";

        //_ref.Child("test").SetValueAsync(test);

        //Debug.Log("Wrote test string to databank");
    }

    /// <summary>
    /// Another deprecated test method. Will also be removed soon.
    /// </summary>
    public void GetTestData()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.Log("Loging in Anon for test!");
            FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                ReadData("nemesis/user/score", Debug.Log);

                Debug.Log("Signing out Anon");
                FirebaseAuth.DefaultInstance.SignOut();
            });

            return;
        }
        else
            ReadData("nemesis/user/score", Debug.Log);

        return;

    }

    /// <summary>
    /// Write data to a certain path. Use this carefully!
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    public void WriteData(string path, string data)
    {
        //Debug.Log("Writing '" + data + "' to '" + path + "'");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).SetValueAsync(data);

        //Debug.Log("Write call completed!");
    }

    /// <summary>
    /// Read data from a certain path and send it to a callback function. The Menu Manager contains a 5 steps tutorial how the method structure for these works.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callbackFunction"></param>
    public void ReadData(string path, System.Action<string> callbackFunction)
    {
        Debug.Log("Reading data from '" + path + "'");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;
            Debug.Log("Read call completed!");
            callbackFunction(ds.ToString());
        });
    }

    /// <summary>
    /// Read data from a certain path and send it to a callback function. The Menu Manager contains a 5 steps tutorial how the method structure for these works.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callbackFunction"></param>
    public void ReadDataList(string path, System.Action<List<string>> callbackFunction)
    {
        Debug.Log("Reading data from '" + path + "'");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;
            Debug.Log("Read call completed!");

            List<string> ls = new List<string>();

            foreach (DataSnapshot c in ds.Children)
            {
                //Debug.Log(c.Value.ToString());
                ls.Add(c.Value.ToString());
            }

            callbackFunction(ls);
        });
    }

    public void UpdateWeatherData(string lobby_name, int playerID, byte newWeatherData, string uid, System.Action<int[]> callback)
    {
        if (lobby_name.StartsWith("lobby_")) lobby_name = lobby_name.Substring("lobby_".Length);

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Lobbies/lobby_" + lobby_name + "/weather_data");

        _ref.Child(uid).SetValueAsync((int)newWeatherData).ContinueWith(_task => {
            _ref.GetValueAsync().ContinueWith(task =>
            {
                Dictionary<string, object> v = task.Result.Value as Dictionary<string, object>;

                int[] weatherData = new int[] { -1, -1, -1, -1 };
                int i = 0;

                Debug.LogWarning("AAAAAAAAAAAAAAAAAAAAAAAAAAA");

                foreach (var k in v.Keys)
                {
                    object value;

                    if (k == uid) {
                        weatherData[i] = newWeatherData;
                    }
                    else if (v.TryGetValue(k, out value))
                    {
                        weatherData[i] = (int)value;
                        i++;
                    }
                }

                callback(weatherData);

            });
        });
    }

    public struct LeaderboardEntry
    {
        public string name;
        public string uid;
        public long score;
    }

    public void ReadLeaderboard(System.Action<List<LeaderboardEntry>> callbackFunction)
    {
        string path = "Leaderboard";

        Debug.Log("Reading data from '" + path + "'");

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        _ref.Child(path).GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;
            Debug.Log("Read call completed!");

            List<LeaderboardEntry> ls = new List<LeaderboardEntry>();

            foreach (DataSnapshot c in ds.Children)
            {
                string n = c.Child("name").Value+"";
                string u = c.Child("uid").Value + "";
                long s = long.Parse(c.Child("score").Value + "");
                ls.Add(new LeaderboardEntry()
                {
                    name = n,
                    uid = u,
                    score = s
                });
            }

            callbackFunction(ls);
        });

    }

    public struct Profile
    {
        public string uid;
        public string username;
        public int games_won;
        public int games_lost;
        public List<string> friends;
        public List<NemesisEntry> nemeses;

        public struct NemesisEntry
        {
            public string uid;
            public int won_against;
            public int lost_against;

            public override string ToString()
            {
                return uid + " (W=" + won_against + "; L=" + lost_against + ")";
            }
        }

        public override string ToString()
        {
            string friendList = "[";
            for (int i = 0; i < friends.Count; i++)
            {
                friendList += friends[i];
                if(i < friends.Count - 1)
                {
                    friendList += ", ";
                }
            }
            friendList += "]";

            string nemesisList = "[";
            for (int i = 0; i < nemeses.Count; i++)
            {
                nemesisList += nemeses[i].ToString();
                if (i < nemeses.Count - 1)
                {
                    nemesisList += ", ";
                }
            }
            nemesisList += "]";

            return
                "User: " + uid + "\n" +
                "Name: " + username + "\n" +
                "Won: " + games_won + "\n" +
                "Lost: " + games_lost + "\n" +
                "Friends: " + friendList + "\n" +
                "Nemeses: " + nemesisList;
        }
    }

    public void ReadProfile(string uid, System.Action<Profile> callback)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Profiles/"+uid);

        if (_ref == null)
        {
            Debug.LogError("Warning! Player doesn't exist!");
            return;
        }

        _ref.GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;

            string username = ds.Child("username").Value + "";
            int games_won = int.Parse(ds.Child("games_won").Value + "");
            int games_lost = int.Parse(ds.Child("games_lost").Value + "");
            List<string> friends = new List<string>();

            if (ds.HasChild("friends"))
            {
                foreach (var _ds in ds.Child("friends").Children)
                {
                    friends.Add(_ds.Key + "");
                }
            }

            List<Profile.NemesisEntry> nemeses = new List<Profile.NemesisEntry>();

            if (ds.HasChild("nemeses"))
            {
                foreach (var _ds in ds.Child("nemeses").Children)
                {
                    string _uid = _ds.Key;
                    int _won_against = int.Parse(_ds.Child("won_against").Value + "");
                    int _lost_against = int.Parse(_ds.Child("lost_against").Value + "");

                    nemeses.Add(new Profile.NemesisEntry()
                    {
                        uid = _uid,
                        won_against = _won_against,
                        lost_against = _lost_against
                    });
                }
            }

            Profile p = new Profile()
            {
                uid = uid,
                username = username,
                games_won = games_won,
                games_lost = games_lost,
                friends = friends,
                nemeses = nemeses
            };
            Debug.Log(p.ToString());

            callback(p);
        });

    }

    /*public static string getUid(int usernumber)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("/Leaderboard/user_/"+usernumber);

        if (_ref == null)
        {
            Debug.LogError("Warning! Player doesn't exist!");
            return "";
        }

        _ref.GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;

            string uid = ds.Child("uid").Value + "";
            
        });
        return "";
    }*/

    /// <summary>
    /// Only use this when you know exactly what you're doing. All parameters are retrieved directly from the databank acccess which is why this method will nearly always be used as a callback ONLY.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="node_data"></param>
    /// <param name="children"></param>
    public void DeleteData(DatabaseReference node, DataSnapshot node_data, List<DataSnapshot> children)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference;

        if (children == null)
        {
            children = new List<DataSnapshot>(node_data.Children);
        }

        foreach (DataSnapshot ds in children)
        {
            if (node_data.HasChild(ds.Key))
            {
                node.Child(ds.Key).RemoveValueAsync();
            }
        }
    }

    public void DeleteData(DatabaseReference node)
    {
        node.RemoveValueAsync();
    }

    public void AddFriend(string u_uid, string v_uid)
    {
        Debug.Log("Setting " + u_uid + " and " + v_uid + " as friends");
        long time = System.DateTime.UtcNow.Ticks;
        WriteData("Profiles/" + u_uid + "/friends/" + v_uid, time+"");
        WriteData("Profiles/" + v_uid + "/friends/" + u_uid, time+"");
    }

    public void RemoveFriend(string u_uid, string v_uid)
    {
        Debug.Log("Removing " + u_uid + " and " + v_uid + " as friends");
        DeleteData(FirebaseDatabase.DefaultInstance.RootReference.Child("Profiles/" + u_uid + "/friends/" + v_uid));
        DeleteData(FirebaseDatabase.DefaultInstance.RootReference.Child("Profiles/" + v_uid + "/friends/" + u_uid));
    }

    #region Nemesis Score
    public static void ComputeNemesisScore(Profile u, System.Action<Profile, List<Tuple<int, Profile.NemesisEntry>>> callback)
    {
        List<Tuple<int, Profile.NemesisEntry>> nemeses = new List<Tuple<int, Profile.NemesisEntry>>();

        //ComputeSocialGraph(u, callback, ComputeNemesisScore);

        callback(u, nemeses);
    }

    IEnumerator ComputeSocialGraph(Profile u, System.Action<Profile, List<Tuple<int, Profile.NemesisEntry>>> callback)
    {
        string path = "Profiles/" + u.uid + "/friends";

        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Parent.Child(path);

        //int = dist to u
        //string = uid of user
        List<Tuple<int, string>> users = new List<Tuple<int, string>>();
        bool done = false;

        _ref.GetValueAsync().ContinueWith(task =>
        {
            var res = task.Result;
            Debug.Log(res);
        });

        yield return new WaitUntil(() => done = true);

        List<Tuple<int, Profile.NemesisEntry>> nemeses = new List<Tuple<int, Profile.NemesisEntry>>();

        callback(u, nemeses);
    }

    public static int ComputeNemesisScore(Profile u, Profile.NemesisEntry v)
    {
        return ComputeNemesisScore_Main(u, v, 1);
    }

    public static void ComputeSocialGraph(Profile u, System.Action<Profile, SocialGraph> callback)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Profiles");
        
        _ref.GetValueAsync().ContinueWith(task =>
        {
            var res = task.Result;

            Debug.Log(res);
            Dictionary<string, object> dict = res.Value as Dictionary<string, object>;

            List<string> nodes = new List<string>();
            List<Tuple<int, int>> edges = new List<Tuple<int, int>>();
            
            foreach(var entry in dict)
            {
                nodes.Add(entry.Key);
            }

            foreach(var entry in dict)
            {
                Dictionary<string, object> profile = entry.Value as Dictionary<string, object>;

                if (!profile.ContainsKey("friends")) continue;

                Dictionary<string, object> friends = profile["friends"] as Dictionary<string, object>;

                int src = nodes.IndexOf(entry.Key);

                foreach (var _entry in friends)
                {
                    int dest = nodes.IndexOf(_entry.Key);

                    edges.Add(new Tuple<int, int>(src, dest));
                }
            }

            SocialGraph g = new SocialGraph
            {
                nodes = nodes,
                edges = edges
            };

            Debug.Log("Social Graph created!");
            callback(u, g);
        });

    }

    /// <summary>
    /// Performs actual calculation of the nemesis Score of v (for u; given distance d_uv)
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="d_uv"></param>
    /// <returns></returns>
    public static int ComputeNemesisScore_Main(Profile u, Profile.NemesisEntry v, float d_uv)
    {
        float u_played = u.games_won + u.games_lost;
        float v_played = v.won_against + v.lost_against;
        float u_winrate = (float)u.games_won / u_played;
        float v_winrate = (float)v.won_against / v_played;

        float r_uv = v_played / u_played;

        return Mathf.RoundToInt(d_uv * ((1 - r_uv) * (1 - v_winrate) + r_uv * u_winrate) * 1000f);
    }

    [System.Serializable]
    public class SocialGraph
    {
        public List<string> nodes = new List<string>();
        public List<Tuple<int, int>> edges = new List<Tuple<int, int>>();
        
        public int GetDistanceBetween(string a, string b, int maxDist)
        {
            return GetDistanceBetween(nodes.IndexOf(a), nodes.IndexOf(b), maxDist);
        }

        public int GetDistanceBetween(int a, int b, int maxDist)
        {
            // TODO
            for(int i = 0; i < edges.Count; i++)
            {
                if (edges[i].Item1.Equals(a))
                {
                    if (edges[i].Item2.Equals(b))
                    {
                        return 1;
                    }
                    else
                    {
                        if (maxDist > 1)
                        {
                            for (int j = 0; j < edges.Count; j++)
                            {
                                if (edges[j].Item1.Equals(edges[i].Item2))
                                {
                                    if (edges[j].Item2.Equals(b))
                                    {
                                        return 2;
                                    }
                                    else
                                    {
                                        if (maxDist > 2)
                                        {
                                            for (int o = 0; o < edges.Count; o++)
                                            {
                                                if (edges[o].Item1.Equals(edges[j].Item2))
                                                {
                                                    if (edges[o].Item2.Equals(b))
                                                    {
                                                        return 3;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public override string ToString()
        {
            string str = "Social Graph:\nNodes:\n\t[";

            for(int i = 0; i < nodes.Count; i++)
            {
                str += nodes[i]+" ("+i+")";
                if (i < nodes.Count - 1) str += "; ";
            }
            str += "]\nEdges:\n\t[";
            for(int i = 0; i < edges.Count; i++)
            {
                int a = edges[i].Item1;
                int b = edges[i].Item2;
                str += nodes[a] + " (" + a + ") -> " + nodes[b] + " (" + b + ")";
                if (i < edges.Count - 1) str += "; ";
            }
            str += "]\n";
            return str;
        }
    }

    #endregion
}