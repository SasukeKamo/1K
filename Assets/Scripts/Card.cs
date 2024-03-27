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
