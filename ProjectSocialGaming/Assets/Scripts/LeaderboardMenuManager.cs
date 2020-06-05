using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardMenuManager : MonoBehaviour
{
    public static LeaderboardMenuManager lmm;

    private void Awake()
    {
        if (lmm == null) lmm = this;
        else if (lmm != this) Destroy(this);

        //StartCoroutine(a());
    }
    
    IEnumerator a()
    {
        yield return null;
        LoadLeaderboard();
    }

    public Transform leaderboardContainer;
    public Transform leaderboardEntry;

    List<Transform> currentEntries = new List<Transform>();
    
    struct entryToAdd
    {
        public string name;
        public string uid;
        public long score;
    };

    List<entryToAdd> entriesToAdd = new List<entryToAdd>();

    public void LoadLeaderboard()
    {
        FirebaseDataManager.fdm.ReadLeaderboard(AddLeaderboardEntries);

        while(currentEntries.Count > 0)
        {
            var gO = currentEntries[0].gameObject;
            currentEntries.RemoveAt(0);

            Destroy(gO);
        }
    }

    private void Update()
    {
        int i = 0;
        while(entriesToAdd.Count > 0)
        {
            i++;
            Debug.Log(entriesToAdd[0].name+"; "+entriesToAdd[0].score+"; "+i);

            Transform t = Instantiate(leaderboardEntry, leaderboardContainer);
            t.name = "Entry_" + i;
            
            t.Find("Pos").GetComponent<TextMeshProUGUI>().text = i+"";
            t.Find("Name").GetComponent<TextMeshProUGUI>().text = entriesToAdd[0].name;
            t.Find("Score").GetComponent<TextMeshProUGUI>().text = entriesToAdd[0].score+"";
            t.Find("Wins").GetComponent<TextMeshProUGUI>().text = "---";

            t.localPosition = new Vector3(0f, 608f - (i-1) * 64f, 0f);

            var vh = t.gameObject.AddComponent<ValueHolder>();
            vh.value = entriesToAdd[0].uid;

#pragma warning disable CS0618 // Type or member is obsolete
            t.GetComponent<Button>().onClick.AddListener(() =>
#pragma warning restore CS0618 // Type or member is obsolete
            {
                MenuManager.mm.Open_LeaderboardProfile(vh.value);
            });

            entriesToAdd.RemoveAt(0);
            currentEntries.Add(t);
        }
    }

    void AddLeaderboardEntries(List<FirebaseDataManager.LeaderboardEntry> entries) { 
        foreach(var s in entries)
        {
            Debug.Log(s.name+"; "+s.score+"; "+s.uid);
            entriesToAdd.Add(new entryToAdd()
            {
                name = s.name,
                score = s.score,
                uid = s.uid
            });
        }

        entriesToAdd.Sort(CompareEntries);
    }

    private static int CompareEntries(entryToAdd a, entryToAdd b)
    {
        return a.score.CompareTo(b.score) * -1;
    }
}