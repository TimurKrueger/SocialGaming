using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using System;
using UnityEngine.UI;

public class FriendlistMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static FriendlistMenuManager fmm;

    private void Awake()
    {
        if (fmm == null) fmm = this;
        else if (fmm != this) Destroy(this);

        //StartCoroutine(a());
    }

    IEnumerator a()
    {
        yield return null;
        LoadFriends();
    }

    public Transform friendsContainer;
    public Transform friendsEntry;

    struct entryToAdd
    {
        public string name;
        public string uid;
    };

    List<entryToAdd> entriesToAdd = new List<entryToAdd>();
    List<Transform> entries = new List<Transform>();

    public void LoadFriends()
    {
        wipeFriends();
        try
        {
            Debug.Log("Friends add begins.");
            string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            FirebaseDataManager.fdm.ReadProfile(uid, AddFriends);

            //AddFriendsEntries(p);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void wipeFriends()
    {
        i = 0;
        while (entries.Count > 0)
        {
            var t = entries[0];
            Destroy(t.gameObject);
            entries.RemoveAt(0);
        }
    }

    int i = 0;

    private void Update()
    {
        while (entriesToAdd.Count > 0)
        {
            i++;
            //Debug.Log(entriesToAdd[0] + entriesToAdd[0].name);

            Transform t = Instantiate(friendsEntry, friendsContainer);
            t.name = "Entry_" + i;
            
            t.Find("FriendsNameText").GetComponent<TextMeshProUGUI>().text = entriesToAdd[0].name;

            var vh = t.gameObject.AddComponent<ValueHolder>();
            vh.value = entriesToAdd[0].uid;

#pragma warning disable CS0618 // Type or member is obsolete
            t.FindChild("ViewProfileButton").GetComponent<Button>().onClick.AddListener(() =>
#pragma warning restore CS0618 // Type or member is obsolete
            {
                MenuManager.mm.Open_FriendProfile(vh.value);
            });

            t.transform.position= new Vector3(1041.4f, (938f-(265.4f/2)) - (i - 1)* 265.4f, 0f);
            entries.Add(t);
            entriesToAdd.RemoveAt(0);
        }
    }

    void AddFriends(FirebaseDataManager.Profile entries)
    {
        int i = -1;
        foreach (var s in entries.friends)
        {
            i++;
            Debug.Log("FriendUID: " + s);
            FirebaseDataManager.fdm.ReadProfile(s, AddFriendsEntries);
        }
    }

    void AddFriendsEntries(FirebaseDataManager.Profile s)
    {
        Debug.Log("ADDFRIENDS---------"+s.username + "; " + s.uid);
        entriesToAdd.Add(new entryToAdd()
        {
            name = s.username,
            uid = s.uid
        });
    }
}