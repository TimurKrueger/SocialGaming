using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OwnProfileName : MonoBehaviour
{

    public TextMeshProUGUI textMeshPro;
    public RectTransform refreshtrans;

    // Start is called before the first frame update
    void Start()
    {      
        getData();
    }

    public void getData()
    {
        //hier brauchen wir den pfad zu dem eigenen profil
        FirebaseDataManager.fdm.ReadDataList("/Leaderboard/user_0", _getData);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void refresh()
    {
        getData();
        refreshtrans.gameObject.SetActive(false);
        refreshtrans.gameObject.SetActive(true);
    }

    public void _getData(List<string> data)
    {
        string result = data[0];
        string result2 = data[1];

        textMeshPro.text = result + "         Score: " + result2;
    }
}
