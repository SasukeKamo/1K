using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameUpdate : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = "Player Name";
    }

    // Update is called once per frame
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
