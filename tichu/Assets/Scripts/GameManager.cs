using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class GameManager : MonoBehaviourPunCallbacks
{
    public string[] names;
    public Sprite[] cardSprite;
    public Card[] deck;

    public PhotonView PV;
    void Awake()
    {
        Mulligan();
    }

    public void Mulligan()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

    }
}
