using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public Sprite sprite;
    public int id;
    public string name;
    public int score;
    public bool isSelected { get; set; }
    public int cardpos { get; set; }

    public Card(Sprite sprite, string name,int id, int score)
    {
        this.sprite = sprite;
        this.id = id;
        this.name = name;
        this.score = score;
        isSelected = false;
    }
}
