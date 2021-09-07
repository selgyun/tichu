using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardClick : MonoBehaviour, IPointerClickHandler
{
    public static bool[] interactable = {true, true, true, true, true, true, true, true, true, true, true, true, true, true};
    public int id;

    private void Awake()
    {
        for(int i = 0; i < 14; i++)
        {
            interactable[i] = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            CardClickEvent();
        }
    }

    public void CardClickEvent()
    {
        Debug.Log(id);
        if (!interactable[id])
            return;
        if (!GameManager.gameStart[GameManager.pos])
            return;

        GameManager.hand[id].isSelected = !GameManager.hand[id].isSelected;

        if (!GameManager.isChangeCard[GameManager.pos])
        {
            for (int i = 0; i < GameManager.hand.Length; i++)
            {
                if (i == id)
                    continue;
                if (GameManager.hand[i].isSelected)
                {
                    GameManager.hand[i].isSelected = false;
                    GameObject.FindGameObjectWithTag("Hand").transform.GetChild(i).transform.position -= new Vector3(0, 0.5f, 0);
                }
            }
        }

        if (GameManager.hand[id].isSelected)
        {
            this.transform.position = this.transform.position + new Vector3(0, 0.5f, 0);
        }
        else
        {
            this.transform.position = this.transform.position + new Vector3(0, -0.5f, 0);
        }
    }

}
