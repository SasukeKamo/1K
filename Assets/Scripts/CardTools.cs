using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardTools
{
    public static string AsSymbol(this Card.Rank rank)
    {
        return rank switch
        {
            Card.Rank.Nine => "9",
            Card.Rank.Ten => "10",
            Card.Rank.Jack => "J",
            Card.Rank.Queen => "Q",
            Card.Rank.King => "K",
            Card.Rank.Ace => "A",
            _ => "*"
        };
    }

    public static string AsSymbol(this Card.Suit suit)
    {
        return suit switch
        {
            Card.Suit.Hearts => "♥",
            Card.Suit.Diamonds => "♦",
            Card.Suit.Clubs => "♣",
            Card.Suit.Spades => "♠",
            _ => "*"
        };
    }
}
