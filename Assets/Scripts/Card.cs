using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades };
    public enum Rank { Nine, Ten, Jack, Queen, King, Ace };

    //mozecie tez dodac tutaj np image albo sprite i jakas wartosc np czyOdwrocona i zmieniac zdjecie jesli jest i jesli nie jest

    private Suit suit;
    private Rank rank;
    private int value;

    public Card(Suit cardSuit, Rank cardRank)
    {
        suit = cardSuit;
        rank = cardRank;

        switch (rank)
        {
            case Rank.Ace:
                value = 11;
                break;
            case Rank.Ten:
                value = 10;
                break;
            case Rank.King:
                value = 4;
                break;
            case Rank.Queen:
                value = 3;
                break;
            case Rank.Jack:
                value = 2;
                break;
            case Rank.Nine:
                value = 0;
                break;
            default:
                value = 0;
                break;
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

    public int GetValue()
    {
        return value;
    }
}
