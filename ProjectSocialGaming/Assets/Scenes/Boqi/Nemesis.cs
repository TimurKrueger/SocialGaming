using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Firebase.Auth;

public class Nemesis : MonoBehaviour
{

    public Transform nemesisContainer;
    public Transform nemesisEntry;
    /*public TextMeshProUGUI pos1;
    public TextMeshProUGUI name1;
    public TextMeshProUGUI win1;
    public TextMeshProUGUI score1;
    public FirebaseDataManager.Profile profile;
    public TextMeshProUGUI losses1;*/
    // Start is called before the first frame update
    /*List<FirebaseDataManager.Profile> nemisis = new List<FirebaseDataManager.Profile>();
    private void open()
    {
        //leaderboardContainer = transform.Find("HighscoreContainer");
        //leaderboardEntry = transform.Find("Panel");

        nemesisEntry.gameObject.SetActive(false);
        
       // for (int i = 0; i < 10; i++)
        //{

            //string uid = FirebaseDataManager.fdm.re("/Leaderboard/user_/");
            /*string uid = FirebaseDataManager.getUid(i);
            index++;
            profile = FirebaseDataManager.ReadProfile(uid);
            profiles.Add(profile);*
            string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            FirebaseDataManager.fdm.ReadProfile(uid, _open);
        //}

    }
    /*public void LoadLeaderboarddata()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, leaderboardData);
        if(File.Exists (filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
        }
    }*
    int index = 0;
    /*struct entryToAdd
    {
        public string name;
        public string score;
        public int pos;
    };
    List<entryToAdd> entrys = new List<entryToAdd>();*
    public void _open(FirebaseDataManager.Profile a) 
    {

        profile = a;
        for(int i  = 0; i < profile.nemeses.Count; i++)
        {
            FirebaseDataManager.fdm.ReadProfile(profile.nemeses[i].uid, __open);
        }
        /*if (data.Count == 0)
        {
            return;
        }
        foreach (string p in data)
        {
            print(p);
        }
        /*entrys.Add(new entryToAdd() { name = data[0], score = data[1], pos = index });
        print("ENTRYS: COUNT" + entrys.Count);*
    }

    public void __open(FirebaseDataManager.Profile a)
    {
        nemisis.Add(profile);
    }
    void Start()
    {
        open();
        Update1();
    }

    // Update is called once per frame
    public void Update1()
    {
        index = 0;
        print("NOOOOOOOOOOOO");
        foreach (FirebaseDataManager.Profile p in nemisis)
        {
            print("WHHHHHHHHHHHHHHH");
            float templateHeight = 50f;
            Transform entryTransform = Instantiate(nemesisEntry, nemesisContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * index);
            entryTransform.gameObject.SetActive(true);
            


            int rank = index + 1;
            index++;
            string rankString;
            switch (rank)
            {
                default:
                    rankString = rank + "TH"; break;

                case 1: rankString = "1ST"; break;
                case 2: rankString = "2nd"; break;
                case 3: rankString = "3rd"; break;
            }

            pos1.text = rankString;

            string name = p.username;
            name1.text = p.username;
            int wins = p.games_won;
            win1.text = wins.ToString();
            int losess = p.games_lost;
            losses1.text = losess.ToString();
            int score = Random.Range(0, 10000);
            score1.text = score.ToString();
            
        }

    }*/
}
/*
List<entryToAdd> entriesToAdd = new List<entryToAdd>();
void AddLeaderboardEntries(List<FirebaseDataManager.LeaderboardEntry> entries)
{
    int i = 0;
    foreach (var s in entries)
    {
        FirebaseDataManager.fdm.ReadProfile(entries.nemesis.uid, AddNemesesEntries);
        lost_against = s.lost_against,
        won_against = s.won_against,
        i++;
    }

    entriesToAdd.Sort(CompareEntries);
}

void AddNemesesEntries(FirebaseDataManager.Profile s)
{
    Debug.Log(s.name + "; " + s.score + "; " + s.uid);
    entriesToAdd.Add(new entryToAdd()
    {
        name = s.name,
        score = s.score,
        uid = s.uid
    });
}

void sort_(List<entryToAdd> n) {
    for(int i = 0; i < n.size(); i++)
    {
        for(int j = 0; j < n.size(); j++)
        {
            if(n[j].score > n[i].score)
            {
                entryToAdd temp = n[j];
                n[j] = n[i];
                n[i] = temp;
            }
        }
    }
}*/
