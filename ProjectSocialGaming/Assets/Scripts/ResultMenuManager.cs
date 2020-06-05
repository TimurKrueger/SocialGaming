using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultMenuManager : MonoBehaviour
{
    #region Singleton Structure
    public static ResultMenuManager rmm;

    private void Awake()
    {
        if (rmm == null) rmm = this;
        else if (rmm != this) Destroy(this);
    }
    #endregion

    /// <summary>
    /// Calculate the rank that was achieved with the given score and update the Result-Text respectively
    /// </summary>
    /// <param name="score"></param>
    public void CalculateRank(long score)
    {
        DatabaseReference _ref = FirebaseDatabase.DefaultInstance.RootReference.Child("Leaderboard");

        _ref.GetValueAsync().ContinueWith(task =>
        {
            DataSnapshot ds = task.Result;

            string leaderboard = "";
            foreach(DataSnapshot _ds in ds.Children)
            {
                string str = "Key: " + _ds.Key + "; Name: " + _ds.Child("name").Value + "; Score: " + _ds.Child("score");
                Debug.Log(str);
                leaderboard += str;
            }

            Debug.Log(leaderboard);
        });
    }   
}
