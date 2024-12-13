using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private SettingsMenu SettingsMenu;
    [SerializeField]
    private OnlineMenu OnlineMenu;
    [SerializeField] 
    private GameObject loadGameObject;


    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlayMenuSong();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        FileInfo file = new FileInfo(GameManager.savePath);
        if (file.Exists)
        {
            loadGameObject.SetActive(true);
        }
    }


    public void PlayLocalGame()
    {
        Debug.Log("Loading HotSeat scene.");
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueLocalGame()
    {
        GameManager.IsGameContinued = true;
        SceneManager.LoadScene("GameScene");
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

