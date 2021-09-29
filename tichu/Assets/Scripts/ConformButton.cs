using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon;
using Photon.Pun;
using Photon.Realtime;


public class ConformButton : MonoBehaviour
{
    public UnityEvent onCardSwap;
    public UnityEvent onCardSwapDone;
    public UnityEvent onGetScores;

    public GameManager gmr;
    public GameObject BirdWishPanel;
    public Text BirdWishText;
    public Text InfoText;
    
    public static bool receiveCard = false;

    public PhotonView PV;
    private void Awake()
    {
        receiveCard = false;
        this.transform.GetComponent<Image>().color = Color.green;
    }

    public void BirdWishButton(int i)
    {
        PV.RPC("Wish", RpcTarget.All, i);
        BirdWishPanel.SetActive(false);
    }

    [PunRPC]
    void Wish(int i)
    {
        GameManager.BIRDWISH = i;
        if (i != 0)
        {
            BirdWishText.gameObject.SetActive(true);
            switch (i)
            {
                case 11:
                    BirdWishText.text = "Wish: J";
                    break;
                case 12:
                    BirdWishText.text = "Wish: Q";
                    break;
                case 13:
                    BirdWishText.text = "Wish: K";
                    break;
                case 14:
                    BirdWishText.text = "Wish: A";
                    break;
                default:
                    BirdWishText.text = "Wish: " + i;
                    break;
            }
        }
    }

    [PunRPC]
    void MakeWish()
    {
        BirdWishText.gameObject.SetActive(false);
        GameManager.BIRDWISH = 0;
    }

    public void WishCheck(bool isMakeWish, int[] selectCards, int selectNum, GameManager.Rank rank, int power)
    {
        if (isMakeWish)
        {
            bool isCanBet = false;
            for (int i = 0; i < selectNum; i++)
            {
                if (selectCards[i] % 13 == GameManager.BIRDWISH - 2)
                {
                    isCanBet = true;
                }
            }
            if (isCanBet)
            {
                PV.RPC("MakeWish", RpcTarget.All);
                gmr.Bet(selectCards, rank, power);
            }
            else
            {
                InfoText.text = "You have to grant the sparrow's wish!";
                return;
            }
        }
        else
        {
            gmr.Bet(selectCards, rank, power);
        }
    }

    public void Conform()
    {
        if (!GameManager.isChangeCard[GameManager.pos])
        {
            if ((GiveCardImage.giveCard[0] != null && GiveCardImage.giveCard[1] != null) && GiveCardImage.giveCard[2] != null)
            {
                onCardSwap.Invoke();
            }
            else
            {
                this.transform.GetComponent<Image>().color = Color.red;
                Invoke("Awake", 1.0f);
            }
        }
        else
        {
            if (!receiveCard)
            {
                receiveCard = true;
                for (int i = 0; i < 3; i++)
                {
                    GameManager.hand[GiveCardImage.cardPos[i]] = GiveCardImage.giveCard[i];
                }
                gmr.ViewHand();
                onCardSwapDone.Invoke();
            }
            else
            {
                // 점수를 먹어야 하는 경우
                if (this.transform.GetChild(0).transform.GetComponent<Text>().text != "Conform")
                {
                    onGetScores.Invoke();
                }
                else
                {
                    bool isMakeWish = false;
                    //참새의 소원 처리
                    if (GameManager.BIRDWISH != 0)
                    {
                        if (gmr.checkBirdWish(gmr.GetCurHandRankingLength()))
                        {
                            isMakeWish = true;
                        }
                    }

                    int[] tempCards = new int[14];
                    int selectNum = 0;
                    int power = 0;
                    bool isBird = false;
                    for (int i = 0; i < GameManager.hand.Length; i++)
                    {
                        if (GameManager.hand[i].isSelected)
                        {
                            tempCards[selectNum] = GameManager.hand[i].id;
                            selectNum++;
                            power = Int32.Parse(GameManager.hand[i].name.Split()[1]);
                            Debug.Log("selected id: " + GameManager.hand[i].id);
                            if (GameManager.hand[i].id == 52)
                                isBird = true;
                        }
                    }
                    if (selectNum == 0)
                        return;
                    int[] selectCards = new int[selectNum];
                    for (int i = 0; i < selectNum; i++)
                    {
                        selectCards[i] = tempCards[i];
                    }
                    // rank 와 power 계산
                    GameManager.Rank rank = gmr.judgeRank(selectCards);
                    if (rank == GameManager.Rank.Dragon)
                        power = 15;
                    else if (rank == GameManager.Rank.Phoenix)
                    {
                        if (GameManager.curRankPower == 0)
                            power = 1;
                        else
                            power = GameManager.curRankPower;
                    }
                    else if (rank == GameManager.Rank.FullHouse)
                    {
                        if (power != selectCards[2] % 13 + 2 && !(selectCards[0] == 54 && power != selectCards[3]))
                            power = selectCards[2] % 13 + 2;
                    }
                    if (rank == GameManager.Rank.FourOfaKind || rank == GameManager.Rank.StraightFlush)
                    {
                        //폭탄일 때 처리
                        if (GameManager.curRank == GameManager.Rank.FourOfaKind || GameManager.curRank == GameManager.Rank.StraightFlush)
                        {
                            if (gmr.GetCurHandRankingLength() < selectNum)
                            {
                                GameManager.pressBomb = false;
                                WishCheck(isMakeWish, selectCards, selectNum, rank, power);
                            }
                            else if (gmr.GetCurHandRankingLength() == selectNum)
                            {
                                if (GameManager.curRankPower < power)
                                {
                                    GameManager.pressBomb = false;
                                    WishCheck(isMakeWish, selectCards, selectNum, rank, power);
                                }
                                else
                                {
                                    InfoText.text = "The bomb's power is weaker than the current one.";
                                    return;
                                }
                            }
                            else
                            {
                                InfoText.text = "The bomb's power is weaker than the current one.";
                                return;
                            }
                        }
                        else
                        {
                            GameManager.pressBomb = false;
                            WishCheck(isMakeWish, selectCards, selectNum, rank, power);
                        }
                    }
                    if (GameManager.pressBomb)
                    {
                        InfoText.text = "You Have to play Bomb this turn";
                        return;
                    }
                    if (rank == GameManager.Rank.Phoenix && (GameManager.curRank == GameManager.Rank.Single || GameManager.curRank == GameManager.Rank.Bird))
                    {
                        // 내 패가 봉황이고, 현재 패 싱글 또는 새 PASS
                    }
                    else if (rank == GameManager.Rank.Single && (GameManager.curRank == GameManager.Rank.Phoenix || GameManager.curRank == GameManager.Rank.Bird))
                    {
                        // 내 패가 싱글이고, 현재 패 봉황 또는 새 PASS
                    }
                    else if (rank == GameManager.Rank.Dragon && ((GameManager.curRank == GameManager.Rank.Phoenix || GameManager.curRank == GameManager.Rank.Bird) || GameManager.curRank == GameManager.Rank.Single))
                    {
                        // 내 패가 용이고, 현재 패 싱글 또는 새 또는 봉황
                    }
                    // 선택된 패가 없거나, 족보가 다르고 현재 나와있는 패가 있을 때. 제외
                    else if (rank == GameManager.Rank.Empty || (GameManager.curRank != rank && GameManager.curRank != GameManager.Rank.Empty))
                    {
                        InfoText.text = "The hand is empty or different!";
                        return;
                    }
                    // 내 패와 현재 나와있는 패의 족보는 같으나 길이가 다를 때
                    if (rank == GameManager.curRank && selectNum != gmr.GetCurHandRankingLength())
                    {
                        InfoText.text = "The length of rank is different!";
                        return;
                    }
                    // 족보, 길이가 같지만 수치가 밀릴 때, 봉황 아닐때
                    if (power <= GameManager.curRankPower && rank != GameManager.Rank.Phoenix)
                    {
                        InfoText.text = "weak power!";
                        return;
                    }
                    // 내 패가 참새임
                    if (isBird)
                    {
                        BirdWishPanel.SetActive(true);
                        gmr.Bet(selectCards, rank, power);
                    }
                    // 족보도 같고 수치도 안밀림
                    else
                    {
                        WishCheck(isMakeWish, selectCards, selectNum, rank, power);
                    }
                }
            }
        }
    }
}
