using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Init Card")]
    public string[] names;
    public Sprite[] cardSprite;
    public Sprite empty;

    [Header("UI")]
    public Image[] HandImage;
    public Image[] HandRankingView;
    public GameObject GiveCardPanel;
    public GameObject InGameBtnBar;
    public GameObject GTBar;

    [Header("Button")]
    public Button[] PlayerBtn;
    public Button SmallTichuBtn;

    [Header("Text")]
    public Text CurScoreText;
    public Text HandRankingText;
    public Text TeamBlueText;
    public Text TeamRedText;


    private int[] deck = new int[56];
    public static Card[] hand = new Card[14];
    public static int pos = 0;
    public static bool[] gameStart = new bool[4];
    public static bool isGreatTichu;
    public static bool isSmallTichu;
    public static bool[] isChangeCard = new bool[4];
    public PhotonView PV;

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LoadLevel("Room");
    }

    void Awake()
    {
        getPos();
        for(int i = 0;i <PhotonNetwork.PlayerList.Length;i++)
            PlayerBtn[i].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
        newGame();
    }

    void newGame()
    {
        Init();
        Mulligan();

    }
    void Init()
    {
        GTBar.SetActive(true);
        InGameBtnBar.SetActive(false);
        GiveCardPanel.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            isChangeCard[i] = false;
            gameStart[i] = false;
            PlayerBtn[i].transform.GetChild(3).gameObject.SetActive(false);
            PlayerBtn[i].transform.GetChild(4).gameObject.SetActive(false);
            PlayerBtn[i].transform.GetChild(5).gameObject.SetActive(true);
        }
        isGreatTichu = false;
        isSmallTichu = false;
        for (int i = 0; i < 14; i++)
            HandRankingView[i].sprite = empty;
    }
    void InitDeck()
    {
        for (int i = 0;i < 56; i++)
        {
            deck[i] = i;
        }
    }
    public void Mulligan()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        InitDeck();
        Shuffle(deck, UnityEngine.Random.Range(0, 100));
        PV.RPC("GetHand", RpcTarget.All, deck);
        PV.RPC("ViewHand", RpcTarget.All);

    }

    public static int[] Shuffle(int[] array, int seed)
    {
        System.Random prng = new System.Random(seed);
        for (int i = 0;i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            int tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    void getPos()
    {
        for (int i = 0;i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                pos = i;
                return;
            }
        }
    }

    [PunRPC]
    void GetHand(int[] arr)
    {
        for(int i = 0;i < 14; i++)
        {
            int id = pos * 14 + i;
            if(arr[id] == 53)
                hand[i] = new Card(cardSprite[arr[id]], names[arr[id]], arr[id], 25);
            else if(arr[id] == 52)
                hand[i] = new Card(cardSprite[arr[id]], names[arr[id]], arr[id], -25);
            else if (arr[id] % 13 == 3)
                hand[i] = new Card(cardSprite[arr[id]], names[arr[id]], arr[id], 5);
            else if (arr[id] % 13 == 8 || arr[id] % 13 == 11)
                hand[i] = new Card(cardSprite[arr[id]], names[arr[id]], arr[id], 10);
            else
                hand[i] = new Card(cardSprite[arr[id]], names[arr[id]], arr[id], 0);
        }
    }

    [PunRPC]
    public void ViewHand()
    {
        if (!gameStart[pos])
        {
            Card[] temp = new Card[8];

            for (int i = 0; i < 8; i++)
                temp[i] = hand[i];
            Array.Sort(temp, delegate (Card a, Card b) {
                return Int32.Parse(a.name.Split()[1]) - Int32.Parse(b.name.Split()[1]);
            });
            for (int i = 0; i < 8;i++)
                HandImage[i].sprite = temp[i].sprite;
            for (int i = 8; i < 14;i++)
                HandImage[i].sprite = empty;
            
            return;
        }
        Array.Sort(hand, delegate (Card a, Card b) {
            return Int32.Parse(a.name.Split()[1]) - Int32.Parse(b.name.Split()[1]);
        });
        for (int i = 0;i < hand.Length; i++)
        {
            HandImage[i].sprite = hand[i].sprite;
        }
    }

    public void GreatTichuButton()
    {
        isGreatTichu = true;
        PV.RPC("ReceiveGreatTichu", RpcTarget.All, pos);
        SmallTichuBtn.gameObject.SetActive(false);
        PV.RPC("CheckGameStart", RpcTarget.All, pos);
        ViewHand();
    }

    public void NoTichuButton()
    {
        PV.RPC("CheckGameStart", RpcTarget.All, pos);
        ViewHand();
    }

    public void SmallTichuButton()
    {
        isSmallTichu = true;
        PV.RPC("ReceiveSmallTichu", RpcTarget.All, pos);
        SmallTichuBtn.gameObject.SetActive(false);
    }

    [PunRPC]
    void CheckGameStart(int p)
    {
        PlayerBtn[p].transform.GetChild(5).gameObject.SetActive(false);
        gameStart[p] = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        if((gameStart[0] && gameStart[1]) && (gameStart[2] && gameStart[3]))
        {
            PV.RPC("GameStart", RpcTarget.All);
        }
    }
    [PunRPC]
    void GameStart()
    {
        for(int i = 0; i < 4; i++)
            PlayerBtn[i].transform.GetChild(5).gameObject.SetActive(true);
        GTBar.SetActive(false);
        InGameBtnBar.SetActive(true);
        GiveCardPanel.SetActive(true);
    }
    [PunRPC]
    void ReceiveGreatTichu(int p)
    {
        PlayerBtn[p].transform.GetChild(3).gameObject.SetActive(true);
    }
    [PunRPC]
    void ReceiveSmallTichu(int p)
    {
        PlayerBtn[p].transform.GetChild(4).gameObject.SetActive(true);
    }
}
