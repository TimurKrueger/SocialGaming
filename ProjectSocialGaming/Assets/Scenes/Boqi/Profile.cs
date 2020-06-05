using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;

public class Profile : MonoBehaviour
{
    public TextMeshProUGUI GP;
    public TextMeshProUGUI GW;
    public TextMeshProUGUI GL;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Update2()
    {
        Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser);
        try
        {
            FirebaseDataManager.fdm.ReadProfile(FirebaseAuth.DefaultInstance.CurrentUser.UserId, Update1);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        
    }

    public void Update1(FirebaseDataManager.Profile p)
    {
        
        GP.text = (p.games_lost + p.games_won).ToString();
        GW.text = p.games_won.ToString();
        GL.text = p.games_lost.ToString();

    }
}
