using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhotonNetwork = Photon.Pun.PhotonNetwork;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager _instance;

    [SerializeField]
    RoomMenu RoomMenu;

    [SerializeField] 
    OnlineMenu OnlineMenu;

    public static NetworkManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("NetworkManager");
                _instance = go.AddComponent<NetworkManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // laczenie z photonem
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }

    public void ReJoinLobby()
    {
        if(PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
        //PhotonNetwork.GetCustomRoomList(TypedLobby.Default, "");
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("connected with photon");
        PhotonNetwork.JoinLobby(); // wejscie do lobby
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        // gui do wyboru pokoju
    }

    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = 4, IsVisible = true, IsOpen = true });
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room '"+PhotonNetwork.CurrentRoom.Name+"'");

        OnlineMenu.OnExit();
        RoomMenu.OnJoinedRoom();

        // wczytanie sceny
        //SceneManager.LoadScene("GameScene");
    }
}
