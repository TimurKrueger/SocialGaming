using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadTestUserCredentials : MonoBehaviour
{
    [System.Serializable]
    public struct CredentialPair {
        public string email;
        public string password;
    };

    public CredentialPair[] testUsers = new CredentialPair[4];

    public void InsertCredentials(int id)
    {
        if (id < 0 || id > 3) return;

        TMP_InputField t1, t2;

        t1 = FirebaseLoginManager.flm.email;
        t2 = FirebaseLoginManager.flm.pw;

        t1.text = testUsers[id].email;
        t2.text = testUsers[id].password;
    }
}
