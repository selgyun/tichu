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
        int[] bird = { 52 };
        gmr.Bet(bird, GameManager.Rank.Bird);
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
                // ������ �Ծ�� �ϴ� ���
                if (this.transform.GetChild(0).transform.GetComponent<Text>().text != "Conform")
                {
                    onGetScores.Invoke();
                }
                else
                {
                    bool isMakeWish = false;
                    //������ �ҿ� ó��
                    if (GameManager.BIRDWISH != 0)
                    {
                        if (gmr.checkBirdWish(GameManager.BIRDWISH, gmr.GetCurHandRankingLength()))
                        {
                            isMakeWish = true;
                        }
                    }

                    int[] tempCards = new int[14];
                    int selectNum = 0;
                    int power = 0;
                    for (int i = 0; i < GameManager.hand.Length; i++)
                    {
                        if (GameManager.hand[i].isSelected)
                        {
                            tempCards[selectNum] = GameManager.hand[i].id;
                            selectNum++;
                            power = Int32.Parse(GameManager.hand[i].name.Split()[1]);
                            Debug.Log("selected id: " + GameManager.hand[i].id);
                        }
                    }
                    if (selectNum == 0)
                        return;
                    int[] selectCards = new int[selectNum];
                    for(int i = 0;i < selectNum; i++)
                    {
                        selectCards[i] = tempCards[i];
                    }
                    GameManager.Rank rank = gmr.judgeRank(selectCards);

                    if (rank == GameManager.Rank.FourOfaKind && rank == GameManager.Rank.StraightFlush)
                    {
                        //��ź�� �� ó��
                        if (GameManager.curRank == GameManager.Rank.FourOfaKind || GameManager.curRank == GameManager.Rank.StraightFlush)
                        {
                            if (gmr.GetCurHandRankingLength() < selectNum)
                                gmr.Bet(selectCards, rank);
                            else if (gmr.GetCurHandRankingLength() == selectNum)
                            {
                                if (GameManager.curRankPower < power)
                                    gmr.Bet(selectCards, rank);
                                else
                                    return;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            gmr.Bet(selectCards, rank);
                        }
                    }
                    // ���õ� �а� ���ų�, ������ �ٸ��� ���� �����ִ� �а� ���� ��. ����
                    else if (rank == GameManager.Rank.Empty || (GameManager.curRank != rank && GameManager.curRank != GameManager.Rank.Empty))
                    {
                        // �� �а� �̱��̰� ���� �а� �����ų� ��Ȳ�϶��� ����
                        if (!(rank == GameManager.Rank.Single && (GameManager.curRank == GameManager.Rank.Bird || GameManager.curRank == GameManager.Rank.Phoenix)))
                            return;
                    }
                    // �� �п� ���� �����ִ� ���� ���̰� �ٸ� ��
                    if (rank == GameManager.curRank && selectNum != gmr.GetCurHandRankingLength())
                    {
                        return;
                    }
                    // ������ ������ ��ġ�� �и� ��, �巡�� �ƴҶ�
                    if (power < GameManager.curRankPower && rank != GameManager.Rank.Dragon)
                        return;
                    // �� �а� ������
                    if (rank == GameManager.Rank.Bird)
                    {
                        BirdWishPanel.SetActive(true);
                    }
                    // ������ ���� ��ġ�� �ȹи�
                    else
                    {
                        // ���� ������ �ҿ��� ������ �� ��
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
                                gmr.Bet(selectCards, rank);
                            }
                            else
                                return;
                        }
                        else
                        {
                            gmr.Bet(selectCards, rank);
                        }
                    }
                }
            }
        }
    }
}
