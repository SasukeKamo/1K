using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Room = Assets.Scripts.Menu_Scripts.Room;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager _instance;

    public TMP_InputField input_Create;
    public TMP_InputField input_Join;


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
        PhotonNetwork.LeaveRoom();
        LeaveLobby();
        JoinLobby();
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

    public void CreateRoom()
    {
        CreateRoom(input_Create.text);
    }

    public void CreateRoom(string roomName)
    {
        //Debug.Log("Creating Room '" + input_Create.text + "'");1
        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = 4, IsVisible = true, IsOpen = true });
        //Debug.Log("Created Room '" + input_Create.text + "'");
    }

    public void JoinRoom()
    {
        JoinRoom(input_Join.text);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
        // wczytanie sceny
        //SceneManager.LoadScene("GameScene");
    }
}
