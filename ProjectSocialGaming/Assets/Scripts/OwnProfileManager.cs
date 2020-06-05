using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OwnProfileManager : MonoBehaviour
{
    public static OwnProfileManager opm;

    [Header("References")]
    public TextMeshProUGUI text_name;
    public TextMeshProUGUI text_games;
    public TextMeshProUGUI text_nemesis;

    [Header("Data")]
    public string p_name;
    public int p_played;
    public int p_won;
    public int p_lost;
    public float p_ratio;

    public string p_rival;
    public int p_nscore;
    public int p_nplayed;
    public float p_nratio;

    private void Awake()
    {
        if (opm == null) opm = this;
        else if (opm != this) Destroy(this);
    }

    bool updated = false;
    private void Update()
    {
        if(updated)
        {
            updated = false;
            text_name.text = p_name;
            text_games.text =
                "Games Played: " + p_played + "\n" +
                "Games Won: " + p_won + "\n" +
                "Games Lost: " + p_lost + "\n" +
                "Win Ratio: " + p_ratio;
            if (p_rival == "")
            {
                text_nemesis.text = "No nemesis found yet!\nPlay more often and add other players as friends";
            }
            else
            {
                text_nemesis.text =
                    "Rival: " + p_rival + "\n" +
                    "Nemesis Score: " + p_nscore + "\n" +
                    "Games Played Against Them: " + p_nplayed + "\n" +
                    "Win Ratio Against Them: " + p_nratio;
            }
        }
    }

    public void UpdateValues()
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.LogError("Current User not logged in! Aborting!");
            MenuManager.mm.Return_OwnProfile();
            return;
        }

        Debug.Log("Updating own profile Data!");

        ResetValues();

        FirebaseDataManager.fdm.ReadProfile(FirebaseAuth.DefaultInstance.CurrentUser.UserId, RetrieveValues);
    }

    public void ResetValues()
    {
        p_name = "Unknown";
        p_won = -1;
        p_lost = -1;
        p_played = -1;
        p_ratio = -1;

        p_nscore = -1;
        p_nplayed = -1;
        p_nratio = -1;
        p_rival = "Unknown";

        updated = true;
    }

    private void RetrieveValues(FirebaseDataManager.Profile p)
    {
        Debug.Log("Setting own profile data: "+p.ToString());

        p_name = p.username;
        p_won = p.games_won;
        p_lost = p.games_lost;
        p_played = p_won + p_lost;
        p_ratio = (float)p_won / (float)p_played;
        if (float.IsNaN(p_ratio)) p_ratio = 0.0f;

        p_ratio *= 1000f;
        p_ratio = Mathf.Round(p_ratio);
        p_ratio /= 10f;


        if (p.nemeses.Count == 0)
        {
            updated = true;
            p_rival = "";
            return;
        }

        //FirebaseDataManager.ComputeNemesisScore(p, SetRivalData);
        
        List<Tuple<int, FirebaseDataManager.Profile.NemesisEntry>> nemeses = new List<Tuple<int, FirebaseDataManager.Profile.NemesisEntry>>();

        foreach(var ne in p.nemeses)
        {
            int nScore = FirebaseDataManager.ComputeNemesisScore(p, ne);
            nemeses.Add(new Tuple<int, FirebaseDataManager.Profile.NemesisEntry>(nScore, ne));
            Debug.Log("Found Nemesis: " + ne.uid + "; " + nScore);
        }

        nemeses.Sort(CompareNemesisEntries);

        Tuple<int, FirebaseDataManager.Profile.NemesisEntry> rival = nemeses[0];
        
        p_nscore = rival.Item1;
        p_nplayed = rival.Item2.lost_against + rival.Item2.won_against;
        p_nratio = (float)rival.Item2.won_against / (float)p_nplayed;
        if (float.IsNaN(p_nratio)) p_nratio = 0.0f;

        p_nratio *= 1000f;
        p_nratio = Mathf.Round(p_nratio);
        p_nratio /= 10f;

        FirebaseDataManager.fdm.ReadProfile(rival.Item2.uid, SetRivalName);
    }

    public void SetRivalData(FirebaseDataManager.Profile p, List<Tuple<int, FirebaseDataManager.Profile.NemesisEntry>> nemeses)
    {

    }

    private void SetRivalName(FirebaseDataManager.Profile r)
    {
        p_rival = r.username;

        updated = true;
    }

    public static int CompareNemesisEntries(Tuple<int, FirebaseDataManager.Profile.NemesisEntry> a, Tuple<int, FirebaseDataManager.Profile.NemesisEntry> b)
    {
        return b.Item1.CompareTo(a.Item1);
    }
}