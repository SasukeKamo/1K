using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public enum Position { down, left, up, right };
    public Position position;

    [SerializeField] 
    public string playerName;

    public List<Card> hand;
    private int score = 0;
    private int roundScore = 0;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI roundScoreText;
    public int team;
    private bool hasPassed = false;
    private bool hasBidded = false;
    public int playerNumber;

    void Awake()
    {
        scoreText = GameObject.Find("ScoreP" + playerNumber.ToString()).GetComponent<TextMeshProUGUI>();
        scoreText.text = score.ToString();

        roundScoreText = GameObject.Find("RoundScoreP" + playerNumber.ToString()).GetComponent<TextMeshProUGUI>();
        roundScoreText.text = roundScore.ToString();
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

    public void SetPassed(bool passed) { hasPassed = passed; }
    public bool HasPassed() { return hasPassed; }

    public void SetBidded(bool bidded) { hasBidded = bidded; }
    public bool HasBidded() { return hasBidded; }

    public void AddCardToHand(Card card)
    {
        hand.Add(card);
        //card.gameObject.transform.Rotate(0, 0, 90);

        // Rotationg card to match hand rotation
        //Vector3 currentRotation = transform.eulerAngles;
        //currentRotation.z = 0;
        //card.gameObject.transform.eulerAngles = currentRotation;
    }

    public void RemoveCardFromHand(Card card)
    {
        AudioManager.Instance.PlayPlayCardSound();
        if (hand.Contains(card)){
            hand.Remove(card);
        }
        else
        {
            Debug.LogWarning("Player doesn't have this card");
        }
    }

    public void ClearHand()
    {
        hand.Clear();
    }

    public int GetScore()
    {
        return score;
    }

    public void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString();
    }

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = score.ToString();
    }

    public int GetRoundScore()
    {
        return roundScore;
    }

    public void SetRoundScore(int score)
    {
        roundScore = score;
        roundScoreText.text = roundScore.ToString();
    }

    public void AddRoundScore(int points)
    {
        roundScore += points;
        roundScoreText.text = roundScore.ToString();
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

    public Position GetNextPosition(Position position)
    {
        Position[] arr = (Position[])Enum.GetValues(position.GetType());
        int j = Array.IndexOf(arr, position) + 1;
        return (arr.Length == j) ? arr[0] : arr[j];
    }

    public void Reset()
    {
        hasBidded = false;
        hasPassed = false;
    }

}
