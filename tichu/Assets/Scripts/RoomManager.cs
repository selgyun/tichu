using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public Text RoomNameText;
    public Text[] ChatText;
    public Button[] userList;
    public InputField ChatInput;

    public PhotonView PV;
    public Text ReadyButtonText;

    private int pos = 0;
    private bool isReady = false;

    void Awake()
    {
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        for (int i = 0; i < ChatText.Length; i++)
        {
            ChatText[i].text = "";
        }
        UpdateUserList();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ChatRPC("<color=blue>" + newPlayer.NickName + " entered the room.</color>");
        UpdateUserList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ChatRPC("<color=blue>" + otherPlayer.NickName + " left the room.</color>");
        UpdateUserList();
    }

    public void QuitRoom()
    {
        isReady = false;
        PV.RPC("UpdateReady", RpcTarget.All, isReady, pos);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
        {
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        }
        if (!isInput)
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }

    [PunRPC]
    void UpdateUserList()
    {
        for (int i = 0; i < userList.Length; i++)
        {
            userList[i].transform.GetChild(0).GetComponent<Text>().text = "";
            userList[i].transform.GetChild(1).GetComponent<Text>().text = "";
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            userList[i].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
        }

        PV.RPC("FindPos", RpcTarget.All);
        PV.RPC("UpdateReady", RpcTarget.All, isReady, pos);
    }

    [PunRPC]
    void UpdateReady(bool ready, int p)
    {
        if (ready)
            userList[p].transform.GetChild(1).GetComponent<Text>().text = "Ready";
        else
            userList[p].transform.GetChild(1).GetComponent<Text>().text = "";
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        bool isStart = true;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (userList[i].transform.GetChild(1).GetComponent<Text>().text != "Ready")
            {
                isStart = false;
                break;
            }
        }
        if (isStart && PhotonNetwork.PlayerList.Length == 4)
            ReadyButtonText.text = "Game Start";
        else
            ReadyButtonText.text = "Ready";
    }
    
    [PunRPC]
    public void FindPos()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                pos = i;
                break;
            }
        }
    }

    public void ReadyBtn()
    {
        if (ReadyButtonText.text == "Game Start")
        {
            PV.RPC("GameStart", RpcTarget.All);
            return;
        }
        FindPos();
        isReady = !isReady;
        PV.RPC("UpdateReady", RpcTarget.All, isReady, pos);
    }

    [PunRPC]
    void GameStart()
    {
        PhotonNetwork.LoadLevel("Main");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        for(int i = 0;i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (userList[i].transform.GetChild(0).GetComponent<Text>().text == newMasterClient.NickName)
            {
                userList[i].transform.GetChild(0).GetComponent<Text>().color = Color.blue;
                break;
            }
        }
    }
}
