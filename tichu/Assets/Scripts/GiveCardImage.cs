using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GiveCardImage : MonoBehaviour, IPointerClickHandler
{
    public GameManager gm;
    public int id;
    public static Card[] giveCard = new Card[3];
    public Sprite empty;
    Sprite panel;
    public static int[] cardPos = new int[3];

    void Awake()
    {
        giveCard[0] = null;
        giveCard[1] = null;
        giveCard[2] = null;
        panel = GetComponent<Image>().sprite;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (GameManager.isChangeCard[GameManager.pos])
                return;
            Debug.Log(id);
            if (giveCard[id] != null)
            {
                GameManager.hand[cardPos[id]].sprite = giveCard[id].sprite;
                CardClick.interactable[cardPos[id]] = true;
                this.GetComponent<Image>().sprite = panel;
                giveCard[id] = null;
                gm.ViewHand();
            }

            for (int i = 0;i < GameManager.hand.Length; i++)
            {
                if (GameManager.hand[i].isSelected)
                {
                    GameManager.hand[i].isSelected = false;
                    GameObject.FindGameObjectWithTag("Hand").transform.GetChild(i).transform.position -= new Vector3(0, 0.5f, 0);
                    giveCard[id] = new Card(GameManager.hand[i].sprite, GameManager.hand[i].name, GameManager.hand[i].id, GameManager.hand[i].score);
                    cardPos[id] = i;
                    this.GetComponent<Image>().sprite = giveCard[id].sprite;
                    GameManager.hand[i].sprite = empty;
                    CardClick.interactable[i] = false;
                    gm.ViewHand();
                    break;
                }
            }

        }
    }
}
