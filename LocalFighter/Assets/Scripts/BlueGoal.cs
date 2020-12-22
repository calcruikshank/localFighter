using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueGoal : MonoBehaviour
{
    public PlayerController player;

    void OnCollisionEnter2D(Collision2D other)
    {
        player = other.transform.GetComponent<PlayerController>();
        if (player != null)
        {
            if (player.team == 1)
            {
                player.stocksLeft--;
                if (player.stocksLeft < 0)
                {
                    Debug.Log("BlueLost");

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
