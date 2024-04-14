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
    private int roundNumber;
    private List<int> teamScore; // teamScore[0]: team1, teamScore[1]: team2 etc.
    const int targetScore = 1000;
    private int currentBid;
    private Player currentBidder; // Player who is winning the auction at the moment
    private Player currentPlayer; // Player who is making any move at the moment
    private bool played;
    private Card playedCard;
    [SerializeField] private GameObject t;
    [SerializeField] private GameObject downPlace;
    [SerializeField] private GameObject leftPlace;
    [SerializeField] private GameObject upPlace;
    [SerializeField] private GameObject rightPlace;
    [SerializeField] private GameObject auctionDialog;

    void Start()
    {
        InitializeGame();
        StartRound();
    }

    void InitializeGame()
    {
        mainDeck.Shuffle();
        Debug.Log("Cards shuffled.");
        DealInitialCards();
        roundNumber = 1;
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

        //_instance.GetComponent<RunLog>().logText("<"+currentBidder.GetPlayerName() + "> has bidded " + currentBid + ".", Color.white);
        _instance.GetComponent<RunLog>().logText("<"+currentBidder.name + "> has bidded " + currentBid + ".", Color.white);

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

        int passed = 0;

        foreach(Player player in players)
        {
            if (player.HasPassed())
                passed++;
        }

        if(passed >= 3) //Wygrana jednego gracza -> oddanie kart innym graczom
        {
            //Debug.Log(currentBidder.GetPlayerName() + " wins the auction with a bid of " + currentBid + " points.");
            Debug.Log(currentBidder.name + " wins the auction with a bid of " + currentBid + " points.");

            for (int i = 0; i < 4; i++)
            {
                Card drawnCard = mainDeck.DrawCard();

                // UNCOMMENT BELOW WHEN CARDS DEALING TO OTHERS IMPLEMENTED
                //currentBidder.AddCardToHand(drawnCard);
            }

            MovePlayerToPosition(currentBidder, Player.Position.down, true);
            UpdateCardVisibility();

            // UNCOMMENT BELOW WHEN CARDS DEALING TO OTHERS IMPLEMENTED
            //DealCardsToOtherPlayers();
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

        currentPlayer = players[0];     // TODO: Gracz rozpoczynaj�cy dan� tur�
        currentBidder = currentPlayer;
        currentPlayer = GetNextPlayer(currentPlayer);

        MovePlayersToNextPositions();
        UpdateCardVisibility();

        DisplayAuctionDialog();
    }



    private Player GetNextPlayer(Player currentPlayer)
    {
        int currentIndex = players.IndexOf(currentPlayer);

        if (currentIndex != -1)
        {
            int nextIndex = (currentIndex + 1) % players.Count;
            this.currentPlayer = currentPlayer;
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
        winner.AddScore(value);
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
        Player currentPlayer = currentBidder;
        Player trickWinner = currentBidder;
        List<Card> currentTrick = new List<Card>();

        int numberOfTurns = currentPlayer.GetCardsInHand();

        for (int i = 0; i < numberOfTurns; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                Debug.Log("Current player: " + currentPlayer.playerNumber);
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
            MovePlayerToPosition(trickWinner, Player.Position.down);
            UpdateCardVisibility();
        }
    }

    public void Play(Card card)
    {
        playedCard = card;
        played = true;
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
        StartCoroutine(Gameplay());
    }

    void EndRound()
    {
        Debug.Log("Ending Round " + roundNumber);
        CalculateRoundScores();
        CheckForGameEnd();
        roundNumber++;
        // przygotowanie na nastepna runde
        //tutaj powinnismy karty wlozyc do talii znowu
        mainDeck.Shuffle();
        AudioManager.Instance.PlayDealCardSound();
        DealInitialCards();
    }

    void CalculateRoundScores()
    {
        if(currentBidder.GetScore() < currentBid)
        {
            currentBidder.AddScore(-currentBid);
        }
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

            // Przesuni�cie gracza na kolejn� pozycj�
            if (player.position == Player.Position.right)
            {
                playerObject.transform.position = downPlace.transform.position;
                player.position = Player.Position.down;
            }
            else if (player.position == Player.Position.up)
            {
                playerObject.transform.position = rightPlace.transform.position;
                player.position = Player.Position.right;
            }
            else if (player.position == Player.Position.left)
            {
                playerObject.transform.position = upPlace.transform.position;
                player.position = Player.Position.up;
            }
            else if (player.position == Player.Position.down)
            {
                playerObject.transform.position = leftPlace.transform.position;
                player.position = Player.Position.left;
            }
            else
            {
                Debug.LogWarning("Unsupported player position!");
            }

            playerObject.transform.rotation = Quaternion.Euler(0, 0, (playerObject.transform.rotation.eulerAngles.z+90)%180);
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
                player.gameObject.transform.position = rightPlace.transform.position;
                player.gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
            }
            else if (position == Player.Position.up)
            {
                player.gameObject.transform.position = upPlace.transform.position;
                player.gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
            }
            else if (position == Player.Position.left)
            {
                player.gameObject.transform.position = leftPlace.transform.position;
                player.gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
            }
            else if (position == Player.Position.down)
            {
                player.gameObject.transform.position = downPlace.transform.position;
                player.gameObject.transform.eulerAngles = new Vector3(0, 0, 90);
            }
            else
            {
                Debug.LogWarning("Unsupported player position!");
                return;
            }

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
