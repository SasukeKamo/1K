using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineMenu : MonoBehaviour
{
    [SerializeField]
    GameObject MainMenu;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnter()
    {
        NetworkManager.Instance.JoinLobby();
    }

    public void OnExit()
    {
        NetworkManager.Instance.LeaveLobby();
    }

    public void Back()
    {
        OnExit();

        MainMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}

