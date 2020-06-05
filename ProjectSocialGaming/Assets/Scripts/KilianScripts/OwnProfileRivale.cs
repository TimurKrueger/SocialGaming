using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OwnProfileRivale : MonoBehaviour
{
    public TextMeshProUGUI tmp;

    public void _getData(List<string> data)
    {
        string result = "";
        foreach (string p in data)
        {
            result += p + " ";
        }
        tmp.text = "Rivals Name: " + result+"       Nemesis-Score: "+result+"\nGames Played against your Rival: "+result+"\nWin/Loss ratio: "+result;
    }

    // Start is called before the first frame update
    void Start()
    {
        getData();
    }

    public void getData()
    {
        //pfad zu eigenem profil brauchen wir noch
        FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_0", _getData);
    }

    // Update is called once per frame
    void Update()
    {
        //FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_1", _getData);
    }
}
