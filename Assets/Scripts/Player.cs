using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private string playerName;
    private List<Card> hand;
    public int score;
    private int team;

    public Player(string name, int team)
    {
        playerName = name;
        hand = new List<Card>();
        score = 0;
        this.team = team;
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
