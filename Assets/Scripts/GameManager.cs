using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public Deck otherCards;
    private int roundNumber;
    private List<int> teamScore = new List<int>(); // teamScore[0]: team1, teamScore[1]: team2 etc.
    const int targetScore = 1000;
    private int currentBid;
    private Player currentBidder; // Player who is winning the auction at the moment
    public Player currentPlayer; // Player who is making any move at the moment
    public Player currentCardReceiver;  // Player who is being given a card at the moment
    private int firstPlayer = 0;
    private bool played;
    private Card playedCard;
    public bool isGivingStage = false;
    public RunLog runLog;
    private bool gameplayFinished = false;
    private bool auctionFinished = false;
    [SerializeField] private GameObject t;
    [SerializeField] private GameObject downPlace;
    [SerializeField] private GameObject leftPlace;
    [SerializeField] private GameObject upPlace;
    [SerializeField] private GameObject rightPlace;
    [SerializeField] private GameObject auctionDialog;
    [SerializeField] private GameObject handOverDialog;
    [SerializeField] private GameObject restOfTheDeck;

    void Start()
    {
        runLog = _instance.GetComponent<RunLog>();
        InitializeGame();
        StartCoroutine(GameLoop());
    }

    void InitializeGame()
    {
        mainDeck.Shuffle();
        Debug.Log("Cards shuffled.");
        DealInitialCards();
        roundNumber = 1;
        teamScore.Add(0);
        teamScore.Add(0);
        Debug.Log($"{teamScore[0]}, {teamScore[1]}");

    }

    IEnumerator GameLoop()
    {
        while (teamScore[0] < targetScore && teamScore[1] < targetScore)
        {
            Debug.Log("Round Started");
            yield return StartCoroutine(StartRound());
            EndRound();
            Debug.Log("Round Ended");
        }
    }

    void DealInitialCards()
    {
        int initialCardCount = 5;

        for (int p = 0; p < players.Count; p++)
        {
            for (int i = p * initialCardCount; i < (p + 1) * initialCardCount; i++)
            {
                Card currentCard = mainDeck.cards[i];
                string cardName = "Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank();

                GameObject go = GameObject.Find(cardName);
                go.transform.SetParent(players[p].transform);
                players[p].AddCardToHand(currentCard);

                if (p == 0)
                {
                    go.GetComponent<Card>().SetVisible(true);
                }
                else if (p == 2)
                {
                    go.GetComponent<Card>().SetVisible(false);
                }
                else
                {
                    go.transform.Rotate(0, 0, 90);
                    go.GetComponent<Card>().SetVisible(false);
                }
            }
        }
        SaveRestOfTheCards();
    }

    private void SaveRestOfTheCards()
    {
        int initialCardCount = 5;
        for (int i = players.Count * initialCardCount; i < mainDeck.cards.Count; i++)
        {
            Card currentCard = mainDeck.cards[i];
            otherCards.AddCard(currentCard);
            string cardName = "Card_" + currentCard.GetSuit() + "_" + currentCard.GetRank();

            GameObject go = GameObject.Find(cardName);
            go.transform.SetParent(restOfTheDeck.transform);
            currentCard.SetVisible(true);
        }
    }

    void DisplayAuctionDialog()
    {
        auctionDialog.SetActive(true);

        GameObject text = GameObject.Find("CurrentBidText");

        TextMeshProUGUI currentBidText = GameObject.Find("CurrentBidText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI currentWinnerText = GameObject.Find("CurrentWinnerText").GetComponent<TextMeshProUGUI>();

        currentBidText.text = "CURRENT BID: " + currentBid;
        currentWinnerText.text = "CURRENT WINNER: " + currentBidder.name;
    }

    public void PositiveAuctionDialog()
    {
        currentBid += 10;
        currentBidder = currentPlayer;
        auctionDialog.SetActive(false);

        runLog.logText("<"+currentBidder.playerName + "> has bidded " + currentBid + ".", Color.white);

        do
        {
            currentPlayer = GetNextPlayer(currentPlayer);
            MovePlayersToNextPositions();

        } while (currentPlayer.HasPassed());
        UpdateCardVisibility();

        DisplayAuctionDialog();
    }
    public void NegativeAuctionDialog()
    {
        auctionDialog.SetActive(false);

        currentPlayer.SetPassed(true);

        runLog.logText("<" + currentBidder.playerName + "> passed.", Color.yellow);

        int passed = 0;

        foreach(Player player in players)
        {
            if (player.HasPassed())
                passed++;
        }

        if (passed >= 3) //Wygrana jednego gracza -> oddanie kart innym graczom
        {
            currentPlayer = currentBidder;

            Debug.Log(currentBidder.name + " wins the auction with a bid of " + currentBid + " points.");


/*            for (int i = 0; i < 4; i++)
            {
                Card drawnCard = mainDeck.DrawCard();

                // UNCOMMENT BELOW WHEN CARDS DEALING TO OTHERS IMPLEMENTED
                //currentBidder.AddCardToHand(drawnCard);
            }*/

            MovePlayerToPosition(currentBidder, Player.Position.down, true);
            UpdateCardVisibility();

            // UNCOMMENT BELOW WHEN CARDS DEALING TO OTHERS IMPLEMENTED
            DealCardsToOtherPlayers();
        }
        else //Nie wszyscy spasowali -> dilog dla nast�pnego gracza
        {
            do
            {
                currentPlayer = GetNextPlayer(currentPlayer);
                MovePlayersToNextPositions();

            } while (currentPlayer.HasPassed());
            UpdateCardVisibility();

            DisplayAuctionDialog();
        }
    }

    void Auction()
    {
        currentBid = 100;

        currentPlayer = players[firstPlayer];     // TODO: Gracz rozpoczynaj�cy dan� tur�
        currentBidder = currentPlayer;
        currentPlayer = GetNextPlayer(currentPlayer);

        //MovePlayersToNextPositions();
        MovePlayerToPosition(currentPlayer, Player.Position.down);
        UpdateCardVisibility();

        DisplayAuctionDialog();
    }



    public Player GetNextPlayer(Player currentPlayer)
    {
        int currentIndex = players.IndexOf(currentPlayer);

        if (currentIndex != -1)
        {
            int nextIndex = (currentIndex + 1) % players.Count;
            return players[nextIndex];
        }

        return null;
    }

    private Player GetPreviousPlayer(Player currentPlayer)
    {
        int currentIndex = players.IndexOf(currentPlayer);

        if (currentIndex != -1)
        {
            int prevIndex = (currentIndex + players.Count - 1) % players.Count;
            return players[prevIndex];
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
        winner.AddRoundScore(value);
    }

    private bool UpdateTrickWinner(List<Card> cards)
    {
        int lastCard = cards.Count - 1;
        int max = 0;
        for (int i = 0; i < cards.Count - 1; i++)
        {
            if (cards[i].GetValue() > max && cards[0].GetSuit() == cards[i].GetSuit())
                max = cards[i].GetValue();
        }

        for (int i = 0; i < cards.Count - 1; i++)
        {
            if (cards[0].GetSuit() == cards[lastCard].GetSuit() && max < cards[lastCard].GetValue())
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerator Gameplay()
    {
        yield return new WaitUntil(() => auctionFinished);

        //Debug.Log("Gameplay started");

        auctionFinished = false;

        Player currentPlayer = currentBidder;
        Player trickWinner = currentBidder;
        List<Card> currentTrick = new List<Card>();

        gameplayFinished = false;

        int numberOfTurns = currentPlayer.GetCardsInHand();

        for (int i = 0; i < numberOfTurns; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                // TODO: marriage / meldunek
                yield return new WaitUntil(() => played);
                played = false;
                currentTrick.Add(playedCard);
                if (UpdateTrickWinner(currentTrick))
                {
                    trickWinner = currentPlayer;
                }
                currentPlayer = GetNextPlayer(currentPlayer);
            }
            UpdatePlayerScore(currentTrick, trickWinner);
            currentTrick.Clear();
            int playerNum = currentPlayer.playerNumber;
            currentPlayer = trickWinner;
            MovePlayerToPosition(trickWinner, Player.Position.down, true);
            UpdateCardVisibility();
        }
        gameplayFinished = true;
    }

    public void Play(Card card)
    {
        playedCard = card;
        played = true;
    }

    void DealCardsToOtherPlayers()
    {
        handOverDialog.SetActive(true);
        isGivingStage = true;
        currentCardReceiver = GetNextPlayer(currentPlayer);

        TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
        currentCardReceiverText.text = "Choose card for player: " + currentCardReceiver.playerName;
    }

    public void EndDealingStage()
    {
        isGivingStage = false;
        handOverDialog.SetActive(false);
        auctionFinished = true;
    }

    IEnumerator StartRound()
    {
        Debug.Log("Starting Round " + roundNumber);

        Auction();
//        DealCardsToOtherPlayers();
        StartCoroutine(Gameplay());
        yield return new WaitUntil(() => gameplayFinished);
        gameplayFinished = false;
    }

    void ResetDeck()
    {
        string[] s = { "Hearts", "Diamonds", "Clubs", "Spades" };
        List<string> suits = new List<string>(s);

        string[] r = { "Nine", "Ten", "Jack", "Queen", "King", "Ace" };
        List<string> ranks = new List<string>(r);

        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                string name = $"Card_{suit}_{rank}";
                GameObject go = GameObject.Find(name);
                go.transform.SetParent(null);
                go.transform.position = new Vector3Int(343, 512, -918);
                go.transform.localEulerAngles = new Vector3Int(0, 0, 0);
                Card card = go.GetComponent<Card>();
                card.SetVisible(false);
            }
        }

        foreach (Card card in mainDeck.cards)
        {
            Debug.Log(card.GetCardFullName());
        }
    }

    void EndRound()
    {
        Debug.Log("Ending Round " + roundNumber);
        CalculateRoundScores();
        CheckForGameEnd();
        roundNumber++;
        ResetDeck();

        firstPlayer = (firstPlayer + 1) % players.Count;
        //Debug.Log(firstPlayer);
        InputHandler.Instance.ResetCardsToDeal();
        foreach (Player player in players)
        {
            player.Reset();
        }

        //MovePlayerToPosition(players[firstPlayer], Player.Position.down);
        // przygotowanie na nastepna runde
        //tutaj powinnismy karty wlozyc do talii znowu
        mainDeck.Shuffle();
        //AudioManager.Instance.PlayDealCardSound();
        DealInitialCards();
    }

    void CalculateRoundScores()
    {
        if(currentBidder.GetRoundScore() < currentBid)
        {
            currentBidder.SetRoundScore(-currentBid);
        }
        else
        {
            currentBidder.SetRoundScore(currentBid);
        }
        foreach (Player player in players)
        {
            if (player.GetTeam() == 1)
            {
                teamScore[0] += player.GetRoundScore();
            }
            else
            {
                teamScore[1] += player.GetRoundScore();
            }
            player.SetRoundScore(0);
            player.ClearHand();
        }

        foreach (Player player in players)
        {
            if (player.GetTeam() == 1)
            {
                player.SetScore(teamScore[0]);
            }
            else
            {
                player.SetScore(teamScore[1]);
            }
            player.ClearHand();
        }
    }

    void CheckForGameEnd()
    {
        if (teamScore[0] >= targetScore)
        {
            //AudioManager.Instance.PlayWinSound();
            Debug.Log("Team 1 wins!");
            // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
        }
        else if (teamScore[1] >= targetScore)
        {
            //AudioManager.Instance.PlayWinSound();
            Debug.Log("Team 2 wins!");
            // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
            return;
        }
    }

    public Player GetPlayerForCurrentCard(GameObject cardObject)
    {
        foreach (Player player in GameManager.Instance.players)
        {
            if (player.transform == cardObject.transform.parent)
            {
                return player;
            }
        }
        return null;
    }

    public List<Card> GetPlayerHand(Player current) {
        List<Card> hand = null;
        foreach (Player player in GameManager.Instance.players) {
            if (player == current){
                return player.hand;
            }
        }
        return hand;
    }

    public void MovePlayersToNextPositions()
    {
        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObject = players[i].gameObject;
            Player player = players[i];

            // Przesuniecie gracza na kolejna pozycje
            if (player.position == Player.Position.right)
            {
                MovePlayerToPosition(player, Player.Position.up, false);
            }
            else if (player.position == Player.Position.up)
            {
                MovePlayerToPosition(player, Player.Position.left, false);
            }
            else if (player.position == Player.Position.left)
            {
                MovePlayerToPosition(player, Player.Position.down, false);
            }
            else if (player.position == Player.Position.down)
            {
                MovePlayerToPosition(player, Player.Position.right, false);
            }
            else
            {
                Debug.LogWarning("Unsupported player position!");
            }
        }
    }

    public void MovePlayerToPosition(Player playerPivot, Player.Position positionPivot, bool moveOtherPlayers = true)
    {
        int p = 1;
        if (moveOtherPlayers) p = players.Count;

        Player player = playerPivot;
        Player.Position position = positionPivot;

        for (int i = 0; i < p; i++)
        {

            if (position == Player.Position.right)
            {
                player.gameObject.transform.SetParent(rightPlace.transform);
            }
            else if (position == Player.Position.up)
            {
                player.gameObject.transform.SetParent(upPlace.transform);
            }
            else if (position == Player.Position.left)
            {
                player.gameObject.transform.SetParent(leftPlace.transform);
            }
            else if (position == Player.Position.down)
            {
                player.gameObject.transform.SetParent(downPlace.transform);
            }
            else
            {
                Debug.LogWarning("Unsupported player position!");
                return;
            }

            player.gameObject.transform.localPosition = Vector3.zero;
            player.gameObject.transform.localRotation = Quaternion.identity;

            player.position = position;

            position = player.GetNextPosition(player.position);
            player = GetNextPlayer(player);
        }
    }


    public void UpdateCardVisibility()
    {
        int currentPlayerIndex = 0;

        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObject = players[currentPlayerIndex].gameObject;
            Player player = players[currentPlayerIndex];

            if (player.position == Player.Position.down)
            {
                foreach (Transform cardTransform in playerObject.transform)
                {
                    Card card = cardTransform.GetComponent<Card>();
                    if (card != null)
                    {
                        card.SetVisible(true);
                    }
                }
            }
            else
            {

                foreach (Transform cardTransform in playerObject.transform)
                {
                    Card card = cardTransform.GetComponent<Card>();
                    if (card != null)
                    {
                        card.SetVisible(false);
                    }
                }
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }
    }


}