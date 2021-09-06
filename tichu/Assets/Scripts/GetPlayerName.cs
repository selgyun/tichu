using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon;
using Photon.Realtime;
using Photon.Pun;

public class GetPlayerName : MonoBehaviour
{
    public int id;
    void Start()
    {
        this.GetComponent<Text>().text = PhotonNetwork.PlayerList[(GameManager.pos + id) % 4].NickName;
    }
}
