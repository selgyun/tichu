using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string[] names;
    public Sprite[] cardSprite;
    public Card[] deck;
    void Awake()
    {
        for(int i = 0;i < 56; i++)
        {
            deck[i] = new Card(cardSprite[i], names[i]);
        }
    }

    public void Mulligan()
    {

    }
}
