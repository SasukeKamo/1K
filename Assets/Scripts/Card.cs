using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades, None };
    public enum Rank { Nine, Ten, Jack, Queen, King, Ace };

    //mozecie tez dodac tutaj np image albo sprite i jakas wartosc np czyOdwrocona i zmieniac zdjecie jesli jest i jesli nie jest
    [SerializeField] private Sprite front;
    [SerializeField] private Sprite back;

    public bool visible;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    [SerializeField] private GameObject currentPlayerHand;
    [SerializeField] private GameObject auctionLeftOvers;


    [SerializeField] private Suit suit;
    [SerializeField] private Rank rank;
    private int value;

    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = front;
        visible = false;

        switch (rank)
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

    public string GetSuitToString()
    {
        return suit.ToString();
    }

    public Suit GetSuit()
    {
        return suit;
    }

    public string GetRank()
    {
        return rank.ToString();
    }

    public int GetValue()
    {
        return value;
    }

    private void OnMouseEnter()
    {
        originalScale = transform.localScale;
        if (transform.parent.parent == currentPlayerHand.transform || transform.parent == auctionLeftOvers.transform)
        {
            transform.localScale = originalScale * 1.1f;
        }
    }

    private void OnMouseExit()
    {
        if (transform.localScale != originalScale)
        {
            transform.localScale = originalScale;
        }
    }

}

public static class SuitValue
{
    public static int GetValue(this Card.Suit suit)
    {
        switch (suit)
        {
            case Card.Suit.Hearts:
                return 100;
            case Card.Suit.Diamonds:
                return 80;
            case Card.Suit.Clubs:
                return 60;
            case Card.Suit.Spades:
                return 40;
            default:
                Debug.LogError("Cannot obtain suit value.");
                return 0;
        }
    }
}