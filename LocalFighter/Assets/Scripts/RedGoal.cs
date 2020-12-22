using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RedGoal : MonoBehaviour
{
    public PlayerController player;
    
    void OnCollisionEnter2D(Collision2D other)
    {
        player = other.transform.GetComponent<PlayerController>();
        if (player != null)
        {
            if (player.team == 0)
            {
                player.stocksLeft--;
                if (player.stocksLeft < 0)
                {
                    Debug.Log("RedLost");
                    
                }
                if (player.stocksLeft >= 0)
                {
                    player.Respawn();
                        //gameObject.transform.position = new Vector2(0, 0);
                    Debug.Log("lost a stock");
                }
            }
        }
    }
}
