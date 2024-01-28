using System;
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
    private List<int> teamScore; // teamScore[0]: team1, teamScore[1]: team2 etc.
    const int targetScore = 1000;
    private int currentBid;
    private Player currentBidder;

    [SerializeField] private GameObject p1;
    [SerializeField] private GameObject p2;
    [SerializeField] private GameObject p3;
    [SerializeField] private GameObject p4;
    [SerializeField] private GameObject t;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        Debug.Log("Game Manager started.");
        InitializePlayers();
        Debug.Log("Players initialized");
        mainDeck = new Deck();
        Debug.Log("Deck initialized");
        mainDeck.InitializeAndShuffle();
        Debug.Log("Cards shuffled.");
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

    void CreateCard(Card.Suit suit, Card.Rank rank)
    {
        GameObject cardObject = new GameObject();

        // Add the Card script component to the GameObject
        Card cardScript = cardObject.AddComponent<Card>();

        // Initialize the card with the specified suit and rank
        cardScript = new Card(suit, rank);

        // Set the position, rotation, and scale as needed
        cardObject.transform.position = new Vector3(0f, 0f, 0f);
        cardObject.transform.rotation = Quaternion.identity;
        cardObject.transform.localScale = new Vector3(1f, 1f, 1f);
    }
    
    void DealInitialCards()
    {
        int initialCardCount = 5;

        for (int i = 0; i < mainDeck.cards.Count; i++)
        {
            Debug.Log("Card " + i + " : " + mainDeck.cards[i].GetCardFullName());
        }

        for (int i = 0; i < initialCardCount; i++)
        {
            Card currentCard = mainDeck.cards[i];
            GameObject go = GameObject.Find("Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank());
            go.gameObject.transform.SetParent(p1.transform);
            go.GetComponent<Card>().SetVisible(true);
        }
        for (int i = initialCardCount; i < 2*initialCardCount; i++)
        {
            Card currentCard = mainDeck.cards[i];
            GameObject go = GameObject.Find("Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank());
            go.transform.Rotate(0,0,90);
            go.gameObject.transform.SetParent(p2.transform);
            go.GetComponent<Card>().SetVisible(false);
        }
        for (int i = 2* initialCardCount; i <3* initialCardCount; i++)
        {
            Card currentCard = mainDeck.cards[i];
            GameObject go = GameObject.Find("Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank());
            go.gameObject.transform.SetParent(p3.transform);
            go.GetComponent<Card>().SetVisible(false);
        }
        for (int i = 3* initialCardCount; i < 4*initialCardCount; i++)
        {
            Card currentCard = mainDeck.cards[i];
            GameObject go = GameObject.Find("Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank());
            go.transform.Rotate(0, 0, 90);
            go.gameObject.transform.SetParent(p4.transform);
            go.GetComponent<Card>().SetVisible(false);
        }

/*        for (int i = 4 * initialCardCount; i < 4 * initialCardCount + 4; i++)
        {
            Card currentCard = mainDeck.cards[i];
            GameObject go = GameObject.Find("Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank());
            go.gameObject.transform.SetParent(t.transform);
            go.GetComponent<Card>().SetVisible(false);
        }*/

        /*
        foreach (Player player in players)
        {
            for (int i = 0; i < initialCardCount; i++)
            {
                mainDeck.cards[i].gameObject.transform.SetParent(p1.transform);

                
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
        */
    }

    void Auction()
    {
        int playersRemainingInAuction = players.Count;
        currentBid = 100;

        while (playersRemainingInAuction > 1)
        {
            foreach (Player player in players)
            {
                if (!player.HasPassed())
                {
                    if (player.IsBidding(currentBid + 10))
                    {
                        currentBidder = player;
                        currentBid += 10;
                        Debug.Log(player.GetPlayerName() + " bids " + currentBid + " points.");
                    }
                    else
                    {
                        playersRemainingInAuction--;
                        Debug.Log(player.GetPlayerName() + " passes.");
                    }
                }
                else
                {
                    Debug.Log(player.GetPlayerName() + " has already passed.");
                }
            }
        }

        Debug.Log(currentBidder.GetPlayerName() + " wins the auction with a bid of " + currentBid + " points.");

        for (int i = 0; i < 4; i++)
        {
            Card drawnCard = mainDeck.DrawCard();
            currentBidder.AddCardToHand(drawnCard);
        }
    }

    private Player GetNextPlayer(Player currentPlayer)
    {
        int currentIndex = players.IndexOf(currentPlayer);

        if (currentIndex != -1)
        {
            int nextIndex = (currentIndex + 1) % players.Count;
            return players[nextIndex];
        }

        return null;
    }

    private void UpdatePlayerScore(List<Card> trick, Player winner)
    {
        int value = 0;
        for (int i = 0; i < trick.Count; i++)
        {
            value += trick[i].GetValue();     
        }
        winner.AddScore(value);
    }

    private bool UpdateTrickWinner(List<Card> cards)
    {
        int lastCard = cards.Count - 2;
        for (int i = 0; i < cards.Count - 1; i++) 
        {
            if (cards[0].GetSuit() == cards[lastCard].GetSuit() && cards[0].GetValue() < cards[lastCard].GetValue())
            {
                return true;
            }
        }
        return false;
    }

    private void Gameplay()
    {
        Player currentPlayer = currentBidder;
        Player trickWinner = currentBidder;
        List<Card> currentTrick = new List<Card>();

        int numberOfTurns = currentPlayer.GetCardsInHand();

        for (int i = 0; i < numberOfTurns; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                // TODO: marriage / meldunek
                Card playedCard = currentPlayer.MakeMove(currentTrick); 
                currentTrick.Add(playedCard);
                if (UpdateTrickWinner(currentTrick))
                {
                    trickWinner = currentPlayer;
                }
                currentPlayer = GetNextPlayer(currentPlayer);
            }
            UpdatePlayerScore(currentTrick, trickWinner);
            currentTrick.Clear();
            currentPlayer = trickWinner;
        }
    }

    void DealCardsToOtherPlayers()
    {
        foreach (Player player in players)
        {
            if (currentBidder != player)
            {
                Debug.Log("Dealing card to player " + player.GetPlayerName());
                // gracz wybiera karty z talii
            }
        }
    }

    void StartRound()
    {
        Debug.Log("Starting Round " + roundNumber);

        Auction();
        DealCardsToOtherPlayers();
        Gameplay();
    }

    void EndRound()
    {
        Debug.Log("Ending Round " + roundNumber);
        CalculateRoundScores();
        CheckForGameEnd();
        roundNumber++;
        // przygotowanie na nastepna runde
        mainDeck.InitializeAndShuffle();
        AudioManager.Instance.PlayDealCardSound();
        DealInitialCards();
    }

    void CalculateRoundScores()
    {
        foreach (Player player in players)
        {
            if (player.GetTeam() == 1)
            {
                teamScore[0] += player.GetScore();
            }
            else
            {
                teamScore[1] += player.GetScore();
            }
            player.ClearHand();
        }
    }

    void CheckForGameEnd()
    {
        if (teamScore[0] >= targetScore)
        {
            AudioManager.Instance.PlayWinSound();
            Debug.Log("Team 1 wins!");
            // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
        }
        else if (teamScore[1] >= targetScore)
        {
            AudioManager.Instance.PlayWinSound();
            Debug.Log("Team 2 wins!");
            // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
            return;
        }
    }
}
