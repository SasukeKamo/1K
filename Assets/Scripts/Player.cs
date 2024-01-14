using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string playerName;
    private List<Card> hand;
    private int score;
    private int team;
    private bool hasPassed = false;

    public Player(string name, int team)
    {
        playerName = name;
        hand = new List<Card>();
        score = 0;
        this.team = team;
    }

    public int GetCardsInHand()
    {
        return hand.Count;
    }

    public Card MakeMove(List<Card> trick)
    {
        // TBD
        return trick[0];
    }

    public void DealCardsToOtherPlayers()
    {
        // TBD
    }

    public bool HasPassed()
    {
        return hasPassed;
    }

    public bool IsBidding(int bid)
    {
        // Czy gracz licytuje wartosc int bid? -> gracz klika w okienko: "tak" lub "nie"
        bool decision = true; // pobierz wartosc 
        if (!decision) hasPassed = true;
        return decision;
    }

    public void AddCardToHand(Card card)
    {
        hand.Add(card);
    }

    private void RemoveCardFromHand(Card card)
    {
        AudioManager.Instance.PlayPlayCardSound();
        hand.Remove(card);
    }

    public void ClearHand()
    {
        hand.Clear();
    }

    public int GetScore()
    {
        return score;
    }

    public void AddScore(int points)
    {
        score += points;
    }

    private void TransferCardToPlayer(Card card, Player otherPlayer)
    {
        if (hand.Contains(card))
        {
            RemoveCardFromHand(card);

            otherPlayer.AddCardToHand(card);
        }
        else
        {
            Debug.LogWarning("Player doesn't have this card");
        }
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public int GetTeam()
    {
        return team;
    }

}
