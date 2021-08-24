using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public Sprite sprite;
    public string name;
    public Card(Sprite sprite, string name)
    {
        this.sprite = sprite;
        this.name = name;
    }
}
