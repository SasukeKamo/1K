using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private List<Card> cards;

    public Deck()
    {
        cards = new List<Card>();
    }

    private void InitializeDeck()
    {
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                Card newCard = new Card(suit, rank);
                cards.Add(newCard);
            }
        }
    }

    private void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            Card tempCard = cards[i];
            int randomIndex = Random.Range(i, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = tempCard;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count > 0)
        {
            Card drawnCard = cards[0];
            cards.RemoveAt(0);
            return drawnCard;
        }
        else
        {
            Debug.LogError("Deck is empty!");
            return null;
        }
    }

    public void InitializeAndShuffle()
    {
        InitializeDeck();
        Shuffle();
    }
}

