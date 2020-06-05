using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootCollider : MonoBehaviour
{
    public Player p;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        p.SetGrounded();
    }
}
