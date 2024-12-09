using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Photon.Pun;

public class Player : MonoBehaviourPun
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

    public string ToDebugString()
    {
        return $"Player: {playerName}, " +
               $"Player Number: {playerNumber}, " +
               $"Position: {position}, " +
               $"Score: {score}, " +
               $"Round Score: {roundScore}, " +
               $"Team: {team}, " +
               $"Has Passed: {hasPassed}, " +
               $"Has Bidded: {hasBidded}, " +
               $"ScoreText: {scoreText}, "+
               $"RoundScoreText: {roundScoreText}, "+
               $"Hand: [{string.Join(", ", hand ?? new List<Card>())}]";
    }

    void Awake()
    {
    }

    public void Setup()
    {
        Debug.Log("Player"+playerNumber+" -> Setup()");

        if (playerNumber == 1 || playerNumber == 2) scoreText = GameObject.Find("ScoreP" + playerNumber).GetComponent<TextMeshProUGUI>();
        if (playerNumber == 1 || playerNumber == 2) scoreText.text = score.ToString();

        roundScoreText = GameObject.Find("RoundScoreP" + playerNumber.ToString()).GetComponent<TextMeshProUGUI>();
        roundScoreText.text = roundScore.ToString();

        hand = new List<Card>();

        Debug.Log(ToDebugString());
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

        if (GameManager.IsMultiplayerMode)
        {
            photonView.RPC("SyncAddCardToHand", RpcTarget.OthersBuffered, this, card.name);
        }
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
        if (playerNumber == 1 || playerNumber == 2) scoreText.text = score.ToString();
    }

    public void AddScore(int points)
    {
        score += points;
        if (playerNumber == 1 || playerNumber == 2) scoreText.text = score.ToString();
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

    public static byte[] SerializePlayer(object customType)
    {
        Player player = (Player)customType;

        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write(player.playerName);
                writer.Write((int)player.position);
                writer.Write(player.team);
                writer.Write(player.score);
                writer.Write(player.roundScore);
                writer.Write(player.hasPassed);
                writer.Write(player.hasBidded);
                writer.Write(player.playerNumber);

                writer.Write(player.hand.Count);
                foreach (Card card in player.hand)
                {
                    writer.Write(card.name);
                }

                return ms.ToArray();
            }
        }
    }

    public static object DeserializePlayer(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(ms))
            {
                string playerName = reader.ReadString();
                Player player = GameObject.Find(playerName)?.GetComponent<Player>() ?? new Player();

                player.playerName = playerName;
                player.position = (Player.Position)reader.ReadInt32();
                player.team = reader.ReadInt32();
                player.score = reader.ReadInt32();
                player.roundScore = reader.ReadInt32();
                player.hasPassed = reader.ReadBoolean();
                player.hasBidded = reader.ReadBoolean();
                player.playerNumber = reader.ReadInt32();

                int handCount = reader.ReadInt32();
                player.hand = new List<Card>();
                for (int i = 0; i < handCount; i++)
                {
                    string cardName = reader.ReadString();
                    Card card = GameObject.Find(cardName)?.GetComponent<Card>();
                    if (card != null)
                    {
                        player.hand.Add(card);
                    }
                }

                return player;
            }
        }
    }


}
