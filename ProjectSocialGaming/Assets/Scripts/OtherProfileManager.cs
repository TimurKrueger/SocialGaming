using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtherProfileManager : MonoBehaviour
{
    public static OtherProfileManager opm;

    [Header("References")]
    public TextMeshProUGUI text_name;
    public TextMeshProUGUI text_games;
    public TextMeshProUGUI text_GamesAgainstYou;
    public TextMeshProUGUI text_NemesisScoreText;

    public Button button_addFriend;
    public ValueHolder button_addFriend_vh;
    public Image button_addFriend_image;
    public TextMeshProUGUI button_addFriend_text;

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
    public string p_uid;

    public int score = -1;

    private string uid;

    private void Awake()
    {
        if (opm == null) opm = this;
        else if (opm != this) Destroy(this);
    }

    bool updated = false;
    int setButtonValuesDelayed = 0;
    private void Update()
    {
        if (updated)
        {
            updated = false;
            text_name.text = p_name;
            text_games.text =
                "Games Played: " + p_played + "\n" +
                "Games Won: " + p_won + "\n" +
                "Games Lost: " + p_lost + "\n" +
                "Win Ratio: " + p_ratio;
            string current_player = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            FirebaseDataManager.fdm.ReadProfile(current_player, setNemText1);
        }
        if(setButtonValuesDelayed != 0)
        {
            setButtonValuesDelayed = 0;

            if(setButtonValuesDelayed == 1)
            {
                button_addFriend_image.color = Color.white;
                button_addFriend_text.text = "Add as friend";
            }
            else
            {
                button_addFriend_image.color = Color.red;
                button_addFriend.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Remove friend";
            }
        }
    }

    public void setNemText1(FirebaseDataManager.Profile e)
    {
        FirebaseDataManager.ComputeSocialGraph(e, setNemText);
    }

    public void setNemText(FirebaseDataManager.Profile e, FirebaseDataManager.SocialGraph graph)
    {
        foreach (var s in e.nemeses)
        {
            if (s.uid == p_uid)
            {
                float dist = graph.GetDistanceBetween(e.uid, s.uid, 3);
                score = FirebaseDataManager.ComputeNemesisScore_Main(e, s, dist);
                Debug.Log("Score: " + score);
            }
        }
        if (score >= 0)
        {
            text_NemesisScoreText.text = "Nemesis: " + score;
        }
        else
        {
            text_NemesisScoreText.text = "No Nemesis.";
        }
    }
    
    public void UpdateValues(string uid)
    {
        Debug.Log("Fetching profile Data of User '"+uid+"'!");

        ResetValues();

        FirebaseDataManager.fdm.ReadProfile(uid, RetrieveValues);
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

        score = -1;

        updated = true;
    }

    private void RetrieveValues(FirebaseDataManager.Profile p)
    {
        Debug.Log("Setting own profile data: " + p.ToString());

        p_name = p.username;
        p_uid = p.uid;
        p_won = p.games_won;
        p_lost = p.games_lost;
        p_played = p_won + p_lost;
        p_ratio = (float)p_won / (float)p_played;
        if (float.IsNaN(p_ratio)) p_ratio = 0.0f;

        p_ratio *= 1000f;
        p_ratio = Mathf.Round(p_ratio);
        p_ratio /= 10f;

        string temp = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        try
        {
            if (!p.friends.Contains(temp))
            {
                Debug.Log("Setting Friend Button to ADD");
                button_addFriend_vh.value = p.uid;
                button_addFriend_vh.value2 = temp;

                setButtonValuesDelayed = 1;

                button_addFriend.interactable = true;
                button_addFriend.onClick.RemoveAllListeners();
                button_addFriend.onClick.AddListener(() =>
                {
                    Debug.Log("Hi");
                    FirebaseDataManager.fdm.AddFriend(button_addFriend_vh.value, button_addFriend_vh.value2);
                    UpdateValues(p.uid);
                    setButtonValuesDelayed = 2;
                });
            }
            else
            {
                Debug.Log("Setting Friend button to REMOVE");
                button_addFriend_vh.value = p.uid;
                button_addFriend_vh.value2 = temp;

                setButtonValuesDelayed = 2;

                button_addFriend.interactable = true;
                button_addFriend.onClick.RemoveAllListeners();
                button_addFriend.onClick.AddListener(() =>
                {
                    Debug.Log("Bye");
                    FirebaseDataManager.fdm.RemoveFriend(button_addFriend_vh.value, button_addFriend_vh.value2);
                    UpdateValues(p.uid);
                    setButtonValuesDelayed = 1;
                });
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }


        if (p.nemeses.Count == 0)
        {
            updated = true;
            p_rival = "";
            //return;
        }
        updated = true;
        //FirebaseDataManager.ComputeNemesisScore(p, SetRivalData);
    }
}
