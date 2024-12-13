using Photon.Pun;
using UnityEngine;

public class PlayerNetwork : MonoBehaviourPun
{
    private void Start()
    {
        if (!GameManager.IsMultiplayerMode)
        {
            this.enabled = false;
            return;
        }

        // Logika multiplayer
        if (!photonView.IsMine)
        {
            Destroy(this); // usuwanie kontroli nad obiektami innych graczy
        }
    }
}
