using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class OtherProfileScript : MonoBehaviour
{
    public TextMeshProUGUI tmp;
    public RectTransform refreshtrans;

    public void _getData(List<string> data)
    {
        string result = "";
        foreach (string p in data)
        {
            result += p + " ";
        }
        tmp.text = "                    ProfileName\n\n\n\n\nGames Played: " + result + "\nGames Won: " + result + "\nGames Lost: " + result;
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

    public void refresh()
    {
        getData();
        refreshtrans.gameObject.SetActive(false);
        refreshtrans.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        //FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_1", _getData);
    }
}
