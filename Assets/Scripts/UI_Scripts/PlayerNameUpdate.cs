using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameUpdate : MonoBehaviour
{

    void Start()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = "Player Name";
    }

    void Update()
    {
        string playerName = "Unknown Player";
        GameObject place = gameObject.transform.parent.gameObject;

        for (int i = 0; i < place.transform.childCount; i++)
        {
            Transform child = place.transform.GetChild(i);
            if (child.name.Contains("Hand"))
            {
                playerName = child.gameObject.GetComponent<Player>().playerName;
                break;
            }
        }

        gameObject.GetComponent<TextMeshProUGUI>().text = playerName;
    }
}
