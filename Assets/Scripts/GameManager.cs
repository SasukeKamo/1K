using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _1K_ComputerPlayer;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Rendering;
using UnityEngine.XR;
using System.Security.Cryptography;
using Unity.VisualScripting;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
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
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }



    void Update()
    {
        // Check if the Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitToMenu();
        }
    }

    public void ExitToMenu()
    {
        if (GameManager.IsMultiplayerMode && PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
        IsMultiplayerMode = false;
        SceneManager.LoadScene("Menu");
    }

    public List<Player> players;
    public enum GamePhase { Start, Auction, Handover, Gameplay };
    public bool onePlayerMode = true;
    public GameRules gameRules;

    public Deck mainDeck;
    public Deck otherCards;
    private int roundNumber;
    private List<int> teamScore = new List<int>(); // teamScore[0]: team1, teamScore[1]: team2 etc.
    const int targetScore = 1000;
    private List<Tuple<Player, Card.Suit>> marriages;
    private List<Tuple<Player, Card.Suit>> tempMarriages;
    private int currentBid;
    private Player currentBidder; // Player who is winning the auction at the moment
    public Player currentPlayer; // Player who is making any move at the moment
    public Player GameplayCurrentPlayer;
    public Player currentCardReceiver;  // Player who is being given a card at the moment
    private int firstPlayer = 0;    // Player who is starting the round
    public const int humanPlayer = 0;    // Human player (if onePlayerMode) is always Player1
    private bool played;
    private Card playedCard;
    public bool isGivingStage = false;
    public RunLog runLog;
    private bool gameplayFinished = false;
    public bool auctionFinished = false;
    public bool setupFinished = false;
    public GamePhase gamePhase;
    public static string savePath = "./saves/save.txt";

    private Player localPlayer;
    public static bool IsMultiplayerMode = false;
    public static bool IsGameContinued = false;
    private bool IsRoundSynced = false;
    [SerializeField] private bool forcePlayerChangeDialog = true;
    [SerializeField] private GameObject t;
    [SerializeField] private TrickManager trickManager;
    [SerializeField] private GameObject downPlace;
    [SerializeField] private GameObject leftPlace;
    [SerializeField] private GameObject upPlace;
    [SerializeField] private GameObject rightPlace;
    [SerializeField] private GameObject auctionDialog;
    [SerializeField] private GameObject readyDialog;
    [SerializeField] private GameObject setupDialog;
    [SerializeField] private GameObject handOverDialog;
    [SerializeField] GameObject BombButton;
    [SerializeField] private GameObject waitingDialog;
    [SerializeField] private GameObject waitingForCardDialog;
    [SerializeField] private GameObject endGameDialog;
    [SerializeField] private GameObject restOfTheDeck;
    [SerializeField] private GameObject[] nickNames;


    void Start()
    {
        AudioManager.Instance.PlayGamesceneSong();
        runLog = _instance.GetComponent<RunLog>();
        //InitializeGame();

        gameRules = new GameRules(); // TU ZAINICJOWAC RULESY WG BOOLI WZIETYCH Z CHECKBOXOW

        if (IsMultiplayerMode)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            onePlayerMode = false;

            SetupMultiplayerGame();

            StartCoroutine(GameLoop());
        }
        else
        {
            if (IsGameContinued)
            {
                IsGameContinued = false;
                Continue();
            }
            else
            {
                DisplaySetupDialog();
            }

            StartCoroutine(GameLoop());
        }
    }

    void InitializeGame()
    {
        if (IsMultiplayerMode)
        {
            InitializeMultiplayerGame();
        }
        else
        {
            InitializeLocalGame();
        }
    }

    void InitializeLocalGame()
    {
        mainDeck.Shuffle();
        marriages = new List<Tuple<Player, Card.Suit>>();
        tempMarriages = new List<Tuple<Player, Card.Suit>>();
        roundNumber = 1;
        firstPlayer = UnityEngine.Random.Range(0, 4);
        currentPlayer = players[firstPlayer];
        MovePlayerToPosition(currentPlayer, Player.Position.down);
        gamePhase = GamePhase.Start;
        DealInitialCards();
        teamScore.Add(0);
        teamScore.Add(0);
    }

    void InitializeMultiplayerGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            mainDeck.Shuffle();
            SendDeck();
        }

        marriages = new List<Tuple<Player, Card.Suit>>();
        tempMarriages = new List<Tuple<Player, Card.Suit>>();
        roundNumber = 1;

        if (PhotonNetwork.IsMasterClient)
        {
            firstPlayer = UnityEngine.Random.Range(0, players.Count);
            photonView.RPC("SyncFirstPlayer", RpcTarget.AllBuffered, firstPlayer);
        }

        MovePlayerToPosition(players[PhotonNetwork.LocalPlayer.ActorNumber - 1], Player.Position.down);

        gamePhase = GamePhase.Start;

        if (PhotonNetwork.IsMasterClient)
        {
            DealInitialCards();
        }

        teamScore.Add(0);
        teamScore.Add(0);
    }

    [PunRPC]
    public void SyncDeck(object[] deckData)
    {
        mainDeck.cards.Clear();

        foreach (object cardData in deckData)
        {
            string cardName = (string)cardData;
            GameObject cardObject = GameObject.Find(cardName);
            if (cardObject != null)
            {
                Card card = cardObject.GetComponent<Card>();
                if (card != null)
                {
                    mainDeck.AddCard(card);
                }
            }
        }
    }

    void SendDeck()
    {
        List<object> deckData = new List<object>();

        foreach (Card card in mainDeck.cards)
        {
            deckData.Add(card.name); // Przesyłaj nazwę karty
        }

        photonView.RPC("SyncDeck", RpcTarget.Others, deckData.ToArray());
    }

    IEnumerator GameLoop()
    {
        yield return new WaitUntil(() => setupFinished);

        while (teamScore[0] < targetScore && teamScore[1] < targetScore)
        {
            GameManager.Instance.runLog.logText("");
            GameManager.Instance.runLog.logText("                     •••  Round " + roundNumber + "  •••", Color.magenta);

            if (IsMultiplayerMode)
            {
                IsRoundSynced = false;

                // master zarzadza rundami
                if (PhotonNetwork.IsMasterClient)
                {
                    if (roundNumber > 1)
                    {
                        mainDeck.Shuffle();
                        SendDeck();
                        DealInitialCards();
                    }

                    yield return StartCoroutine(StartRound());

                    Debug.LogError("Round Ended");
                    photonView.RPC("SyncEndRound", RpcTarget.AllBuffered);  //EndRound();


                    yield return new WaitUntil(() =>
                    {
                        var playersReady = players.Count(p => p.isReady);
                        Debug.LogError("Ready Players: " + playersReady);
                        return playersReady >= 4;
                    });
                    foreach (var player in players)
                        player.SetReady(false);


                    Debug.LogError("All ready, starting next round!");
                }
                else
                {
                    // pozostali czekaja na synchronizacje
                    yield return new WaitUntil(() => IsRoundSynced);
                }
            }
            else
            {
                // Lokalne tryby gry (singleplayer i multiplayer lokalny)
                yield return StartCoroutine(StartRound());
                EndRound();
                //SaveGame();
            }
        }
    }

    [PunRPC]
    public void SyncEndRound()
    {
        DisplayTrumpText();
        CalculateRoundScores();
        if (CheckForGameEnd())
            return;
        roundNumber++;
        ResetDeck();
        ResetCardsVariables();
        trickManager.ClearPlayedCards();

        //firstPlayer = (firstPlayer + 1) % players.Count;
        InputHandler.Instance.ResetCardsToDeal();
        foreach (Player player in players)
        {
            player.Reset();
        }

        auctionFinished = false;
        gameplayFinished = false;

        otherCards.cards.Clear();

        IsRoundSynced = true;



        photonView.RPC("SyncPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, "SetReady", new object[] { true });

        Debug.LogError("READY");
    }

    public void AddInTurnMarriage(Player player, Card.Suit suit)
    {
        tempMarriages.Add(Tuple.Create(player, suit));
    }

    public void AddHandMarriage(Player player, Card.Suit suit)
    {
        tempMarriages.Add(Tuple.Create(player, suit));
        marriages.Add(Tuple.Create(player, suit));
        DisplayTrumpText();
    }

    void DealInitialCards()
    {
        if (IsMultiplayerMode)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int initialCardCount = 5;

                for (int p = 0; p < players.Count; p++)
                {
                    for (int i = p * initialCardCount; i < (p + 1) * initialCardCount; i++)
                    {
                        Card currentCard = mainDeck.cards[i];
                        players[p].AddCardToHand(currentCard);
                    }
                }

                SendPlayerHands();
            }
        }
        else
        {
            // rozdanie kart w trybie lokalnym
            int initialCardCount = 5;

            for (int p = 0; p < players.Count; p++)
            {
                for (int i = p * initialCardCount; i < (p + 1) * initialCardCount; i++)
                {
                    Card currentCard = mainDeck.cards[i];
                    string cardName = "Card_" + currentCard.GetSuitToString() + "_" + currentCard.GetRank();
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

        SaveRestOfTheCards();
    }

    [PunRPC]
    public void SyncAddCardToHand(int playernumber, string cardName)
    {
        Card card = GameObject.Find(cardName).GetComponent<Card>();
        if (card != null)
        {
            players[playernumber].hand.Add(card);
        }
    }


    [PunRPC]
    public void SyncPlayerHands(object[][] handsData)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].hand.Clear();

            foreach (object cardData in handsData[i])
            {
                string cardName = (string)cardData;
                GameObject cardObject = GameObject.Find(cardName);
                if (cardObject != null)
                {
                    cardObject.transform.SetParent(players[i].transform);
                    Card card = cardObject.GetComponent<Card>();
                    if (card != null)
                    {
                        players[i].hand.Add(card);
                        card.SetVisible(players[i].playerNumber == PhotonNetwork.LocalPlayer.ActorNumber);
                        card.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }
        }
    }

    void SendPlayerHands()
    {
        List<object[]> handsData = new List<object[]>();

        foreach (Player player in players)
        {
            List<object> playerHandData = new List<object>();

            foreach (Card card in player.hand)
            {
                playerHandData.Add(card.name);
            }

            handsData.Add(playerHandData.ToArray());
        }

        photonView.RPC("SyncPlayerHands", RpcTarget.AllBuffered, handsData.ToArray());
    }

    private void SaveRestOfTheCards()
    {
        if (IsMultiplayerMode)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int initialCardCount = 5;
                for (int i = players.Count * initialCardCount; i < mainDeck.cards.Count; i++)
                {
                    Card currentCard = mainDeck.cards[i];
                    otherCards.AddCard(currentCard);
                }

                SendRestOfDeck();
            }
        }
        else
        {
            // tryb lokalny
            int initialCardCount = 5;
            for (int i = players.Count * initialCardCount; i < mainDeck.cards.Count; i++)
            {
                Card currentCard = mainDeck.cards[i];
                otherCards.AddCard(currentCard);
                string cardName = "Card_" + currentCard.GetSuitToString() + "_" + currentCard.GetRank();

                GameObject go = GameObject.Find(cardName);
                go.transform.SetParent(restOfTheDeck.transform);
                currentCard.SetVisible(true);
            }
        }
    }

    [PunRPC]
    public void SyncRestOfDeck(object[] restOfDeckData)
    {
        otherCards.cards.Clear();

        foreach (object cardData in restOfDeckData)
        {
            string cardName = (string)cardData;
            GameObject cardObject = GameObject.Find(cardName);
            if (cardObject != null)
            {
                cardObject.transform.SetParent(restOfTheDeck.transform);
                Card card = cardObject.GetComponent<Card>();
                if (card != null)
                {
                    card.SetVisible(false);
                    //card.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    otherCards.AddCard(card);
                }
            }
        }
    }

    void SendRestOfDeck()
    {
        List<object> restOfDeckData = new List<object>();

        foreach (Card card in otherCards.cards)
        {
            restOfDeckData.Add(card.name);
        }

        photonView.RPC("SyncRestOfDeck", RpcTarget.AllBuffered, restOfDeckData.ToArray());
    }

    public void DisplayTrumpText()
    {
        TextMeshProUGUI currentText = GameObject.Find("TrumpText").GetComponent<TextMeshProUGUI>();

        Card.Suit trump = GetAtuSuit();

        if (trump != Card.Suit.None)
        {
            currentText.text = " TRUMP " + trump.ToString() + " " + trump.AsSymbol();
        }
        else
        {
            currentText.text = "";
        }
    }

    void DisplayAuctionDialog()
    {
        if (waitingDialog.activeInHierarchy)
            waitingDialog.SetActive(false);

        auctionDialog.SetActive(true);

        GameObject text = GameObject.Find("CurrentBidText");

        TextMeshProUGUI currentBidText = GameObject.Find("CurrentBidText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI currentWinnerText = GameObject.Find("CurrentWinnerText").GetComponent<TextMeshProUGUI>();

        currentBidText.text = "Current Bid: " + currentBid;
        currentWinnerText.text = "Current winner: " + currentBidder.playerName;
    }

    void DisplayWaitingDialog()
    {
        waitingDialog.SetActive(true);
    }

    [PunRPC]
    public void ShowWaitingForOtherPlayers(string currentPlayerName)
    {
        if (PhotonNetwork.NickName != currentPlayerName)
        {
            DisplayWaitingDialog();
        }
    }

    void DisplayWaitingForCardDialog()
    {
        waitingForCardDialog.SetActive(true);
    }

    [PunRPC]
    public void ShowWaitingForOtherPlayerToDeal(string currentPlayerName)
    {
        if (PhotonNetwork.NickName != currentPlayerName)
        {
            DisplayWaitingForCardDialog();
        }
    }

    [PunRPC]
    public void SyncLog(string logString, float r, float g, float b)
    {
        runLog.logText(logString, new Color(r, g, b));
    }

    public void PositiveAuctionDialog()
    {
        currentBid += 10;
        currentBidder = currentPlayer;
        auctionDialog.SetActive(false);

        if (IsMultiplayerMode)
        {
            do
            {
                currentPlayer = GetNextPlayer(currentPlayer);
                if (!gameRules.CanPlayerBid(currentBid, currentPlayer.hand)){
                    NegativeAuctionDialog();
                }
            } while (currentPlayer.HasPassed());

            photonView.RPC("SyncLog", RpcTarget.AllBuffered, "<" + currentBidder.playerName + "> has bidded " + currentBid + ".", 1.0f, 1.0f, 0.0f);
            photonView.RPC("SyncAuction", RpcTarget.AllBuffered, currentBid, currentPlayer.playerNumber, currentBidder.playerNumber);
        }
        else
        {
            runLog.logText("<" + currentBidder.playerName + "> has bidded " + currentBid + ".", Color.yellow);

            do
            {
                currentPlayer = GetNextPlayer(currentPlayer);
                MovePlayersToNextPositions();
                if (!gameRules.CanPlayerBid(currentBid, currentPlayer.hand)){
                    NegativeAuctionDialog();
                }

            } while (currentPlayer.HasPassed());

            ChangePlayer();
        }
    }


    [PunRPC]
    public void SyncPlayer(int playerNumber, string methodName, object[] parameters)
    {
        players[playerNumber - 1].GetType().GetMethod(methodName).Invoke(players[playerNumber - 1], parameters);
    }   


    public void NegativeAuctionDialog()
    {
        if (IsMultiplayerMode)
        {
            photonView.RPC("SyncPlayer", RpcTarget.AllBuffered, currentPlayer.playerNumber, "SetPassed", new object[] { true });
            photonView.RPC("SyncLog", RpcTarget.AllBuffered, "<" + currentPlayer.playerName + "> passed.", 1.0f, 1.0f, 0.0f);
            //currentPlayer.GetType().GetMethod("SetPassed").Invoke(currentPlayer, parameters); // null, jeśli metoda nie ma parametrów

            do
            {
                currentPlayer = GetNextPlayer(currentPlayer);
                if (!gameRules.CanPlayerBid(currentBid, currentPlayer.hand)){
                    currentPlayer.SetPassed(true);
                }
            } while (currentPlayer.HasPassed());

            photonView.RPC("SyncAuction", RpcTarget.AllBuffered, currentBid, currentPlayer.playerNumber, currentBidder.playerNumber);
        }
        else
        {
            auctionDialog.SetActive(false);

            currentPlayer.SetPassed(true);

            runLog.logText("<" + currentPlayer.playerName + "> passed.", Color.yellow);

            int passed = 0;

            foreach (Player player in players)
            {
                if (player.HasPassed())
                    passed++;
            }

            if (passed >= 3) //Wygrana jednego gracza -> oddanie kart innym graczom
            {
                currentPlayer = currentBidder;
                GameplayCurrentPlayer = currentPlayer;

                runLog.logText("<" + currentPlayer.playerName + "> won auction [" + currentBid + " points].",
                    Color.yellow);

                PrintTrickToLog();

                gamePhase = GamePhase.Handover;

                if (!onePlayerMode) MovePlayerToPosition(currentBidder, Player.Position.down, true);
                ChangePlayer();

            }
            else //Nie wszyscy spasowali -> dilog dla nast�pnego gracza
            {
                do
                {
                    currentPlayer = GetNextPlayer(currentPlayer);
                    MovePlayersToNextPositions();
                    if (!gameRules.CanPlayerBid(currentBid, currentPlayer.hand)){
                        NegativeAuctionDialog();
                    }

                } while (currentPlayer.HasPassed());

                ChangePlayer();
            }
        }
    }

    void DisplayReadyDialog()
    {
        HideAllCards();
        readyDialog.SetActive(true);

        TextMeshProUGUI readyPlayerNameText = GameObject.Find("ReadyPlayerNameText").GetComponent<TextMeshProUGUI>();
        if (gamePhase == GamePhase.Auction || gamePhase == GamePhase.Start)
            readyPlayerNameText.text = currentPlayer.playerName;
        else
            readyPlayerNameText.text = GameplayCurrentPlayer.playerName;
    }

    public void SetPlayerReady()
    {
        readyDialog.SetActive(false);
        UpdateCardVisibility();

        if (gamePhase == GamePhase.Start)
            Auction();
        else if (gamePhase == GamePhase.Auction)
            DisplayAuctionDialog();
        else if (gamePhase == GamePhase.Handover)
        {
            if (onePlayerMode && currentPlayer != players[humanPlayer])
            {
                //StartCoroutine(ShowTrickCards());
                StartCoroutine(BotDealCardsDecision());
            }
            else
            {
                DealCardsToOtherPlayers();
            }
        }
    }

    void DisplaySetupDialog()
    {
        DisplayNicknames(false);
        setupDialog.SetActive(true);
    }

    public void FinishSetupMultiplayer()
    {
        onePlayerMode = false;
        FinishSetup();
    }
    public void FinishSetupSinglePlayer()
    {
        onePlayerMode = true;
        FinishSetup();
    }

    public void FinishSetup()
    {
        string player1Name = GameObject.Find("InputName1Text").GetComponent<TextMeshProUGUI>().text;
        players[0].playerName = player1Name;
        GameObject.Find("NameP1").GetComponent<TextMeshProUGUI>().text = player1Name;

        string player2Name = GameObject.Find("InputName2Text").GetComponent<TextMeshProUGUI>().text;
        players[1].playerName = player2Name;
        GameObject.Find("NameP2").GetComponent<TextMeshProUGUI>().text = player2Name;

        string player3Name = GameObject.Find("InputName3Text").GetComponent<TextMeshProUGUI>().text;
        players[2].playerName = player3Name;
        GameObject.Find("NameP3").GetComponent<TextMeshProUGUI>().text = player3Name;

        string player4Name = GameObject.Find("InputName4Text").GetComponent<TextMeshProUGUI>().text;
        players[3].playerName = player4Name;
        GameObject.Find("NameP4").GetComponent<TextMeshProUGUI>().text = player4Name;

        setupDialog.SetActive(false);
        InitializeGame();
        DisplayNicknames(true);
        setupFinished = true;
    }

    void SetupMultiplayerGame()
    {
        foreach (Photon.Realtime.Player photonPlayer in PhotonNetwork.PlayerList)
        {
            Player player = GameObject.Find("P" + photonPlayer.ActorNumber + " Hand").GetComponent<Player>();

            player.playerName = photonPlayer.NickName;

            GameObject.Find("NameP" + player.playerNumber).GetComponent<TextMeshProUGUI>().text = player.playerName;
        }

        MovePlayerToPosition(players[PhotonNetwork.LocalPlayer.ActorNumber - 1], Player.Position.down);

        InitializeGame();

        setupFinished = true;
    }

    void BotAuctionDecision()
    {
        /* RANDOM DECISION
        int i = UnityEngine.Random.Range(0, 3);
        if (i == 0) PositiveAuctionDialog();
        else NegativeAuctionDialog();
        */
        // AI decision
        List<Card> hand = GetPlayerHand(currentPlayer);
        bool shouldBid = ComputerPlayer.ShouldBid(hand, currentBid);
        if (shouldBid) PositiveAuctionDialog();
        else NegativeAuctionDialog();
    }

    void Auction()
    {
        currentBid = 100;
        currentBidder = GetPreviousPlayer(currentPlayer);
        gamePhase = GamePhase.Auction;

        if (IsMultiplayerMode)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                DisplayAuctionDialog();
                photonView.RPC("ShowWaitingForOtherPlayers", RpcTarget.Others, currentPlayer.playerName);
            }
        }
        else
        {
            MovePlayerToPosition(currentPlayer, Player.Position.down);
            UpdateCardVisibility();

            DisplayAuctionDialog();

            if (onePlayerMode && currentPlayer != players[humanPlayer])
            {
                BotAuctionDecision();
            }
        }
    }

    [PunRPC]
    public void SyncSetupAuction()
    {
        currentBid = 100;
        currentBidder = GetPreviousPlayer(currentPlayer);
        gamePhase = GamePhase.Auction;
    }

    [PunRPC]
    public void SyncAuction(int currentBid, int currentPlayerNumber, int currentBidderPlayerNumber)
    {
        auctionDialog.SetActive(false);
        waitingDialog.SetActive(false);

        this.currentBid = currentBid;
        this.currentPlayer = players[currentPlayerNumber - 1];
        this.currentBidder = players[currentBidderPlayerNumber - 1];


        int passed = 0;
        foreach (Player player in players)
        {
            if (player.HasPassed())
                passed++;
        }
        if (passed >= 3)
        {
            GameplayCurrentPlayer = currentPlayer;

            runLog.logText("<" + currentPlayer.playerName + "> won auction [" + currentBid + " points].",
                Color.yellow);

            gamePhase = GamePhase.Handover;
            return;
        }


        if (PhotonNetwork.LocalPlayer.ActorNumber == currentPlayerNumber)
        {
            DisplayAuctionDialog();
        }
        else
        {
            DisplayWaitingDialog();
        }
    }


    private IEnumerator BotDealCardsDecision()
    {
        // show trick section
        Card[] leftOvers = restOfTheDeck.GetComponentsInChildren<Card>();

        foreach (Card card in leftOvers)
        {
            trickManager.AddCard(card);
            card.GetComponent<SpriteRenderer>().sortingOrder = InputHandler.Instance.sortingOrder;
            InputHandler.Instance.sortingOrder++;
            card.gameObject.transform.SetParent(t.transform);
        }

        yield return new WaitForSeconds(2.5f);

        // move cards from table to winner's hand
        Card[] trickCards = trickManager.GetTrickCards();
        for (int i = 0; i < 4; i++)
        {
            GameObject trickCard = t.transform.GetChild(0).gameObject;
            trickCard.transform.SetParent(currentPlayer.transform);
            currentPlayer.AddCardToHand(trickCards[i]);
        }
        trickManager.ClearTrickCards();
        InputHandler.Instance.sortingOrder = 1;
        UpdateCardVisibility();
        waitingForCardDialog.SetActive(true);

        // handle dealing cards to other players
        List<Card> hand = GetPlayerHand(currentPlayer);
        isGivingStage = true;

        Player pp = currentPlayer;
        for (int i = 0; i < 3; i++)
        {
            currentCardReceiver = GetNextPlayer(pp);
            pp = currentCardReceiver;

            // simulate card click

            // random decision
            //Card card = hand.First();

            // AI decision
            bool isMaximizingPlayer = pp.team == currentPlayer.team;
            Card card = ComputerPlayer.GetCardToDeal(hand, isMaximizingPlayer);

            yield return new WaitForSeconds(0.7f);
            InputHandler.Instance.OnClickHandle(card);
        }
        waitingForCardDialog.SetActive(false);
    }


    void ChangePlayer()
    {
        if (!onePlayerMode && !IsMultiplayerMode && forcePlayerChangeDialog)
        {
            // lokalny multi
            DisplayReadyDialog();
        }
        else
        {
            UpdateCardVisibility();

            if (gamePhase == GamePhase.Start)
            {
                Auction();
            }
            else if (gamePhase == GamePhase.Auction)
            {
                if (onePlayerMode)
                {
                    if (currentPlayer != players[humanPlayer])
                    {
                        BotAuctionDecision();
                    }
                    else
                    {
                        DisplayAuctionDialog();
                    }
                }
                else
                {
                    DisplayAuctionDialog();
                }
            }
            else if (gamePhase == GamePhase.Handover)
            {
                if (onePlayerMode && currentPlayer != players[humanPlayer])
                {
                    StartCoroutine(BotDealCardsDecision());
                }
                else
                {
                    DealCardsToOtherPlayers();
                }
            }
        }
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

    public Card.Suit GetAtuSuit()
    {
        int size = marriages.Count;
        if (size > 0)
        {
            Card.Suit suit = marriages[size - 1].Item2;
            return suit;
        }
        return Card.Suit.None;
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

    public void UpdateMarriageScore()
    {
        foreach (Player player in players)
        {
            foreach (Tuple<Player, Card.Suit> marriage in marriages)
            {
                if (player.playerNumber == marriage.Item1.playerNumber)
                {
                    int score = marriage.Item2.GetMarriageValue();
                    player.AddRoundScore(score);
                }
            }
        }
    }

    private bool IsNewTrickWinner(List<Card> cards)
    {
        int lastCard = cards.Count - 1;
        int maxAtu = -1, max = 0;

        if (cards.Count == 1)
        {
            return true;
        }

        Card.Suit trump = GetAtuSuit();
        if (trump != Card.Suit.None)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].GetSuit() == trump)
                    if (maxAtu == -1) maxAtu = i;
                    else if (cards[i].GetValue() > cards[maxAtu].GetValue())
                    {
                        maxAtu = i;
                    }
            }

            if (maxAtu == -1)
            {
                if (cards[lastCard].GetSuit() == trump)
                {
                    return true;
                }
                else
                {
                    for (int i = 0; i < cards.Count - 1; i++)
                    {
                        if (cards[i].GetValue() > max && cards[0].GetSuitToString() == cards[i].GetSuitToString())
                            max = cards[i].GetValue();
                    }

                    for (int i = 0; i < cards.Count - 1; i++)
                    {
                        if (cards[0].GetSuitToString() == cards[lastCard].GetSuitToString() && max < cards[lastCard].GetValue())
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if (cards[lastCard].GetSuit() == trump && lastCard == maxAtu)
                {
                    return true;
                }
                return false;
            }
        }
        else
        {
            int maxc = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[0].GetSuitToString() == cards[i].GetSuitToString() && cards[i].GetValue() > cards[maxc].GetValue())
                {
                    maxc = i;
                }
            }
            if (maxc == cards.Count - 1)
            {
                return true;
            }
        }
        return false;
    }

    private Card ChooseCardToPlay(Player player)
    {
        List<Card> hand = GetPlayerHand(player);
        List<Card> played = trickManager.GetPlayedCards();
        List<Card> trick = trickManager.GetTrick();
        Card.Suit atu = GetAtuSuit();

        return ComputerPlayer.GetBestCardToPlay(hand, played, trick, atu);
        /*
        List<Card> hand = GetPlayerHand(player);
        Card cardToPlay = hand[0];
        foreach (Card card in hand)
        {
            cardToPlay = card;
            if (InputHandler.Instance.ValidateCardOK(cardToPlay, hand)) break;
        }

        return cardToPlay;
        */
    }

    private IEnumerator DelayedBotMove(Player player)
    {
        // Wait for before making a move
        if(player!=players[humanPlayer+1]) yield return new WaitForSeconds(1.0f);

        InputHandler.Instance.OnClickHandle(ChooseCardToPlay(player));
    }

    private void ClearCurrentPlayerText()
    {
        TextMeshProUGUI currentPlayerText = GameObject.Find("CurrentPlayerText").GetComponent<TextMeshProUGUI>();
        currentPlayerText.text = "";
    }
    private void DisplayCurrentPlayerText()
    {
        if (GameManager.IsMultiplayerMode)
        {
            TextMeshProUGUI currentPlayerText = GameObject.Find("CurrentPlayerText").GetComponent<TextMeshProUGUI>();
            if (PhotonNetwork.LocalPlayer.ActorNumber == GameplayCurrentPlayer.playerNumber)
            {
                currentPlayerText.fontSize = 45;
                currentPlayerText.color = Color.white;
                currentPlayerText.text = "Your move";
            }
            else
            {
                currentPlayerText.fontSize = 25;
                currentPlayerText.color = Color.grey;
                currentPlayerText.text = GameManager.Instance.GameplayCurrentPlayer.playerName + "'s move";
            }
        }
        else
        {
            TextMeshProUGUI currentPlayerText = GameObject.Find("CurrentPlayerText").GetComponent<TextMeshProUGUI>();
            if (GameplayCurrentPlayer == players[humanPlayer])
            {
                currentPlayerText.fontSize = 45;
                currentPlayerText.color = Color.white;
                currentPlayerText.text = "Your move";
            }
            else
            {
                currentPlayerText.fontSize = 25;
                currentPlayerText.color = Color.grey;
                currentPlayerText.text = GameManager.Instance.GameplayCurrentPlayer.playerName + "'s move";
            }
        }
    }

    List<Card> currentTrick = new List<Card>();
    private Player trickWinner;



    [PunRPC]
    public void SyncSetupGameplayTurns(int roundStartingPlayerNumber, int turnIndex) // Przygotowuje rozgrywkę do nowej lewy (4 kart)
    {
        runLog.logText(string.Format("{0,26} • {1}. turn •", " ", turnIndex + 1), Color.grey);

        currentTrick.Clear();
        currentPlayer = players[roundStartingPlayerNumber - 1]; //aka bidding winner or trick winner
        GameplayCurrentPlayer = currentPlayer;
        trickWinner = GameplayCurrentPlayer;
        DisplayCurrentPlayerText();
        DisplayTrumpText();
    }

    [PunRPC]
    public void SyncGameplayTurn(int currentPlayerNumber, string playedCardName)
    {
        StartCoroutine(GameplayTurn(currentPlayerNumber, playedCardName));
    }


    public IEnumerator GameplayTurn(int currentPlayerNumber, string playedCardName)
    {
        Card card = GameObject.Find(playedCardName).GetComponent<Card>();

        if (currentPlayerNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            card.ForceScale();
        }

        InputHandler.Instance.PlayCard(card, players[currentPlayerNumber - 1].hand, players[currentPlayerNumber - 1]);
        Play(card);
        yield return new WaitUntil(() => card.isDotweenAnimEnded);

        players[currentPlayerNumber - 1].hand.Remove(playedCard);
        currentTrick.Add(playedCard);

        if (IsNewTrickWinner(currentTrick))
        {
            trickWinner = GameplayCurrentPlayer;
        }
        GameplayCurrentPlayer = GetNextPlayer(GameplayCurrentPlayer);
        DisplayCurrentPlayerText();

        //Jeśli koniec (wszyscy położyli 1 kartę)
        if (currentTrick.Count == 4)
        {
            GameplayCurrentPlayer = trickWinner;
            yield return new WaitUntil(() => AllCardsReadyForDissolve(currentTrick));
            foreach (Card c in currentTrick)
            {
                c.Dissolve();
            }
            yield return new WaitForSeconds(1.5f);
            EndTurn();
            UpdatePlayerScore(currentTrick, trickWinner);
            DisplayTrumpText();
            currentTrick.Clear();

            photonView.RPC("SyncPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, "SetReady", new object[] { true });
        }
    }

    void PrintTrickToLog()
    {
        Card[] leftOvers = restOfTheDeck.GetComponentsInChildren<Card>();
        Instance.runLog.logText("");
        Instance.runLog.logText("                       • Trick cards •", Color.green);
        foreach (Card card in leftOvers)
        {
            Instance.runLog.logText("Card " + card.GetCardName(), Color.green);
        }
        Instance.runLog.logText("");
    }

    void UpdateMarriageList(){
        marriages.Clear();
        foreach (var m in tempMarriages)
        {
            marriages.Add(m);
        }
    }

    private IEnumerator Gameplay()
    {
        yield return new WaitUntil(() => auctionFinished);

        if (bombPlayed)
        {
            gameplayFinished = true;
            yield break;
        }

        if (IsMultiplayerMode)
        {
            GameplayCurrentPlayer = currentBidder;
            int numberOfTurns = GameplayCurrentPlayer.GetCardsInHand();

            for (int i = 0; i < numberOfTurns; i++)
            {
                photonView.RPC("SyncSetupGameplayTurns", RpcTarget.AllBuffered, GameplayCurrentPlayer.playerNumber, i);

                yield return new WaitUntil(() =>
                {
                    var playersReady = players.Count(p => p.isReady);
                    Debug.LogError("Ready Players: " + playersReady);
                    return playersReady >= 4;
                });
                foreach (var player in players)
                    player.SetReady(false);

                Debug.LogError("Lewa sie skonczyla");

                photonView.RPC("SyncEndGameplayTurns", RpcTarget.AllBuffered, players[0].GetRoundScore(), players[1].GetRoundScore(), players[2].GetRoundScore(), players[3].GetRoundScore());

                yield return new WaitUntil(() =>
                {
                    var playersReady = players.Count(p => p.isReady);
                    Debug.LogError("Ready Players: "+ playersReady);
                    return playersReady >= 4;
                });
                foreach (var player in players)
                    player.SetReady(false);
            }

            Debug.LogError("Runda sie skonczyla");
            // Koniec rundy (wszyscy mają puste ręce)

            photonView.RPC("SyncEndGameplayRound", RpcTarget.AllBuffered);
        }
        else
        {
            GameplayCurrentPlayer = currentPlayer;
            Player trickWinner = currentBidder;
            List<Card> currentTrick = new List<Card>();

            gameplayFinished = false;

            int numberOfTurns = GameplayCurrentPlayer.GetCardsInHand();

            for (int i = 0; i < numberOfTurns; i++)
            {
                Instance.runLog.logText(string.Format("{0,26} • {1}. turn •", " ", i + 1), Color.grey);
                for (int j = 0; j < players.Count; j++)
                {
                    //HERE
                    if (!onePlayerMode)
                        ChangePlayer();

                    if (onePlayerMode) DisplayCurrentPlayerText();
                    if (onePlayerMode && GameplayCurrentPlayer != players[humanPlayer])
                    {
                        StartCoroutine(DelayedBotMove(GameplayCurrentPlayer));
                    }
                    yield return new WaitUntil(() => played);
                    played = false;
                    currentTrick.Add(playedCard);
                    if (IsNewTrickWinner(currentTrick))
                    {
                        trickWinner = GameplayCurrentPlayer;
                    }
                    GameplayCurrentPlayer = GetNextPlayer(GameplayCurrentPlayer);
                }
                if (!onePlayerMode) HideAllCards();
                yield return new WaitUntil(() => AllCardsReadyForDissolve(currentTrick));
                foreach (Card card in currentTrick)
                {
                    card.Dissolve();
                }
                yield return new WaitForSeconds(1.5f);
                EndTurn();
                UpdateMarriageList();
                UpdatePlayerScore(currentTrick, trickWinner);
                currentTrick.Clear();
                DisplayTrumpText();
                int playerNum = GameplayCurrentPlayer.playerNumber;
                GameplayCurrentPlayer = trickWinner;
                if (!onePlayerMode) MovePlayerToPosition(trickWinner, Player.Position.down, true); UpdateCardVisibility();
            }
            if (onePlayerMode) yield return new WaitForSeconds(0.9f);
            //UpdateMarriageScore();
            marriages.Clear();
            tempMarriages.Clear();
            ClearCurrentPlayerText();

            gameplayFinished = true;
        }
    }

    [PunRPC]
    public void SyncEndGameplayTurns(int scorePlayer1, int scorePlayer2, int scorePlayer3, int scorePlayer4)
    {
        players[0].SetRoundScore(scorePlayer1);
        players[1].SetRoundScore(scorePlayer2);
        players[2].SetRoundScore(scorePlayer3);
        players[3].SetRoundScore(scorePlayer4);
        UpdateMarriageList();
        DisplayTrumpText();

        photonView.RPC("SyncPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber, "SetReady", new object[] { true });
    }


    [PunRPC]
    public void SyncEndGameplayRound()
    {
        //UpdateMarriageScore();
        marriages.Clear();
        tempMarriages.Clear();
        ClearCurrentPlayerText();
        gameplayFinished = true;
    }

    private bool AllCardsReadyForDissolve(List<Card> cTrick)
    {
        foreach (Card card in cTrick)
        {
            if (!card.readyForDissolve)
            {
                return false;
            }
        }
        return true;
    }

    private void EndTurn()
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject trickCard = t.transform.GetChild(0).gameObject;
            Vector3 p = new Vector3(500, 300, 69);
            trickCard.transform.position = p;
            trickCard.transform.SetParent(null);
            foreach (Card card in trickManager.trickCards)
            {
                card.ResetDissolve();
            }
            trickManager.ClearTrickCards();
            InputHandler.Instance.sortingOrder = 1;
        }
    }

    public void Play(Card card)
    {
        playedCard = card;
        played = true;
    }

    private bool bombPlayed = false;

    void DisplayHandoverDialog()
    {
        handOverDialog.SetActive(true);
        // hide bomb button
        if (BombButton == null)
        {
            Debug.LogError("BombButton not found!");
        }
        else
        {
            if (gameRules.CanPlayerUseBomb(currentPlayer.playerNumber - 1))
            {
                BombButton.SetActive(true);
            }
            else
            {
                BombButton.SetActive(false);
            }
        }
    }

    void DealCardsToOtherPlayers()
    {
        if (IsMultiplayerMode)
        {
            if (PhotonNetwork.NickName == currentPlayer.playerName)
            {
                //handOverDialog.SetActive(true);
                DisplayHandoverDialog();
                isGivingStage = true;
                currentCardReceiver = GetNextPlayer(currentPlayer);

                TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
                currentCardReceiverText.text = "Choose card for player: " + currentCardReceiver.playerName;

                photonView.RPC("ShowWaitingForOtherPlayerToDeal", RpcTarget.Others, currentPlayer.playerName);
            }
        }
        else
        {
            DisplayHandoverDialog();
            isGivingStage = true;
            currentCardReceiver = GetNextPlayer(currentPlayer);

            TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
            currentCardReceiverText.text = "Choose card for player: " + currentCardReceiver.playerName;
        }
    }

    public void EndDealingStage()
    {
        isGivingStage = false;
        handOverDialog.SetActive(false);
        waitingForCardDialog.SetActive(false);
        auctionFinished = true;
        gamePhase = GamePhase.Gameplay;
    }

    public void BombEndRound(){
        bombPlayed = true;
        int pointsToGive = RoundToNearestTen(currentBid / 2);
        teamScore[currentBidder.GetTeam() % 2] += pointsToGive;

        runLog.logText("<" + currentPlayer.playerName + "> declared bomb ", Color.blue);

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

        isGivingStage = false;
        ResetDeck();
        handOverDialog.SetActive(false);
        waitingForCardDialog.SetActive(false);
        gamePhase = GamePhase.Gameplay;
        auctionFinished = true;
    }

    IEnumerator StartRound()
    {
        firstPlayer = (firstPlayer + 1) % players.Count;
        currentPlayer = players[firstPlayer];
        gamePhase = GamePhase.Start;

        if (IsMultiplayerMode && PhotonNetwork.IsMasterClient)
        {
            //Auction
            photonView.RPC("SyncFirstPlayer", RpcTarget.AllBuffered, firstPlayer);
            photonView.RPC("SyncResetCardsDissolve", RpcTarget.AllBuffered);
            photonView.RPC("SyncSetupAuction", RpcTarget.AllBuffered);
            photonView.RPC("SyncAuction", RpcTarget.AllBuffered, currentBid, currentPlayer.playerNumber, currentBidder.playerNumber);

            yield return new WaitUntil(() => gamePhase == GamePhase.Handover);

            //Hand Over
            photonView.RPC("SyncSetupHandOver", RpcTarget.AllBuffered);

            yield return new WaitUntil(() => gamePhase == GamePhase.Gameplay);
        }
        else
        {
            ChangePlayer();
        }

        if (!bombPlayed)
        {
            StartCoroutine(Gameplay());
        }

        yield return new WaitUntil(() => gameplayFinished);

    }

    [PunRPC]
    public void SyncSetupHandOver()
    {
        var isGivingPlayer = PhotonNetwork.LocalPlayer.ActorNumber == currentBidder.playerNumber;

        PrintTrickToLog();

        handOverDialog.SetActive(isGivingPlayer);
        waitingForCardDialog.SetActive(!isGivingPlayer);
        isGivingStage = isGivingPlayer;
        currentCardReceiver = GetNextPlayer(currentPlayer);

        foreach (var card in otherCards.cards)
        {
            card.gameObject.SetActive(true);
            card.SetVisible(isGivingPlayer);
        }

        if (isGivingPlayer)
        {
            TextMeshProUGUI currentCardReceiverText =
                GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
            currentCardReceiverText.text = "Choose card for player: " + currentCardReceiver.playerName;
        }
    }

    [PunRPC]
    public void SyncDistributeCardToPlayer(string cardName)
    {
        Card card = null;
        Card[] allCards = FindObjectsOfType<Card>(true);
        foreach (Card c in allCards)
        {
            string parentName = c.transform.parent != null
                ? c.transform.parent.gameObject.name
                : "NO PARENT";

            if (c.gameObject.name == cardName)
            {
                card = c;
                break;
            }
        }
        
        //Card card = GameObject.Find(cardName).GetComponent<Card>();

        currentCardReceiver.AddCardToHand(card);

        if (currentPlayer.hand.Contains(card))
            currentPlayer.hand.Remove(card);

        card.gameObject.transform.SetParent(currentCardReceiver.transform);
        card.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        card.SetVisible(PhotonNetwork.LocalPlayer.ActorNumber == currentCardReceiver.playerNumber);
        otherCards.cards.Remove(card);

        InputHandler.Instance.cardsToDeal--;
        currentCardReceiver = GetNextPlayer(currentCardReceiver);

        if (InputHandler.Instance.cardsToDeal == 1)
        {
            AddRestToCurrentPlayer();
            EndDealingStage();
            return;
        }

        TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
        currentCardReceiverText.text = "Choose card for player: " + currentCardReceiver.playerName;
    }


    [PunRPC]
    public void SyncFirstPlayer(int firstPlayerIndex)
    {
        firstPlayer = firstPlayerIndex;
        currentPlayer = players[firstPlayer];
    }

    [PunRPC]
    public void SyncResetCardsDissolve()
    {
        foreach (var player in players)
        {
            foreach (var card in player.hand)
            {
                card.ResetDissolve();
            }
        }

        foreach (var card in currentTrick)
        {
            card.ResetDissolve();
        }
    }


    private void ResetCardsVariables()
    {
        foreach (Card card in mainDeck.cards)
        {
            card.ResetCard();
        }
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
        DisplayTrumpText();
        CalculateRoundScores();
        if (bombPlayed)
            bombPlayed = false;
        roundNumber++;
        ResetDeck();
        ResetCardsVariables();
        trickManager.ClearPlayedCards();

        //firstPlayer = (firstPlayer + 1) % players.Count;
        SaveGame();
        if (CheckForGameEnd())
            return;
        InputHandler.Instance.ResetCardsToDeal();
        foreach (Player player in players)
        {
            player.Reset();
        }

        auctionFinished = false;
        gameplayFinished = false;

        otherCards.cards.Clear();

        //MovePlayerToPosition(players[firstPlayer], Player.Position.down);
        // przygotowanie na nastepna runde
        //tutaj powinnismy karty wlozyc do talii znowu
        mainDeck.Shuffle();
        //AudioManager.Instance.PlayDealCardSound();
        DealInitialCards();
    }

    int RoundToNearestTen(int number)
    {
        int remainder = number % 10;
        if (remainder <= 5)
        {
            return number - remainder;
        }
        else
        {
            return number + (10 - remainder);
        }
    }

    void CalculateRoundScores()
    {
        if (!bombPlayed)
        {
            int bidderTeam = currentBidder.GetTeam();
            List<int> tempTeamScore = new List<int>();
            tempTeamScore.Add(0);
            tempTeamScore.Add(0);

        foreach (Player player in players)
        {
            tempTeamScore[player.GetTeam() - 1] += player.GetRoundScore();
            foreach ((Player p, Card.Suit suit) in marriages)
            {
                if (p == player)
                {
                    tempTeamScore[player.GetTeam() - 1] += suit.GetMarriageValue();
                }
            }
        }
        marriages.Clear();
        tempMarriages.Clear();

            tempTeamScore[0] = RoundToNearestTen(tempTeamScore[0]);
            tempTeamScore[1] = RoundToNearestTen(tempTeamScore[1]);

            for (int i = 0; i < 2; i++)
            {
                if (i == bidderTeam - 1)
                {
                    if (tempTeamScore[bidderTeam - 1] < currentBid)
                    {
                        GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> scores " + tempTeamScore[i] + " points", Color.red);
                        GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> lost round. [-" + currentBid + " points]", Color.red);
                        teamScore[i] -= currentBid;
                    }
                    else
                    {
                        GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> scores " + tempTeamScore[i] + " points", Color.green);
                        GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> won round.", Color.green);
                        teamScore[i] += tempTeamScore[i];
                    }
                }
                else
                {
                    GameManager.Instance.runLog.logText("<Team " + (i + 1) + "> scores " + tempTeamScore[i] + " points", Color.yellow);
                    teamScore[i] += tempTeamScore[i];
                }
            }
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
            player.SetRoundScore(0);
            player.ClearHand();
        }
    }

    bool CheckForGameEnd()
    {
        // Win
        if (teamScore[0] >= targetScore || teamScore[1] >= targetScore)
        {
            AudioManager.Instance.PlayWinSound();
            endGameDialog.SetActive(true);
            TextMeshProUGUI winnerTeam = GameObject.Find("WinnerTeam").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI winner1 = GameObject.Find("Winner1").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI winner2 = GameObject.Find("Winner2").GetComponent<TextMeshProUGUI>();

            if (teamScore[0] > teamScore[1])
            {
                Debug.Log("Team 1 wins!");
                // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
                winnerTeam.text = "Team 1";
                winner1.text = players[0].playerName;
                winner2.text = players[2].playerName;
            }
            else
            {
                Debug.Log("Team 2 wins!");
                // tutaj jakas logika zakonczenia np. wyswietlenie obrazu kto wygral i jakies opcje np powrot do menu czy reset rozgrywki
                winnerTeam.text = "Team 2";
                winner1.text = players[1].playerName;
                winner2.text = players[3].playerName;
            }

            FileInfo file = new FileInfo(savePath);
            Debug.Log(file.Exists);
            if (file.Exists)
            {
                file.Delete();
            }

            return true;
        }

        return false;
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

    public List<Card> GetPlayerHand(Player current)
    {
        List<Card> hand = null;
        foreach (Player player in GameManager.Instance.players)
        {
            if (player == current)
            {
                return player.hand;
            }
        }
        return hand;
    }

    public void MovePlayersToNextPositions()
    {
        if (!IsMultiplayerMode)
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
    }

    public void MovePlayerToPosition(Player playerPivot, Player.Position positionPivot, bool moveOtherPlayers = true)
    {
        if (onePlayerMode) return;
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

    public void AddRestToCurrentPlayer()
    {
        Card[] leftOvers = restOfTheDeck.GetComponentsInChildren<Card>();
        if (leftOvers.Length != 0)
        {
            foreach (Card card in leftOvers)
            {
                currentPlayer.AddCardToHand(card);
                card.gameObject.transform.SetParent(currentPlayer.transform);
                card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
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
                        card.transform.localRotation = Quaternion.Euler(0, 0, 0);
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
                        card.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }

            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }

        foreach (Transform cardTransform in t.transform)
        {
            Card card = cardTransform.GetComponent<Card>();
            if (card != null)
            {
                card.SetVisible(true);
                card.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    public void HideAllCards()
    {
        for (int i = 0; i < players.Count; i++)
        {
            GameObject playerObject = players[i].gameObject;

            foreach (Transform cardTransform in playerObject.transform)
            {
                Card card = cardTransform.GetComponent<Card>();
                if (card != null)
                {
                    card.SetVisible(false);
                    card.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
    }

    public void DisplayNicknames(bool display)
    {
        foreach (GameObject obj in nickNames)
            obj.SetActive(display);
    }
    public void SaveGame()
    {
        DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(savePath));
        if (!dir.Exists)
            dir.Create();

        using (StreamWriter sw = File.CreateText(savePath))
        {
            if (!onePlayerMode)
                sw.WriteLine($"{roundNumber} {firstPlayer} 0");
            else
                sw.WriteLine($"{roundNumber} {firstPlayer} 1");

            foreach (var player in players)
            {
                sw.WriteLine($"{player.playerNumber} {player.playerName} {player.team} {player.GetScore()}");
            }
        }
    }
    public void LoadGame()
    {
        try
        {
            using (StreamReader sr = new StreamReader(savePath))
            {
                string line;

                line = sr.ReadLine();
                if (line == null)
                {
                    Debug.LogError("Trying to read from empty file");
                    return;
                }

                string[] lineSplit = line.Split(' ');
                roundNumber = int.Parse(lineSplit[0]);
                firstPlayer = int.Parse(lineSplit[1]);

                currentPlayer = players[firstPlayer];
                if (lineSplit[2] == "0")
                    onePlayerMode = false;
                else
                    onePlayerMode = true;

                while ((line = sr.ReadLine()) != null)
                {
                    lineSplit = line.Split(' ');
                    int pNum = int.Parse(lineSplit[0]);
                    string pName = lineSplit[1];
                    int team = int.Parse(lineSplit[2]);
                    int points = int.Parse(lineSplit[3]);

                    players[pNum - 1].playerName = pName;
                    players[pNum - 1].team = team;
                    players[pNum - 1].SetScore(points);
                    teamScore[team - 1] = points;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }

    public void Continue()
    {
        setupDialog.SetActive(false);
        InitializeGame();
        LoadGame();
        DisplayNicknames(true);
        setupFinished = true;
    }

    #region DisconnectionHandling

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        PhotonNetwork.LeaveRoom();
        IsMultiplayerMode = false;
        SceneManager.LoadScene("Menu");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        IsMultiplayerMode = false;
        SceneManager.LoadScene("Menu");
    }

    private void OnApplicationQuit()
    {
        HandleSomeoneDisconnection();
    }

    private void OnDestroy()
    {
        HandleSomeoneDisconnection();
    }

    private void HandleSomeoneDisconnection()
    {
        if (IsMultiplayerMode)
        {
            PhotonNetwork.RaiseEvent(EventCode.SomeoneDisconnected, null, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
    }

    private void OnEventReceived(EventData photonEvent)
    {
        if (photonEvent.Code == EventCode.SomeoneDisconnected)
        {
            PhotonNetwork.LeaveRoom();
            IsMultiplayerMode = false;
            SceneManager.LoadScene("Menu");
        }
    }

    private static class EventCode
    {
        public const byte SomeoneDisconnected = 1;
    }

    #endregion

}