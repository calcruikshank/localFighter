using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int teamID = 0;
    public GameObject team0Prefab;
    public GameObject team1Prefab;
    public Transform text0;
    public Transform text1;
    // Start is called before the first frame update
    void Start()
    {
        /*PlayerController[] players = FindObjectsOfType<PlayerController>(); 
        foreach (PlayerController player in players)
        {
            
            
        }*/
    }

    public void SetTeam(PlayerController player)
    {
        player.team = teamID % 2;
        SetText(teamID, player);
        teamID++;
        Debug.Log(player.team);
        
    }

    public void SetText(int spawnLocation, PlayerController player)
    {
        if (spawnLocation == 0)
        {
            GameObject textObject = Instantiate(team0Prefab, text0.position, Quaternion.identity);
            textObject.transform.SetParent(text0, false);
            player.percentageText = textObject.GetComponent<TMP_Text>();
        }
        if (spawnLocation == 1)
        {
            GameObject textObject = Instantiate(team1Prefab, text1.position, Quaternion.identity);
            textObject.transform.SetParent(text1, false);
            player.percentageText = textObject.GetComponent<TMP_Text>();
        }

    }
}
