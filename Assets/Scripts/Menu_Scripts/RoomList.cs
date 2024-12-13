using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Room = Assets.Scripts.Menu_Scripts.Room;

public class RoomList : MonoBehaviourPunCallbacks
{
    public GameObject roomPrefab;

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in GameObject.Find("RoomListContent").transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var r in roomList)
        {
            GameObject room = Instantiate(roomPrefab, GameObject.Find("RoomListContent").transform);
            room.GetComponent<Room>().Name.text = r.Name;
        }
    }
}