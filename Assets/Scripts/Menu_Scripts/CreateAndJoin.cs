using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class CreateAndJoin : MonoBehaviourPunCallbacks
{
    public TMP_InputField input_Create;
    public TMP_InputField input_Join;

    public void CreateRoom()
    {
        Debug.Log("Creating Room '"+input_Create.text+"'");
        PhotonNetwork.CreateRoom(input_Create.text, new RoomOptions() {MaxPlayers = 4, IsVisible = true, IsOpen = true});
        Debug.Log("Created Room '" + input_Create.text + "'");
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(input_Join.text);
    }

    public void JoinRoomInList(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        //PhotonNetwork.LoadLevel("SampleScene");
        print("Joined Room '"+PhotonNetwork.CurrentRoom.Name+"'");
    }
}
