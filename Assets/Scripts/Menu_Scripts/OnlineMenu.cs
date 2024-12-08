using System.Collections;
using System.Collections.Generic;
using Photon.Pun.Demo.Cockpit.Forms;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineMenu : MonoBehaviour
{
    [SerializeField]
    MainMenu MainMenu;

    [SerializeField]
    RoomMenu RoomMenu;

    [SerializeField] TMP_InputField input_Create;

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
        gameObject.SetActive(true);
    }

    public void OnExit()
    {
        NetworkManager.Instance.LeaveLobby();
        gameObject.SetActive(false);
    }

    public void Back()
    {
        MainMenu.OnEnter();
        OnExit();
    }
    
    public void CreateRoom()
    {
        NetworkManager.Instance.CreateRoom(input_Create.text);
    }

    public void JoinRoom(string roomName)
    {
        NetworkManager.Instance.JoinRoom(roomName);
    }
}

