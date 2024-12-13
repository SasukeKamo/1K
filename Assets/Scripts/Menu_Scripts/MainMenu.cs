using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    GameObject SettingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayGameHotSeat()
    {
        Debug.Log("Loading HotSeat scene.");
        SceneManager.LoadScene("SampleScene");
    }


    public void EnterSettingsMenu()
    {
        SettingsMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Exit()
    {
        Debug.Log("Exit.");
        Application.Quit();
    }
}

