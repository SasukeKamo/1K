using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    SettingsMenu SettingsMenu;
    [SerializeField]
    OnlineMenu OnlineMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void PlayLocalGame()
    {
        Debug.Log("Loading HotSeat scene.");
        SceneManager.LoadScene("SampleScene");
    }

    public void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public void OnExit()
    {
        gameObject.SetActive(false);
    }


    public void EnterSettingsMenu()
    {
        SettingsMenu.OnEnter();
        OnExit();
    }

    public void EnterOnlineMenu()
    {
        OnlineMenu.OnEnter();
        OnExit();
    }

    public void Exit()
    {
        Debug.Log("Exit.");
        Application.Quit();
    }
}

