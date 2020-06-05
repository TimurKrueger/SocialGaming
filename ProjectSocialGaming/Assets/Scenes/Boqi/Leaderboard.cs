using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Leaderboard : MonoBehaviour
{

    public Transform LeaderboardContainer;
    public Transform LeaderboardEntry;
    public TextMeshProUGUI pos1;
    public TextMeshProUGUI name1;
    public TextMeshProUGUI lose1;
    public TextMeshProUGUI score1;
    // Start is called before the first frame update

    private void open()
    {
        //nemesisContainer = transform.Find("HighscoreContainer");
        //nemesisEntry = transform.Find("Panel");

        LeaderboardEntry.gameObject.SetActive(false);

        for (int i = 0; i < 10; i++)
        {
            FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_" + i, _open);
            index++;
        }
        
    }
    /*public void Loadnemesisdata()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, nemesisData);
        if(File.Exists (filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
        }
    }*/
    int index = 0;
    struct entryToAdd {
        public string name;
        public string score;
        public int pos;
    };
    List<entryToAdd> entrys = new List<entryToAdd>();
    public void _open(List<string> data)
{
        if(data.Count == 0)
        {
            return;
        }
        foreach(string p in data)
        {
            //print(p);
        }
        entrys.Add(new entryToAdd() { name = data[0], score = data[1], pos = index });
        //print("ENTRYS: COUNT" + entrys.Count);
     }

    void Start()
    {
        open();
        Update1();
    }

    // Update is called once per frame
    public void Update1()
    {
        index = 0;
        //print("NOOOOOOOOOOOO");
        foreach (entryToAdd p in entrys)
        {
            //print("WHHHHHHHHHHHHHHH");
            float templateHeight = 50f;
            Transform entryTransform = Instantiate(LeaderboardEntry, LeaderboardContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * index);
            entryTransform.gameObject.SetActive(true);


            int rank = index + 1;
            index++;
            string rankString;
            switch (rank)
            {
                default:
                    rankString = rank + "TH"; break;

                case 1: rankString = "1ST"; break;
                case 2: rankString = "2nd"; break;
                case 3: rankString = "3rd"; break;
            }

            pos1.text = rankString;

            name1.text = p.name;
            int wins = Random.Range(0, 10);
            lose1.text = wins.ToString();

            int score = Random.Range(0, 10000);
            score1.text = p.score;
        }
        
    }
}
