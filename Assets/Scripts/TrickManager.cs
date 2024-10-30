using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickManager : MonoBehaviour
{
    public List<Card> trickCards = new List<Card>();

    public void AddCard(Card card)
    {
        if (!trickCards.Contains(card))
        {
            trickCards.Add(card);
        }
    }

    public void RemoveCard(Card card)
    {
        if (trickCards.Contains(card))
        {
            trickCards.Remove(card);
        }
    }

    public Card[] GetTrickCards()
    {
        return trickCards.ToArray();
    }

    public void ClearTrickCards()
    {
        trickCards.Clear();
    }
}

