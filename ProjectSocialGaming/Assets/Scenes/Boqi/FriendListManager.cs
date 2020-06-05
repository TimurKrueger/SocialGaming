using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using System;

public class FriendListManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static FriendListManager fmm;

    private void Awake()
    {
        if (fmm == null) fmm = this;
        else if (fmm != this) Destroy(this);

        StartCoroutine(a());
    }

    IEnumerator a()
    {
        yield return null;
        LoadFriends();
    }

    public Transform leaderboardContainer;
    public Transform leaderboardEntry;

    struct entryToAdd
    {
        public string name;
        public string uid;
    };

    List<entryToAdd> entriesToAdd = new List<entryToAdd>();

    public void LoadFriends()
    {
        try
        {
            string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            FirebaseDataManager.fdm.ReadProfile(uid, AddFriends);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void Update()
    {
        int i = 0;
        while (entriesToAdd.Count > 0)
        {
            i++;
            Debug.Log(entriesToAdd[0] + "; "  + "; " + i);

            Transform t = Instantiate(leaderboardEntry, leaderboardContainer);
            t.name = "Entry_" + i;

            t.Find("Pos").GetComponent<TextMeshProUGUI>().text = i + "";
            t.Find("FriendName").GetComponent<TextMeshProUGUI>().text = entriesToAdd[0].name;
            //t.Find("Score").GetComponent<TextMeshProUGUI>().text = entriesToAdd.score + "";
            //t.Find("Losses").GetComponent<TextMeshProUGUI>().text = entriesToAdd.lost_against.ToString();

            t.localPosition = new Vector3(0f, 608f - (i - 1) * 64f, 0f);

            entriesToAdd.RemoveAt(0);
            
        }
    }

    void AddFriends(FirebaseDataManager.Profile entries)
    {
        int i = -1;
        foreach (var s in entries.nemeses)
        {
            i++;
            FirebaseDataManager.fdm.ReadProfile(s.uid, AddFriendsEntries);
            
        }
    }

    void AddFriendsEntries(FirebaseDataManager.Profile s)
    {
        //Debug.Log(s.name + "; " + s.score + "; " + s.uid);
        entriesToAdd.Add(new entryToAdd()
        {

            name = s.username,
            uid = s.uid
        });
    }
}