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

    public Card(Suit cardSuit, Rank cardRank)
    {
        suit = cardSuit;
        rank = cardRank;
    }

    public string GetCardFullName() //mozna uzyc do debugu
    {
        string rankString = rank.ToString();
        string suitString = suit.ToString();
        return rankString + " of " + suitString;
    }
}
