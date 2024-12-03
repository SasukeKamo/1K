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
        for (int i = 0; i < roomList.Count; i++)
        {
            GameObject room = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity,
                GameObject.Find("RoomListContent").transform);
            room.GetComponent<Room>().Name.text = roomList[i].Name;
        }
    }
}