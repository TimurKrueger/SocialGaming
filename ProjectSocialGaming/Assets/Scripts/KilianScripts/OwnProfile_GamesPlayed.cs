using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OwnProfile_GamesPlayed : MonoBehaviour
{

    public TextMeshProUGUI tmp;

    public void _getData(List<string> data)
    {
        string result="";
        foreach (string p in data)
        {
            result += p + " ";
        }
        tmp.text = "Games Played: "+result+"\nGames Won: "+result+"\nGames Lost: "+result;
    }

    // Start is called before the first frame update
    public void Start()
    {
        //pfad zu eigenem profil brauchen wir noch
        getData();
    }

    void getData()
    {
        FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_1", _getData);
    }

    // Update is called once per frame
    void Update()
    {
        //FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_1", _getData);
    }

}
