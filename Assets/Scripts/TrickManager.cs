using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickManager : MonoBehaviour
{
    public List<Card> trickCards = new List<Card>();
    private List<Card> playedCards = new List<Card>();

    public void AddCard(Card card)
    {
        if (!trickCards.Contains(card))
        {
            trickCards.Add(card);
        }
    }

    public void ClearPlayedCards(){
        playedCards.Clear();
    }

    public List<Card> GetPlayedCards(){
        return playedCards;
    }

    public void AddCardToPlayed(Card card)
    {
        if (!playedCards.Contains(card))
        {
            playedCards.Add(card);
        }
    }

    public void RemoveCard(Card card)
    {
        if (trickCards.Contains(card))
        {
            trickCards.Remove(card);
        }
    }

    public List<Card> GetTrick(){
        return trickCards;
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

