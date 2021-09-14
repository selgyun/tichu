using System.Collections;
using System;
using System.Linq;
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
    Card emptyCard;

    [Header("UI")]
    public Image[] HandImage;
    public Image[] HandRankingView;
    public GameObject GiveCardPanel;
    public GameObject InGameBtnBar;
    public GameObject GTBar;
    public GameObject GameOverPanel;

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
    public Text BirdWishText;
    public Text InfoText;


    private int[] deck = new int[56];
    public static Card[] hand = new Card[14];
    public static int[] CardSwapPack = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public static int pos = 0;
    public static bool[] gameStart = new bool[4]; // 처음 게임 시작 판단
    public static bool isGreatTichu;
    public static bool isSmallTichu;
    public static bool[] isChangeCard = new bool[4]; // 카드 교환 여부 판단
    public static int[] gameFinished = { 0, 0, 0, 0 }; // 게임 끝냈는지
    public static bool[] nextGameReady = { false, false, false, false }; // 다음 겜 준비
    public static int[] conbineScore = { -1, -1, -1, -1};
    public static int playerRanking = 1;
    public static int[] actions = { 0, 0, 0, 0 }; // pass했는지 카드 냈는지 확인
    public static int curTurn = -1;
    public static Rank curRank = Rank.Empty;
    public static int curRankPower = -1;
    public static int BIRDWISH = 0;
    public enum Rank { Empty, Single, Pair, ContinuousPair, Triple, Straight, FullHouse, FourOfaKind, StraightFlush, Bird, Dragon, Phoenix, Dog }

    Color blueTeamColor = new Color(203/255f, 225/255f, 255/255f);
    Color redTeamColor = new Color(255/255f, 184/255f, 184/255f);
    Color turnColor = new Color(131/255f, 255/255f, 112/255f);
    Color FirstRankColor = new Color(255/255f, 255/255f, 96/255f);
    Color FinishedColor = new Color(154/255f, 161/255f, 171/255f);

    public PhotonView PV;

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PhotonNetwork.LoadLevel("Room");
    }

    void Awake()
    {
        emptyCard = new Card(empty, "null", -1, 0);
        getPos();
        for (int i = 0; i < 56; i++)
        {
            if (i == 53)
                scores[i] = 25;
            else if (i == 54)
                scores[i] = -25;
            else if (i % 13 == 3 && i != 55)
                scores[i] = 5;
            else if (i % 13 == 8 || i % 13 == 11)
                scores[i] = 10;
            else
                scores[i] = 0;
        }
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            PlayerBtn[i].transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
        newGame();
    }
    [PunRPC]
    void newGame()
    {
        Init();
        Mulligan();
    }
    void Init()
    {
        hand = new Card[14];
        BIRDWISH = 0;
        curTurn = -1;
        curRank = Rank.Empty;
        curRankPower = -1;
        BirdWishText.text = "Wish: ";
        InfoText.text = "Welcome to Tichu";
        BirdWishText.gameObject.SetActive(false);
        SmallTichuBtn.gameObject.SetActive(true);
        HandRankingText.text = "Empty";
        PassBtn.interactable = false;
        BombBtn.interactable = false;
        GTBar.SetActive(true);
        InGameBtnBar.SetActive(false);
        GiveCardPanel.SetActive(false);
        GameOverPanel.SetActive(false);
        for (int i = 0; i < 4; i++)
        {
            actions[i] = 0;
            isChangeCard[i] = false;
            gameStart[i] = false;
            gameFinished[i] = 0;
            conbineScore[i] = -1;
            nextGameReady[i] = false;
            playerRanking = 1;
            PlayerBtn[i].transform.GetChild(1).transform.GetComponent<Text>().text = "14";
            PlayerBtn[i].transform.GetChild(2).transform.GetComponent<Text>().text = "0";
            PlayerBtn[i].transform.GetChild(3).gameObject.SetActive(false);
            PlayerBtn[i].transform.GetChild(4).gameObject.SetActive(false);
            PlayerBtn[i].transform.GetChild(5).gameObject.SetActive(true);
            PlayerBtn[i].interactable = false;
            if (i % 2 == 0)
                PlayerBtn[i].GetComponent<Image>().color = blueTeamColor;
            else
                PlayerBtn[i].GetComponent<Image>().color = redTeamColor;
        }

        for (int i = 0;i < hand.Length; i++)
        {
            HandImage[i].transform.position = new Vector3(HandImage[i].transform.position.x, HandImage[i].transform.position.y, -(i % 7));
        }

        isGreatTichu = false;
        isSmallTichu = false;
        InitHandRankingView();
    }

    void InitHandRankingView()
    {
        for (int i = 0; i < 14; i++)
            HandRankingView[i].sprite = empty;
    }
    void InitDeck()
    {
        for (int i = 0; i < 56; i++)
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
        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            int tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    public int GetCurHandRankingLength()
    {
        int ans = 0;
        for (int i = 0;i < HandRankingView.Length; i++)
        {
            if (HandRankingView[i].sprite != empty)
                ans += 1;
        }
        return ans;
    }
    void getPos()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
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
        for (int i = 0; i < 14; i++)
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
            Array.Sort(temp, delegate (Card a, Card b)
            {
                return Int32.Parse(a.name.Split()[1]) - Int32.Parse(b.name.Split()[1]);
            });
            for (int i = 0; i < 8; i++)
                HandImage[i].sprite = temp[i].sprite;
            for (int i = 8; i < 14; i++)
                HandImage[i].sprite = empty;

            return;
        }
        Array.Sort(hand, delegate (Card a, Card b)
        {
            return Int32.Parse(a.name.Split()[1]) - Int32.Parse(b.name.Split()[1]);
        });
        for (int i = 0; i < hand.Length; i++)
        {
            HandImage[i].sprite = hand[i].sprite;
            hand[i].cardpos = i;
        }
        for (int i = hand.Length; i < 14; i++)
        {
            HandImage[i].sprite = empty;
            HandImage[i].transform.position = new Vector3(HandImage[i].transform.position.x, HandImage[i].transform.position.y, 10);
            CardClick.interactable[i] = false;
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
        if ((gameStart[0] && gameStart[1]) && (gameStart[2] && gameStart[3]))
        {
            PV.RPC("GameStart", RpcTarget.All);
        }
    }
    [PunRPC]
    void GameStart()
    {
        for (int i = 0; i < 4; i++)
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
        GiveCardPanel.transform.GetChild(1).GetComponent<Text>().text = "Waiting for other players..";
        PV.RPC("SetWaitingImage", RpcTarget.All, pos, false);
        PV.RPC("SendCardSwap", RpcTarget.MasterClient, GiveCardImage.giveCard[0].id, GiveCardImage.giveCard[1].id, GiveCardImage.giveCard[2].id, pos);
        isChangeCard[pos] = true;
        for (int i = 0; i < 14; i++)
            CardClick.interactable[i] = false;
    }

    public void CardSwapDone()
    {
        ConformBtn.interactable = false;
        GiveCardPanel.transform.GetChild(1).GetComponent<Text>().text = "Place a card by touching!";
        for (int i = 0; i < 3; i++)
        {
            GameObject.FindGameObjectWithTag("CardSwapImage").transform.GetChild(i).GetComponent<Image>().sprite = panel;
            GiveCardImage.giveCard[i] = null;
        }
        GiveCardPanel.gameObject.SetActive(false);
        PV.RPC("SetWaitingImage", RpcTarget.All, pos, false);
        for (int i = 0;i < 4;i++)
        {
            if (PlayerBtn[i].transform.GetChild(5).gameObject.activeSelf)
                return;
        }
        PV.RPC("FindBird", RpcTarget.All);
    }

    [PunRPC]
    public void SendCardSwap(int a, int b, int c, int p)
    {
        isChangeCard[p] = true;
        CardSwapPack[p * 3] = a;
        CardSwapPack[p * 3 + 1] = b;
        CardSwapPack[p * 3 + 2] = c;
        if ((isChangeCard[0] && isChangeCard[1]) && (isChangeCard[2] && isChangeCard[3]))
        {
            PV.RPC("ReceiveCardSwap", RpcTarget.All, CardSwapPack);
        }

    }
    [PunRPC]
    public void ReceiveCardSwap(int[] swapPack)
    {
        PV.RPC("SetWaitingImage", RpcTarget.All, pos, true);
        GiveCardPanel.transform.GetChild(1).GetComponent<Text>().text = "Check your cards";
        switch (pos)
        {
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

    public void PassButton()
    {
        if (curTurn == pos)
        {
            PV.RPC("ActivatePassImage", RpcTarget.All, pos);
            PV.RPC("NextTurn", RpcTarget.All, 0);
        }
    }
    [PunRPC]
    public void ActivatePassImage(int p)
    {
        PlayerBtn[p].transform.GetChild(6).gameObject.SetActive(true);
        PlayerBtn[p].transform.GetChild(6).transform.GetComponent<Animator>().SetTrigger("Show");
    }

    [PunRPC]
    public void Turn()
    {
        Debug.Log(curRank.ToString());

        if ((gameFinished[0] != 0 && gameFinished[1] != 0) && (gameFinished[2] != 0 && gameFinished[3] != 0))
        {
            PV.RPC("SendScore", RpcTarget.MasterClient, pos, isSmallTichu, isGreatTichu);
            return;
        }

        // 턴 플레이어 녹색 표기
        for (int i = 0; i < 4; i++)
        {
            if (i == curTurn && gameFinished[i] == 0)
                PlayerBtn[i].GetComponent<Image>().color = turnColor;
            else if (i % 2 == 0 && gameFinished[i] == 0)
                PlayerBtn[i].GetComponent<Image>().color = blueTeamColor;
            else if (i % 2 == 1 && gameFinished[i] == 0)
                PlayerBtn[i].GetComponent<Image>().color = redTeamColor;
        }

        InfoText.text = PhotonNetwork.PlayerList[curTurn].NickName + "'s Turn!";

        if (curTurn != pos)
        {
            if(ConformButton.receiveCard)
                ConformBtn.interactable = false;
            PassBtn.interactable = false;
            return;
        }

        InfoText.text = "Your Turn!";

        ConformBtn.interactable = true;
        // 첫 시작일 경우
        if ((actions[0] == 0 && actions[1] == 0) && (actions[2] == 0 && actions[3] == 0))
        {
            HandRankingText.text = "Empty";
            CurScoreText.text = "Score: 0";
            PassBtn.interactable = false;
        }
        else
            PassBtn.interactable = true;

        //참새의 소원 처리
        if (BIRDWISH != 0)
        {
            if (checkBirdWish(BIRDWISH, GetCurHandRankingLength()))
            {
                PassBtn.interactable = false;
            }
        }

        // 내가 점수를 먹어야 되는 경우
        if ((actions[pos] == 1 && actions[(pos + 1) % 4] == 0) && (actions[(pos + 2) % 4] == 0 && actions[(pos + 3) % 4] == 0))
        {
            PassBtn.interactable = false;
            ConformBtn.transform.GetChild(0).GetComponent<Text>().text = "Get Scores";
        }
        else
        {
            ConformBtn.transform.GetChild(0).GetComponent<Text>().text = "Conform";
        }

        if (hand.Length == 0 && ConformBtn.transform.GetChild(0).GetComponent<Text>().text == "Conform")
            PV.RPC("NextTurn", RpcTarget.All, 0);
    }
    [PunRPC]
    public void FindBird()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i].id == 52)
            {
                PV.RPC("SetBird", RpcTarget.All, pos);
                break;
            }
        }
    }

    [PunRPC]
    public void NextTurn(int act)
    {
        actions[curTurn] = act;
        do
            curTurn = (curTurn + 1) % 4;
        while (gameFinished[curTurn] != 0);
        PV.RPC("Turn", RpcTarget.All);
    }

    [PunRPC]
    void SendScore(int p, bool smallT, bool greatT)
    {
        conbineScore[p] = 0;
        if (smallT)
        {
            if (gameFinished[p] == 1)
                conbineScore[p] = 100;
            else
                conbineScore[p] = -100;
        }
        if (greatT)
        {
            if (gameFinished[p] == 1)
                conbineScore[p] = 200;
            else
                conbineScore[p] = -200;
        }
        if ((conbineScore[0] != 0 && conbineScore[1] != 0) && (conbineScore[2] != 0 && conbineScore[3] != 0))
        {
            PV.RPC("GameOver", RpcTarget.All, conbineScore);
        }
    }
    [PunRPC]
    void GameOver(int[] scoreBoard)
    {
        PV.RPC("SetWaitingImage", RpcTarget.All, pos, true);
        int smallTichuBlue = 0;
        int smallTichuRed = 0;
        int greatTichuBlue = 0;
        int greatTichuRed = 0;
        int total_blue = 0;
        int total_red = 0;
        GameOverPanel.SetActive(true);
        for (int i = 0; i < 4; i++)
        {
            GameOverPanel.transform.GetChild(i + 2).GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName + ": " + Int32.Parse(PlayerBtn[i].transform.GetChild(2).GetComponent<Text>().text);
            if (i % 2 == 0)
            {
                total_blue += Int32.Parse(PlayerBtn[i].transform.GetChild(2).GetComponent<Text>().text);
                if (scoreBoard[i] == 100 || scoreBoard[i] == -100)
                    smallTichuBlue += scoreBoard[i];
                else if (scoreBoard[i] == 200 || scoreBoard[i] == -200)
                    greatTichuBlue += scoreBoard[i];
            }
            else
            {
                total_red += Int32.Parse(PlayerBtn[i].transform.GetChild(2).GetComponent<Text>().text);
                if (scoreBoard[i] == 100 || scoreBoard[i] == -100)
                    smallTichuRed += scoreBoard[i];
                else if (scoreBoard[i] == 200 || scoreBoard[i] == -200)
                    greatTichuRed += scoreBoard[i];
            }
        }

        total_blue += smallTichuBlue + greatTichuBlue;
        total_red += smallTichuRed + greatTichuRed;

        if (smallTichuBlue != 0)
            GameOverPanel.transform.GetChild(6).GetComponent<Text>().text = "Small Tichu: " + smallTichuBlue;
        else
            GameOverPanel.transform.GetChild(6).GetComponent<Text>().text = "";
        if (smallTichuRed != 0)
            GameOverPanel.transform.GetChild(7).GetComponent<Text>().text = "Small Tichu: " + smallTichuRed;
        else
            GameOverPanel.transform.GetChild(7).GetComponent<Text>().text = "";
        if (greatTichuBlue != 0)
            GameOverPanel.transform.GetChild(8).GetComponent<Text>().text = "Great Tichu: " + greatTichuBlue;
        else
            GameOverPanel.transform.GetChild(8).GetComponent<Text>().text = "";
        if (greatTichuRed != 0)
            GameOverPanel.transform.GetChild(9).GetComponent<Text>().text = "Great Tichu: " + greatTichuRed;
        else
            GameOverPanel.transform.GetChild(9).GetComponent<Text>().text = "";

        GameOverPanel.transform.GetChild(10).GetComponent<Text>().text = total_blue.ToString();
        GameOverPanel.transform.GetChild(11).GetComponent<Text>().text = total_red.ToString();
        // 팀 점수 업데이트
        TeamBlueText.text = (Int32.Parse(TeamBlueText.text) + total_blue).ToString();
        TeamRedText.text = (Int32.Parse(TeamRedText.text) + total_red).ToString();
    }

    public void GameOverButton()
    {
        PV.RPC("SetWaitingImage", RpcTarget.All, pos, false);
        PV.RPC("SendGameOverConform", RpcTarget.MasterClient, pos);
    }
    [PunRPC]
    void SendGameOverConform(int p)
    {
        nextGameReady[p] = true;
        if ((nextGameReady[0] && nextGameReady[1]) && (nextGameReady[0] && nextGameReady[1]))
            PV.RPC("newGame", RpcTarget.All);
    }

    [PunRPC]
    public void SetBird(int cur)
    {
        curTurn = cur;
        for(int i = 0;i < 14; i++)
        {
            CardClick.interactable[i] = true;
            hand[i].isSelected = false;
        }
        HandRankingText.text = "Empty";
        CurScoreText.text = "Score: 0";
        PV.RPC("Turn", RpcTarget.All);
    }
    
    [PunRPC]
    void SetWaitingImage(int p, bool flag)
    {
        PlayerBtn[p].transform.GetChild(5).gameObject.SetActive(flag);
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
            case 3:
                if (cardIds[0] == 54)
                {
                    if (Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]))
                    {
                        return Rank.Triple;
                    }
                }
                if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]))
                {
                    return Rank.Triple;
                }
                return Rank.Empty;
            case 4:
                if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]) &&
                    Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]))
                {
                    return Rank.FourOfaKind;
                }
                if (cardIds[0] == 54)
                {
                    if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) ||
                        Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]) ||
                        Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]))
                    {
                        if (Int32.Parse(names[cardIds[0]].Split()[1]) + 1 == Int32.Parse(names[cardIds[2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[0]].Split()[1]) + 1 == Int32.Parse(names[cardIds[3]].Split()[1]) ||
                            Int32.Parse(names[cardIds[1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[3]].Split()[1]))
                        {
                            return Rank.ContinuousPair;
                        }
                    }
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
                if (isStraightFlush && isStraight) 
                {
                    return Rank.StraightFlush;
                }
                if (isStraight || (isPhoenix && cnt <= 2))
                {
                    return Rank.Straight;
                }
                // 풀 하우스
                if(cardIds.Length == 5)
                {
                    if (isPhoenix)
                    {
                        // 봉 22 33
                        if (Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]) &&
                            Int32.Parse(names[cardIds[3]].Split()[1]) == Int32.Parse(names[cardIds[4]].Split()[1]))
                        {
                            return Rank.FullHouse;
                        }
                        // 봉 2 333
                        if (Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]) &&
                            Int32.Parse(names[cardIds[3]].Split()[1]) == Int32.Parse(names[cardIds[4]].Split()[1]))
                        {
                            return Rank.FullHouse;
                        }
                        // 봉 222 3
                        if (Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[2]].Split()[1]) &&
                            Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]))
                        {
                            return Rank.FullHouse;
                        }
                    }
                    else
                    {
                        // 22 444
                        if (Int32.Parse(names[cardIds[0]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1]) &&
                            (Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[3]].Split()[1]) &&
                            Int32.Parse(names[cardIds[3]].Split()[1]) == Int32.Parse(names[cardIds[4]].Split()[1])))
                        {
                            return Rank.FullHouse;
                        }
                        // 222 44
                        if (Int32.Parse(names[cardIds[3]].Split()[1]) == Int32.Parse(names[cardIds[4]].Split()[1]) &&
                            (Int32.Parse(names[cardIds[1]].Split()[1]) == Int32.Parse(names[cardIds[0]].Split()[1]) &&
                            Int32.Parse(names[cardIds[2]].Split()[1]) == Int32.Parse(names[cardIds[1]].Split()[1])))
                        {
                            return Rank.FullHouse;
                        }
                    }
                }
                if (cardIds.Length % 2 != 0)
                    return Rank.Empty;
                cnt = 0;
                for (int i = 0; i < cardIds.Length - 2; i += 2)
                {
                    if (Int32.Parse(names[cardIds[i]].Split()[1]) == Int32.Parse(names[cardIds[i + 1]].Split()[1]) &&
                    Int32.Parse(names[cardIds[i + 1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 2]].Split()[1]) &&
                    Int32.Parse(names[cardIds[i + 2]].Split()[1]) == Int32.Parse(names[cardIds[i + 3]].Split()[1]))
                    {
                        continue;
                    }
                    isContinuousPair = false;
                    cnt += 1;
                    if (isPhoenix)
                    {
                        if (Int32.Parse(names[cardIds[i]].Split()[1]) == Int32.Parse(names[cardIds[i + 1]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i + 1]].Split()[1]) == Int32.Parse(names[cardIds[i + 2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i + 2]].Split()[1]) == Int32.Parse(names[cardIds[i + 3]].Split()[1]))
                        {
                            if (Int32.Parse(names[cardIds[i]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 3]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i + 1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 2]].Split()[1]) ||
                            Int32.Parse(names[cardIds[i + 1]].Split()[1]) + 1 == Int32.Parse(names[cardIds[i + 3]].Split()[1]))
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
    }

    public bool checkBirdWish(int card_num, int len)
    {
        // 참새를 내는 경우의 수 -> 1. 싱글, 2. 스트레이트
        bool isPhoenix = false;
        bool isCardExist = false;
        int targetPos = -1;
        bool[] checkStraight = { false, false, false, false, false, false, false, false, false, false, false, false, false };
        for (int i = 0;i < hand.Length; i++)
        {
            if (hand[i].id == 54)
                isPhoenix = true;
            if (hand[i].id % 13 == card_num - 2)
            {
                isCardExist = true;
                targetPos = hand[i].cardpos;
                break;
            }
        }
        if (!isCardExist)
            return false;
        else
        {
            switch (len)
            {
                case 1:
                    if (curRankPower < card_num)
                        return true;
                    else
                        return false;
                default:
                    for(int i = 0;i < hand.Length; i++)
                    {
                        if(2 <= Int32.Parse(hand[i].name.Split()[1]) && Int32.Parse(hand[i].name.Split()[1]) < 15)
                        {
                            checkStraight[Int32.Parse(hand[i].name.Split()[1])] = true;
                        }
                    }
                    int l = card_num;
                    int r = card_num;
                    int maxPower = 0;
                    for(int i = r; i < 15; i++)
                    {
                        if (checkStraight[i])
                        {
                            r += 1;
                            maxPower = Int32.Parse(hand[i].name.Split()[1]);
                        }
                        else
                            break;
                    }
                    for(int i = l; i > 1; i--)
                    {
                        if (checkStraight[i])
                            l -= 1;
                        else
                            break;
                    }
                    if (r - l - 1 >= len && maxPower > curRankPower)
                        return true;
                    else
                    {
                        if (isPhoenix)
                        {
                            int cnt = 0;
                            int return_r = r;
                            for (int i = r; i < 15; i++)
                            {
                                if (checkStraight[i])
                                {
                                    r += 1;
                                    maxPower = Int32.Parse(hand[i].name.Split()[1]);
                                }
                                else
                                {
                                    if (cnt == 0)
                                    {
                                        r += 1;
                                        maxPower += 1;
                                        cnt += 1;
                                    }
                                    else
                                        break;
                                }
                            }
                            if (r - l - 1 >= len && maxPower > curRankPower)
                                return true;
                            r = return_r;
                            cnt = 0;
                            for (int i = l; i > 1; i--)
                            {
                                if (checkStraight[i])
                                    l -= 1;
                                else
                                {
                                    if (cnt == 0)
                                    {
                                        l -= 1;
                                        cnt += 1;
                                    }
                                    else
                                        break;
                                }
                            }
                            if (r - l - 1 >= len && maxPower > curRankPower)
                                return true;
                        }
                        return false;
                    }
            }
        }
    }

    [PunRPC]
    void DogTurn()
    {
        for (int i = 0; i < 4; i++)
            actions[i] = 0;
        curTurn = (curTurn + 2) % 4;
        actions[curTurn] = 1;
        if (gameFinished[curTurn] == 0)
            PV.RPC("Turn", RpcTarget.All);
        else
            PV.RPC("NextTurn", RpcTarget.All, 0);
    }
    public void Bet(int[] betCards, Rank rank, int power)
    {
        SmallTichuBtn.gameObject.SetActive(false);
        List<Card> tmp = new List<Card>();
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i].isSelected)
            {
                hand[i].isSelected = false;
                GameObject.FindGameObjectWithTag("Hand").transform.GetChild(i).position -= new Vector3(0, 0.5f, 0);
            }
            tmp.Add(hand[i]);
        }
        for (int i = 0;i < betCards.Length; i++)
        {
            for(int j = 0; j < hand.Length; j++)
            {
                if (tmp[j].id == betCards[i])
                    tmp[j] = emptyCard;
            }
        }
        while (tmp.Contains(emptyCard))
            tmp.Remove(emptyCard);
        hand = tmp.ToArray();
        Debug.Log("Hand num:" + hand.Length);
        if (hand.Length == 0)
        {
            PV.RPC("SetGameFinished", RpcTarget.All, pos);
        }
        PV.RPC("ShowBet", RpcTarget.All, betCards, rank, pos, hand.Length, power);
        ViewHand();
        if (rank != Rank.Dog)
            PV.RPC("NextTurn", RpcTarget.All, 1);
        else
            PV.RPC("DogTurn", RpcTarget.All);
    }

    [PunRPC]
    public void ShowBet(int[] betCards, Rank rank, int p, int l, int power)
    {
        //피닉스 처리 나중에!
        int jump = 0;
        int sc = 0;
        if (betCards.Length < 8)
            jump = 1;
        InitHandRankingView();
        curRankPower = power;
        for (int i = 0; i < betCards.Length; i++)
        {
            HandRankingView[i + (jump*i)].sprite = cardSprite[betCards[i]];
            sc += scores[betCards[i]];
        }
        PlayerBtn[p].transform.GetChild(1).GetComponent<Text>().text = l.ToString();
        curRank = rank;

        switch (curRankPower)
        {
            case 0:
                HandRankingText.text = rank.ToString();
                break;
            case 11:
                HandRankingText.text = "J " + rank.ToString();
                break;
            case 12:
                HandRankingText.text = "Q " + rank.ToString();
                break;
            case 13:
                HandRankingText.text = "K " + rank.ToString();
                break;
            case 14:
                HandRankingText.text = "A " + rank.ToString();
                break;
            case 15:
                HandRankingText.text = rank.ToString();
                break;
            default:
                HandRankingText.text = curRankPower.ToString() + " " + rank.ToString();
                break;
        }
        CurScoreText.text = "Score: " + (sc + Int32.Parse(CurScoreText.text.Split()[1])).ToString();
    }
    [PunRPC]
    void SetGameFinished(int p)
    {
        gameFinished[p] = playerRanking;
        playerRanking += 1;
        if (gameFinished[p] == 1)
            PlayerBtn[p].GetComponent<Image>().color = FirstRankColor;
        else
            PlayerBtn[p].GetComponent<Image>().color = FinishedColor;
        if (playerRanking == 4)
        {
            int first = 0;
            int fourth = 0;
            for (int i = 0; i <4;i++)
            {
                if (gameFinished[i] != 0) 
                {
                    gameFinished[i] = playerRanking;
                    fourth = i;
                }
                if (gameFinished[i] == 1)
                    first = i;
            }
            PlayerBtn[first].transform.GetChild(2).GetComponent<Text>().text = (Int32.Parse(PlayerBtn[first].transform.GetChild(2).GetComponent<Text>().text) + Int32.Parse(PlayerBtn[fourth].transform.GetChild(2).GetComponent<Text>().text)).ToString();
            PlayerBtn[fourth].transform.GetChild(2).GetComponent<Text>().text = "0";
        }
    }

    public void GetScores()
    {
        ConformBtn.transform.GetChild(0).transform.GetComponent<Text>().text = "Conform";
        PassBtn.interactable = false;
        int curSc = Int32.Parse(CurScoreText.text.Split()[1]) + Int32.Parse(PlayerBtn[pos].transform.GetChild(2).transform.GetComponent<Text>().text);
        if (curRank != Rank.Dragon)
            PV.RPC("PlayerSetScore", RpcTarget.All, pos, curSc);
        else
        {
            PV.RPC("SetWaitingImage", RpcTarget.All, pos, true);
            InfoText.text = "Choose who will give you points";
            PlayerBtn[(pos + 1) % 4].interactable = true;
            PlayerBtn[(pos + 3) % 4].interactable = true;
        }
        ViewHand();
        if (hand.Length == 0)
            PV.RPC("NextTurn", RpcTarget.All, 0);
    }

    public void RecieveDragonPoints(int i)
    {
        PV.RPC("PlayerSetScore", RpcTarget.All, i, Int32.Parse(CurScoreText.text.Split()[1]) + Int32.Parse(PlayerBtn[i].transform.GetChild(2).transform.GetComponent<Text>().text));
        PV.RPC("SetWaitingImage", RpcTarget.All, pos, false);
        PlayerBtn[(pos + 1) % 4].interactable = false;
        PlayerBtn[(pos + 3) % 4].interactable = false;
        InfoText.text = "Your Turn!";
    }

    [PunRPC]
    public void PlayerSetScore(int p, int sc)
    {
        for (int i = 0; i < 14; i++)
        {
            HandRankingView[i].sprite = empty;
        }
        curRank = Rank.Empty;
        curRankPower = -1;
        HandRankingText.text = "Empty";
        PlayerBtn[p].transform.GetChild(2).transform.GetComponent<Text>().text = sc.ToString();
        CurScoreText.text = "Score: 0";
        for (int i = 0; i < 4; i++)
        {
            if (PlayerBtn[i].transform.GetChild(6).gameObject.activeSelf)
            {
                PlayerBtn[i].transform.GetChild(6).transform.GetComponent<Image>().color = Color.white;
                PlayerBtn[i].transform.GetChild(6).gameObject.SetActive(false);
            }
        }
    }


}
