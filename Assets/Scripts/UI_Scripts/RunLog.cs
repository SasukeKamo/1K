using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunLog : MonoBehaviour
{
    [SerializeField]
    private string text;
    [SerializeField]
    private Color color;

    [SerializeField]
    private TextLogControl logControl;

    public void logText()
    {
        logControl.LogText(text, color);
    }
}
