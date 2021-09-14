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
                    for(int i = 0;i < selectNum; i++)
                    {
                        selectCards[i] = tempCards[i];
                    }
                    // rank �� power ���
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

                    if (rank == GameManager.Rank.FourOfaKind && rank == GameManager.Rank.StraightFlush)
                    {
                        //��ź�� �� ó��
                        if (GameManager.curRank == GameManager.Rank.FourOfaKind || GameManager.curRank == GameManager.Rank.StraightFlush)
                        {
                            if (gmr.GetCurHandRankingLength() < selectNum)
                                gmr.Bet(selectCards, rank, power);
                            else if (gmr.GetCurHandRankingLength() == selectNum)
                            {
                                if (GameManager.curRankPower < power)
                                    gmr.Bet(selectCards, rank, power);
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
                            gmr.Bet(selectCards, rank, power);
                        }
                    }
                    if (rank == GameManager.Rank.Phoenix && (GameManager.curRank == GameManager.Rank.Single || GameManager.curRank == GameManager.Rank.Bird))
                    {
                        // �� �а� ��Ȳ�̰�, ���� �� �̱� �Ǵ� �� PASS
                    }
                    else if (rank == GameManager.Rank.Single && (GameManager.curRank == GameManager.Rank.Phoenix || GameManager.curRank == GameManager.Rank.Bird))
                    {
                        // �� �а� �̱��̰�, ���� �� ��Ȳ �Ǵ� �� PASS
                    }
                    else if (rank == GameManager.Rank.Dragon && ((GameManager.curRank == GameManager.Rank.Phoenix || GameManager.curRank == GameManager.Rank.Bird) || GameManager.curRank == GameManager.Rank.Single))
                    {
                        // �� �а� ���̰�, ���� �� �̱� �Ǵ� �� �Ǵ� ��Ȳ
                    }
                    // ���õ� �а� ���ų�, ������ �ٸ��� ���� �����ִ� �а� ���� ��. ����
                    else if (rank == GameManager.Rank.Empty || (GameManager.curRank != rank && GameManager.curRank != GameManager.Rank.Empty))
                    {
                        return;
                    }
                    // �� �п� ���� �����ִ� ���� ������ ������ ���̰� �ٸ� ��
                    if (rank == GameManager.curRank && selectNum != gmr.GetCurHandRankingLength())
                    {
                        return;
                    }
                    // ����, ���̰� ������ ��ġ�� �и� ��, ��Ȳ �ƴҶ�
                    if (power <= GameManager.curRankPower && rank != GameManager.Rank.Phoenix)
                        return;
                    // �� �а� ������
                    if (isBird)
                    {
                        BirdWishPanel.SetActive(true);
                        gmr.Bet(selectCards, rank, power);
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
                                gmr.Bet(selectCards, rank, power);
                            }
                            else
                                return;
                        }
                        else
                        {
                            gmr.Bet(selectCards, rank, power);
                        }
                    }
                }
            }
        }
    }
}
