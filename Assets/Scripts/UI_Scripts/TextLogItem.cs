using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextLogItem : MonoBehaviour
{
    public void SetText(string text, Color color)
    {
        GetComponent<TextMeshProUGUI>().text = text;
        GetComponent<TextMeshProUGUI>().color = color;
    }
}
