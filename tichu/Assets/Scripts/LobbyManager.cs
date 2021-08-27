using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1";
    public Text connectionInfoText;
    public Text UserNumber;
    public Text PageNumber;
    public Text StatusText;
    public InputField RoomNameInput;
    public Button[] RoomButton;
    public Button PreviousButton;
    public Button NextButton;


    List<RoomInfo> roomList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    public void MyListClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(roomList[multiple + num].Name);
        RoomListRenewal();
    }

    void RoomListRenewal()
    {
        maxPage = (roomList.Count % RoomButton.Length == 0) ? roomList.Count / RoomButton.Length : roomList.Count / RoomButton.Length + 1;

        PreviousButton.interactable = (currentPage <= 1) ? false : true;
        NextButton.interactable = (currentPage >= maxPage) ? false : true;

        multiple = (currentPage - 1) * RoomButton.Length;
        PageNumber.text = $"{currentPage} / {maxPage+1}";
        for (int i = 0;i < RoomButton.Length; i++)
        {
            RoomButton[i].interactable = (multiple + i < roomList.Count) ? true : false;
            RoomButton[i].transform.GetChild(0).GetComponent<Text>().text = (multiple + i < roomList.Count) ? roomList[multiple + i].Name : "";
            RoomButton[i].transform.GetChild(1).GetComponent<Text>().text = (multiple + i < roomList.Count) ? roomList[multiple + i].PlayerCount + "/" + roomList[multiple + i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> rList)
    {
        int roomCount = rList.Count;
        for (int i = 0;i < roomCount; i++)
        {
            if (!rList[i].RemovedFromList)
            {
                if (!roomList.Contains(rList[i])) roomList.Add(rList[i]);
                else roomList[roomList.IndexOf(rList[i])] = rList[i];
            }
            else if (roomList.IndexOf(rList[i]) != -1) roomList.RemoveAt(roomList.IndexOf(rList[i]));
        }
        RoomListRenewal();
    }

    private void Update()
    {
        UserNumber.text = PhotonNetwork.CountOfPlayers + " players connecting to the server";
        StatusText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        connectionInfoText.text = "Connection To Master Server...";
    }

    public override void OnConnectedToMaster()
    {
        connectionInfoText.text = "Online : Connected to Master Server";
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionInfoText.text = $"Offline : Connection Disabled {cause}";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.LocalPlayer.NickName = AuthManager.User.DisplayName;
        roomList.Clear();
    }

    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomNameInput.text == "" ? $"{PhotonNetwork.LocalPlayer.NickName}'s Room" : RoomNameInput.text, new RoomOptions { MaxPlayers = 4 });

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void JoinRoom() => PhotonNetwork.JoinRoom(RoomNameInput.text);

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();
    // enter room scene change
    public override void OnJoinedRoom()
    {

    }

    public override void OnCreateRoomFailed(short returnCode, string message) { StatusText.text = "Room Name Alrealy exist!";  }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        StatusText.text = $"The room {RoomNameInput.text} does not exist!";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        StatusText.text = $"Room does not exist!";
        PhotonNetwork.JoinLobby();
    }
}
