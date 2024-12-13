using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class RoomMenu : MonoBehaviourPun
{
    [SerializeField]
    OnlineMenu OnlineMenu;

    [SerializeField]
    public TextMeshProUGUI Title;

    [SerializeField] 
    public List<TextMeshProUGUI> playerNames;

    [SerializeField] 
    public TextMeshProUGUI nameInputField;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }   

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerNameChanged(string value)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            Debug.Log("Changing player " + PhotonNetwork.LocalPlayer.ActorNumber + " name to '" + value + "'");
            PhotonNetwork.LocalPlayer.NickName = value;
            GetComponent<PhotonView>().RPC("SyncPlayerNames", RpcTarget.All);
        }
        else
        {
            var newName = "Player" + PhotonNetwork.LocalPlayer.ActorNumber;
            Debug.Log("Changing player " + PhotonNetwork.LocalPlayer.ActorNumber + " name to '" + newName + "'");
            PhotonNetwork.LocalPlayer.NickName = newName;
            GetComponent<PhotonView>().RPC("SyncPlayerNames", RpcTarget.All);
        }
    }
    public void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public void OnExit()
    {
        PhotonNetwork.LeaveRoom();
        gameObject.SetActive(false);
    }

    public void Back()
    {
        OnlineMenu.OnEnter();
        OnExit();
    }

    public void StartGame()
    {
        GameManager.IsMultiplayerMode = true;
        photonView.RPC("SetMultiplayerMode", RpcTarget.AllBuffered);
        PhotonNetwork.LoadLevel("GameScene");
    }

    [PunRPC]
    void SetMultiplayerMode()
    {
        GameManager.IsMultiplayerMode = true;
    }

    [PunRPC]
    private void SyncPlayerNames()
    {
        for (int i=0; i<PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            Debug.Log("Syncing name for player "+(i+1));
            playerNames[i].text = PhotonNetwork.CurrentRoom.Players[i+1].NickName;
        }
    }

    public void OnJoinedRoom()
    {
        OnEnter();

        var playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        PhotonNetwork.LocalPlayer.NickName = "Player" + playerNumber;
        Title.text = PhotonNetwork.CurrentRoom.Name;

        //playerNames[playerNumber-1].text = "Tester" + playerNumber;
        GetComponent<PhotonView>().RPC("SyncPlayerNames", RpcTarget.All);

        Debug.Log("You joined room '" + PhotonNetwork.CurrentRoom.Name + "' as " + PhotonNetwork.LocalPlayer.NickName + "'.");
    }
}

