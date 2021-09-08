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
    int[] scores = new int[56];
    public Sprite empty;
    public Sprite panel;

    [Header("UI")]
    public Image[] HandImage;
    public Image[] HandRankingView;
    public GameObject GiveCardPanel;
    public GameObject InGameBtnBar;
    public GameObject GTBar;

    [Header("Button")]
    public Button[] PlayerBtn;
    public Button SmallTichuBtn;
    public Button PassBtn;
    public Button BombBtn;
    public Button ConformBtn;

    [Header("Text")]
    public Text CurScoreText;
    public Text HandRankingText;
    public Text TeamBlueText;
    public Text TeamRedText;


    private int[] deck = new int[56];
    public static Card[] hand = new Card[14];
    public static int[] CardSwapPack = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public static int pos = 0;
    public static bool[] gameStart = new bool[4]; // 처음 게임 시작 판단
    public static bool isGreatTichu;
    public static bool isSmallTichu;
    public static bool[] isChangeCard = new bool[4]; // 카드 교환 여부 판단
    public static int[] actions = { 0, 0, 0, 0 }; // pass했는지 카드 냈는지 확인
    public enum Rank { Empty, Single, Pair, ContinuousPair, Triple, Straight, FullHouse, FourOfaKind, StraightFlush, Bird, Dragon, Phoenix, Dog}

    public PhotonView PV;

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LoadLevel("Room");
    }

    void Awake()
    {
        getPos();
        for(int i = 0;i < 56; i++)
        {
            if (i == 53)
                scores[i] = 25;
            else if (i == 54)
                scores[i] = -25;
            else if (i % 13 == 3)
                scores[i] = 5;
            else if (i % 13 == 8 || i % 13 == 11)
                scores[i] = 10;
            else
                scores[i] = 0;
        }
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
        PassBtn.interactable = false;
        BombBtn.interactable = false;
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
            hand[i] = new Card(cardSprite[arr[id]], names[arr[id]], arr[id], scores[arr[id]]);
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
            hand[i].cardpos = i;
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

    public void CardSwap()
    {
        ConformBtn.interactable = false;
        GiveCardPanel.transform.GetChild(1).GetComponent<Text>().text = "Wait for other players..";
        PV.RPC("SendCardSwap", RpcTarget.MasterClient, GiveCardImage.giveCard[0].id, GiveCardImage.giveCard[1].id, GiveCardImage.giveCard[2].id, pos);
        isChangeCard[pos] = true;
        for (int i = 0; i < 14; i++)
            CardClick.interactable[i] = false;
    }

    public void CardSwapDone()
    {
        ConformBtn.interactable = false;
        GiveCardPanel.transform.GetChild(1).GetComponent<Text>().text = "Place a card by touching!";
        for(int i = 0; i < 3; i++)
        {
            GameObject.FindGameObjectWithTag("CardSwapImage").transform.GetChild(i).GetComponent<Image>().sprite = panel;
            GiveCardImage.giveCard[i] = null;
        }
        GiveCardPanel.gameObject.SetActive(false);
    }

    [PunRPC]
    public void SendCardSwap(int a, int b, int c, int p)
    {
        isChangeCard[p] = true;
        CardSwapPack[p * 3] = a;
        CardSwapPack[p * 3 + 1] = b;
        CardSwapPack[p * 3 + 2] = c;
        if ((isChangeCard[0] && isChangeCard[1]) && (isChangeCard[2] && isChangeCard[3]))
            PV.RPC("ReceiveCardSwap", RpcTarget.All, CardSwapPack);
    }
    [PunRPC]
    public void ReceiveCardSwap(int[] swapPack)
    {
        GiveCardPanel.transform.GetChild(1).GetComponent<Text>().text = "Check your cards";
        switch (pos){
            case 0:
                GiveCardImage.giveCard[0] = new Card(cardSprite[swapPack[5]], names[swapPack[5]], swapPack[5], scores[swapPack[5]]);
                GiveCardImage.giveCard[1] = new Card(cardSprite[swapPack[7]], names[swapPack[7]], swapPack[7], scores[swapPack[7]]);
                GiveCardImage.giveCard[2] = new Card(cardSprite[swapPack[9]], names[swapPack[9]], swapPack[9], scores[swapPack[9]]);
                break;
            case 1:
                GiveCardImage.giveCard[0] = new Card(cardSprite[swapPack[8]], names[swapPack[8]], swapPack[8], scores[swapPack[8]]);
                GiveCardImage.giveCard[1] = new Card(cardSprite[swapPack[10]], names[swapPack[10]], swapPack[10], scores[swapPack[10]]);
                GiveCardImage.giveCard[2] = new Card(cardSprite[swapPack[0]], names[swapPack[0]], swapPack[0], scores[swapPack[0]]);
                break;
            case 2:
                GiveCardImage.giveCard[0] = new Card(cardSprite[swapPack[11]], names[swapPack[11]], swapPack[11], scores[swapPack[11]]);
                GiveCardImage.giveCard[1] = new Card(cardSprite[swapPack[1]], names[swapPack[1]], swapPack[1], scores[swapPack[1]]);
                GiveCardImage.giveCard[2] = new Card(cardSprite[swapPack[3]], names[swapPack[3]], swapPack[3], scores[swapPack[3]]);
                break;
            case 3:
                GiveCardImage.giveCard[0] = new Card(cardSprite[swapPack[2]], names[swapPack[2]], swapPack[2], scores[swapPack[2]]);
                GiveCardImage.giveCard[1] = new Card(cardSprite[swapPack[4]], names[swapPack[4]], swapPack[4], scores[swapPack[4]]);
                GiveCardImage.giveCard[2] = new Card(cardSprite[swapPack[6]], names[swapPack[6]], swapPack[6], scores[swapPack[6]]);
                break;
        }
        for (int i = 0; i < 3; i++)
        {
            GameObject.FindGameObjectWithTag("CardSwapImage").transform.GetChild(i).GetComponent<Image>().sprite = GiveCardImage.giveCard[i].sprite;
        }
        ConformBtn.interactable = true;
    }


    [PunRPC]
    public void Turn(int cur)
    {
        for (int i = 0;i < 4;i++)
        {
            // 턴을 표시해 주는 장치
        }

        if (cur != pos)
        {
            ConformBtn.interactable = false;
            PassBtn.interactable = false;
            return;
        }
    }

    public Rank judgeRank(int[] cardIds)
    // 정렬된 상태라 가정
    {
        switch (cardIds.Length)
        {
            case 0:
                return Rank.Empty;
            case 1:
                switch (cardIds[0])
                {
                    case 52:
                        return Rank.Bird;
                    case 53:
                        return Rank.Dragon;
                    case 54:
                        return Rank.Phoenix;
                    case 55:
                        return Rank.Dog;
                    default:
                        break;
                }
                return Rank.Single;
            case 2:
                if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]))
                {
                    return Rank.Pair;
                }
                else if (cardIds[0] == 54 || cardIds[1] == 54)
                    return Rank.Pair;

                return Rank.Empty;
            case 3: // 피닉스 처리해야함
                if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]))
                {
                    return Rank.Triple;
                }
                return Rank.Empty;
            case 4: // 피닉스 처리 해야함
                if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]) &&
                    Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]))
                {
                    return Rank.FourOfaKind;
                }
                if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[2]].Split()[1]) &&
                    Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]))
                {
                    return Rank.ContinuousPair;
                }
                return Rank.Empty;
            default:
                bool isStraight = true;
                int cnt = 0;
                bool isStraightFlush = true;
                bool isPhoenix = false;
                bool isContinuousPair = true;

                for (int i = 0; i < cardIds.Length - 1; i++)
                {
                    if (Int32.Parse(names[cardIds[i]].Split()[1]) + 1 != Int32.Parse(names[cardIds[i + 1]].Split()[1]))
                    {
                        isStraight = false;
                        cnt++;
                    }
                    if (names[cardIds[i]].Split()[0] != names[cardIds[i + 1]].Split()[0])
                    {
                        isStraightFlush = false;
                    }
                    if (cardIds[i] == 54)
                        isPhoenix = true;
                }
                if (isStraightFlush)
                {
                    return Rank.StraightFlush;
                }
                if (isStraight || (isPhoenix && cnt <= 2))
                {
                    return Rank.Straight;
                }
                if (cardIds.Length % 2 != 0)
                    return Rank.Empty;
                cnt = 0;
                for (int i = 0; i <cardIds.Length-2; i += 2)
                {
                    if (Int32.Parse(names[cardIds[i]].Split()[1]) == Int32.Parse(names[cardIds[i+1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[i+1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i+2]].Split()[1]) &&
                    Int32.Parse(names[cardIds[i+2]].Split()[1]) == Int32.Parse(names[cardIds[i+3]].Split()[1]))
                    {
                        continue;
                    }
                    isContinuousPair = false;
                    cnt += 1;
                    if (isPhoenix)
                    {
                        if (Int32.Parse(names[cardIds[i]].Split()[1]) == Int32.Parse(names[cardIds[i + 1]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i + 2]].Split()[1]) == Int32.Parse(names[cardIds[i + 3]].Split()[1]))
                        {
                            if (Int32.Parse(names[cardIds[i]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 3]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i+1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i+1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 3]].Split()[1]))
                            {
                                continue;
                            }
                        }
                    }
                    break;
                }
                if (isContinuousPair || (isPhoenix && cnt <= 2))
                {
                    return Rank.ContinuousPair;
                }
                return Rank.Empty;
        }
        return Rank.Empty;
    }
}
