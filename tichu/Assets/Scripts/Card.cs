using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public Sprite sprite;
    public int id;
    public string name;
    public int score;

    public Card(Sprite sprite, string name,int id, int score)
    {
        this.sprite = sprite;
        this.id = id;
        this.name = name;
        this.score = score;
    }
}
