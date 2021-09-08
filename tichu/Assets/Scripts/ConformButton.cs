using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ConformButton : MonoBehaviour
{
    public UnityEvent onCardSwap;
    public UnityEvent onCardSwapDone;

    public GameManager gmr;

    public static bool receiveCard = false;
    private void Awake()
    {
        receiveCard = false;
        this.transform.GetComponent<Image>().color = Color.green;
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

            }
        }
    }
}
