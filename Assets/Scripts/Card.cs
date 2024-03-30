using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades };
    public enum Rank { Nine, Ten, Jack, Queen, King, Ace };

    //mozecie tez dodac tutaj np image albo sprite i jakas wartosc np czyOdwrocona i zmieniac zdjecie jesli jest i jesli nie jest
    [SerializeField] private Sprite front;
    [SerializeField] private Sprite back;

    public bool visible;
    private SpriteRenderer spriteRenderer;


    [SerializeField] private Suit suit;
    [SerializeField] private Rank rank;
    private int value;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = front;
        visible = false;

        switch(rank)
        {
            case Rank.Nine:
                value = 0;
                break;
            case Rank.Ten:
                value = 10;
                break;
            case Rank.Jack:
                value = 2;
                break;
            case Rank.Queen:
                value = 3;
                break;
            case Rank.King:
                value = 4;
                break;
            case Rank.Ace:
                value = 11;
                break;
        }
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            this.visible = true;
            spriteRenderer.sprite = front;
        }
        else
        {
            this.visible = false;
            spriteRenderer.sprite = back;
        }
    }

    public string GetCardFullName() //mozna uzyc do debugu
    {
        string rankString = rank.ToString();
        string suitString = suit.ToString();
        return rankString + " of " + suitString;
    }

    public string GetSuit()
    {
        return suit.ToString();
    }

    public string GetRank()
    {
        return rank.ToString();
    }

    public int GetValue()
    {
        return value;
    }
}
