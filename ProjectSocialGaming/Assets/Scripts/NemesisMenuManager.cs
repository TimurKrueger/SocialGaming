using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using System;
using UnityEngine.UI;

public class NemesisMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static NemesisMenuManager nmm;

    private void Awake()
    {
        if (nmm == null) nmm = this;
        else if (nmm != this) Destroy(this);

        //StartCoroutine(a());
    }

    IEnumerator a()
    {
        yield return null;
        LoadNemesis();
    }

    public Transform nemesisContainer;
    public Transform nemesisEntry;

    struct entryToAdd
    {
        public string name;
        public string uid;
    };

    struct WL
    {
        public long score;
        public int won_against;
        public int lost_against;
    };

    List<entryToAdd> entriesToAdd = new List<entryToAdd>();
    List<WL> wl = new List<WL>();
    List<Transform> entries = new List<Transform>();

    public void LoadNemesis()
    {
        //Debug.Log("Load Nemesis!");
        wipeNemesis();
        try
        {
            int i = 0;
            string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            FirebaseDataManager.fdm.ReadProfile(uid, AddNemesis);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void wipeNemesis()
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
            Debug.Log("Adding NemesisEntry "+entriesToAdd[0] + "; " + wl[0].score + "; " + i);

            Transform t = Instantiate(nemesisEntry, nemesisContainer);
            t.name = "Entry_" + i;

            t.Find("Pos").GetComponent<TextMeshProUGUI>().text = i + "";
            t.Find("Name").GetComponent<TextMeshProUGUI>().text = entriesToAdd[0].name;
            t.Find("Score").GetComponent<TextMeshProUGUI>().text = wl[0].score + "";
            t.Find("Losses").GetComponent<TextMeshProUGUI>().text = wl[0].lost_against.ToString();
            
            t.localPosition = new Vector3(0f, 608f - (i - 1) * 64f, 0f);

            var vh = t.gameObject.AddComponent<ValueHolder>();
            vh.value = entriesToAdd[0].uid;

            t.GetComponent<Button>().onClick.AddListener(() =>
            {
                MenuManager.mm.Open_NemesisProfile(vh.value);
            }); 

            entriesToAdd.RemoveAt(0);
            entries.Add(t);
            wl.RemoveAt(0);
        }
    }

    void AddNemesis(FirebaseDataManager.Profile entry)
    {
        //Debug.Log("Load Nemesis!");

        //Get Social Graph
        FirebaseDataManager.ComputeSocialGraph(entry, AddNemesisWithGraph);

        /*
        int i = -1;

        foreach (var s in entry.nemeses)
        {
            i++;
            FirebaseDataManager.fdm.ReadProfile(s.uid, AddNemesesEntries);
            wl.Add(new WL()
            {
                lost_against = s.lost_against,
                won_against = s.won_against,
                score = FirebaseDataManager.ComputeNemesisScore(entry, s),
            });
        }
        sort_(wl, entriesToAdd);*/
    }

    bool computing = false;

    void AddNemesisWithGraph(FirebaseDataManager.Profile entry, FirebaseDataManager.SocialGraph graph)
    {
        Debug.Log(graph);

        computing = true;
        int i = -1;
        
        foreach (var s in entry.nemeses)
        {
            i++;
            float dist = graph.GetDistanceBetween(entry.uid, s.uid, 3);

            print(dist);
            FirebaseDataManager.fdm.ReadProfile(s.uid, AddNemesesEntries);
            wl.Add(new WL()
            {
                lost_against = s.lost_against,
                won_against = s.won_against,
                score = FirebaseDataManager.ComputeNemesisScore_Main(entry, s, dist),
            });
        }
        sort_(wl, entriesToAdd);
    }

    void AddNemesesEntries(FirebaseDataManager.Profile s)
    {
        //Debug.Log(s.name + "; " + s.score + "; " + s.uid);
        entriesToAdd.Add(new entryToAdd()
        {
            name = s.username,
            uid = s.uid
        });
    }

    void sort_(List<WL> n, List<entryToAdd> e)
    {
        for (int i = 0; i < n.Count; i++)
        {
            for (int j = i + 1; j < n.Count; j++)
            {
                if (n[j].score > n[i].score)
                {
                    WL temp = n[j];
                    n[j] = n[i];
                    n[i] = temp;

                    entryToAdd w = e[j];
                    e[j] = e[i];
                    e[i] = w;
                }
            }
        }
    }


    /*private static int CompareEntries(entryToAdd a, entryToAdd b)
    {
        return a.score.CompareTo(b.score) * -1;
    }*/
}