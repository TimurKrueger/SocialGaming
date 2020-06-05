using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;


public class FriendList : MonoBehaviour
{

    public FriendList fl;

    //public Canvas canvas;
    public RectTransform content;
    public RectTransform friendPanel;
    public RectTransform noFriendsPanel;

    public bool noFriends;

    // Start is called before the first frame update
    void Start()
    {
        print("----------------------------------------------------------");
        List<RectTransform> listRec = new List<RectTransform>();
        Vector3 v = new Vector3(friendPanel.gameObject.transform.position.x + 1955.0f, friendPanel.gameObject.transform.position.y, friendPanel.gameObject.transform.position.z);
        if (noFriends)
        {
            friendPanel.gameObject.SetActive(false);
            noFriendsPanel.gameObject.SetActive(true);
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                if (i == 0)
                {
                    v = new Vector3(v.x, (v.y + (0 * friendPanel.sizeDelta.y)), v.z);
                }
                else
                {
                    v = new Vector3(v.x, (v.y + friendPanel.sizeDelta.y), v.z);
                }
                if (i > 2)
                {
                    content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y - friendPanel.sizeDelta.y);
                }
                RectTransform tr = Instantiate(friendPanel, v, friendPanel.rotation);
                listRec.Add(tr);
            }

            listRec.ForEach((t) => { t.gameObject.transform.parent = content.gameObject.transform; });
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
