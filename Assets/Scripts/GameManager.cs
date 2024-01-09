using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameManager");
                _instance = go.AddComponent<GameManager>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<Player> players;
    public Deck mainDeck;
    private int roundNumber;
    private List<int> teamScore;
    const int targetScore = 1000;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        InitializePlayers();
        mainDeck = new Deck();
        mainDeck.InitializeAndShuffle();
        DealInitialCards();
        roundNumber = 1;
    }

    void InitializePlayers()
    {
        players = new List<Player>();

        //do dostosowania w zaleznosci od trybu gry (teraz jest druzynowa)

        Player player1 = new Player("Player 1", 1);
        Player player2 = new Player("Player 2", 1);
        Player player3 = new Player("Player 3", 2);
        Player player4 = new Player("Player 4", 2);

        players.Add(player1);
        players.Add(player2);
        players.Add(player3);
        players.Add(player4);
    }

    void DealInitialCards()
    {
        int initialCardCount = 10;

        foreach (Player player in players)
        {
            for (int i = 0; i < initialCardCount; i++)
            {
                Card drawnCard = mainDeck.DrawCard();

                if (drawnCard != null)
                {
                    player.AddCardToHand(drawnCard);
                }
                else
                {
                    Debug.LogError("Not enough cards in the deck!");
                }
            }
        }
    }

    void StartRound()
    {
        Debug.Log("Starting Round " + roundNumber);
        // tutaj jakas logika z tym rozpoczynaniem czyli licytacja i oddawanie kart czy cos takiego 
    }

    void EndRound()
    {
        Debug.Log("Ending Round " + roundNumber);
        CalculateRoundScores();
        CheckForGameEnd();
        roundNumber++;
        // przygotowanie na nastêpn¹ runde
        mainDeck.InitializeAndShuffle();
        AudioManager.Instance.PlayDealCardSound();
        DealInitialCards();
    }

    void CalculateRoundScores()
    {
        // tutaj powinna byc logika wyliczania punktow
        foreach (Player player in players)
        {
            if (player.GetTeam() == 1)
            {
                //tutaj polecam skorzystac z teamScore
                players[0].AddScore(player.score);
            }
            else
            {
                //tutaj tez 
                players[2].AddScore(player.score);
            }
            player.ClearHand();
        }
    }

    void CheckForGameEnd()
    {
        foreach (Player player in players)
        {
            if (player.score >= targetScore) //tutaj tez mozna uzyc teamScore, zeby te player score byly zarezerwowane dla gry indywidualnej, a nie druzynowej
            {
                AudioManager.Instance.PlayWinSound();
                Debug.Log("Team " + player.GetTeam() + " wins!");
                // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
                return;
            }
        }
    }
}
