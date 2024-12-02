using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        // laczenie z photonem
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("connected with photon");
        PhotonNetwork.JoinLobby(); // wejscie do lobby
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("welcome in lobby");
        // gui do wyboru pokoju
    }

    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("welcome in room");
        // wczytanie sceny
        SceneManager.LoadScene("GameScene");
    }
}
