using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Menu_Scripts
{
    public class Room : MonoBehaviour
    {
        public TextMeshProUGUI Name;

        public void JoinRoom()
        {
            NetworkManager.Instance.JoinRoom(Name.text);
        }
    }
}
