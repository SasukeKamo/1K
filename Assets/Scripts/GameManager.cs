using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            // Load the Menu scene
            SceneManager.LoadScene("Menu");
        }
    }

    public List<Player> players;
    public enum GamePhase { Start, Auction, Handover, Gameplay };

    public Deck mainDeck;
    public Deck otherCards;
    private int roundNumber;
    private List<int> teamScore = new List<int>(); // teamScore[0]: team1, teamScore[1]: team2 etc.
    const int targetScore = 1000;
    private List<Tuple<Player, Card.Suit>> marriages;
    private int currentBid;
    private Player currentBidder; // Player who is winning the auction at the moment
    public Player currentPlayer; // Player who is making any move at the moment
    public Player currentCardReceiver;  // Player who is being given a card at the moment
    private int firstPlayer = 0;    // Player who is starting the round
    private bool played;
    private Card playedCard;
    public bool isGivingStage = false;
    public RunLog runLog;
    private bool gameplayFinished = false;
    public bool auctionFinished = false;
    public bool setupFinished = false;
    public GamePhase gamePhase;
    public string savePath = "save.txt";
    [SerializeField] private bool forcePlayerChangeDialog;
    [SerializeField] private GameObject t;
    [SerializeField] private GameObject downPlace;
    [SerializeField] private GameObject leftPlace;
    [SerializeField] private GameObject upPlace;
    [SerializeField] private GameObject rightPlace;
    [SerializeField] private GameObject auctionDialog;
    [SerializeField] private GameObject readyDialog;
    [SerializeField] private GameObject setupDialog;
    [SerializeField] private GameObject handOverDialog;
    [SerializeField] private GameObject restOfTheDeck;
    [SerializeField] private GameObject[] nickNames;

    void Start()
    {
        runLog = _instance.GetComponent<RunLog>();
        //InitializeGame();
        DisplaySetupDialog();
        StartCoroutine(GameLoop());
    }

    void InitializeGame()
    {
        mainDeck.Shuffle();
        Debug.Log("Cards shuffled.");
        marriages = new List<Tuple<Player, Card.Suit>>();
        roundNumber = 1;
        firstPlayer = UnityEngine.Random.Range(0, 4);
        currentPlayer = players[firstPlayer];
        MovePlayerToPosition(currentPlayer, Player.Position.down);
        gamePhase = GamePhase.Start;
        DealInitialCards();
        teamScore.Add(0);
        teamScore.Add(0);
        Debug.Log($"{teamScore[0]}, {teamScore[1]}");

    }

    IEnumerator GameLoop()
    {
        yield return new WaitUntil(() => setupFinished);

        //LoadGame(); //only for testing

        while (teamScore[0] < targetScore && teamScore[1] < targetScore)
        {
            Debug.Log("Round Started");
            yield return StartCoroutine(StartRound());
            EndRound();
            Debug.Log("Round Ended");
            SaveGame();
            Debug.Log("Game Saved");

        }
    }

    public void AddMarriage(Player player, Card.Suit suit){
        marriages.Add(Tuple.Create(player, suit));
    }

    void DealInitialCards()
    {
        int initialCardCount = 5;

        for (int p = 0; p < players.Count; p++)
        {
            for (int i = p * initialCardCount; i < (p + 1) * initialCardCount; i++)
            {
                Card currentCard = mainDeck.cards[i];
                string cardName = "Card_" + currentCard.GetSuitToString() + "_" + currentCard.GetRank();
                //Debug.Log(cardName);
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
            string cardName = "Card_" + currentCard.GetSuitToString() + "_" + currentCard.GetRank();

            GameObject go = GameObject.Find(cardName);
            go.transform.SetParent(restOfTheDeck.transform);
            currentCard.SetVisible(true);
        }
    }

    void DisplayTrumpText(){
        TextMeshProUGUI currentText = GameObject.Find("TrumpText").GetComponent<TextMeshProUGUI>();

        Card.Suit trump = GetAtuSuit();

        if (trump != Card.Suit.None){
            currentText.text = " TRUMP " + trump.ToString();
        }
        else{
            currentText.text = "";
        }
    }

    void DisplayAuctionDialog()
    {
        auctionDialog.SetActive(true);

        GameObject text = GameObject.Find("CurrentBidText");

        TextMeshProUGUI currentBidText = GameObject.Find("CurrentBidText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI currentWinnerText = GameObject.Find("CurrentWinnerText").GetComponent<TextMeshProUGUI>();

        currentBidText.text = "CURRENT BID: " + currentBid;
        currentWinnerText.text = "CURRENT WINNER: " + currentBidder.playerName;
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

        ChangePlayer();
    }
    public void NegativeAuctionDialog()
    {
        auctionDialog.SetActive(false);

        currentPlayer.SetPassed(true);

        runLog.logText("<" + currentPlayer.playerName + "> passed.", Color.yellow);

        int passed = 0;

        foreach(Player player in players)
        {
            if (player.HasPassed())
                passed++;
        }

        if (passed >= 3) //Wygrana jednego gracza -> oddanie kart innym graczom
        {
            currentPlayer = currentBidder;

            Debug.Log(currentBidder.playerName + " wins the auction with a bid of " + currentBid + " points.");
            runLog.logText("<" + currentPlayer.playerName + "> won auction [" + currentBid + " points].", Color.yellow);

            gamePhase = GamePhase.Handover;

            MovePlayerToPosition(currentBidder, Player.Position.down, true);
            ChangePlayer();

        }
        else //Nie wszyscy spasowali -> dilog dla nast�pnego gracza
        {
            do
            {
                currentPlayer = GetNextPlayer(currentPlayer);
                MovePlayersToNextPositions();

            } while (currentPlayer.HasPassed());

            ChangePlayer();
        }
    }

    void DisplayReadyDialog()
    {
        HideAllCards();
        readyDialog.SetActive(true);

        TextMeshProUGUI readyPlayerNameText = GameObject.Find("ReadyPlayerNameText").GetComponent<TextMeshProUGUI>();
        readyPlayerNameText.text = currentPlayer.playerName;
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
            DealCardsToOtherPlayers();
    }

    void DisplaySetupDialog()
    {
        DisplayNicknames(false);
        setupDialog.SetActive(true);
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

    void Auction()
    {
        currentBid = 100;
        currentBidder = GetPreviousPlayer(currentPlayer);
        gamePhase = GamePhase.Auction;

        DisplayAuctionDialog();
    }

    void ChangePlayer()
    {
        //currentPlayer = GetNextPlayer(currentPlayer);
        //MovePlayerToPosition(currentPlayer, Player.Position.down);

        if (forcePlayerChangeDialog)
        {
            DisplayReadyDialog();
        }
        else
        {
            UpdateCardVisibility();

            if (gamePhase == GamePhase.Start)
                Auction();
            else if (gamePhase == GamePhase.Auction)
                DisplayAuctionDialog();
            else if (gamePhase == GamePhase.Handover)
                DealCardsToOtherPlayers();
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
        if (size > 0){
            Card.Suit suit = marriages[size-1].Item2;
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

    private void UpdateMarriageScore(){
        foreach(Player player in players){
            foreach(Tuple<Player, Card.Suit> marriage in marriages){
                if(player.playerNumber == marriage.Item1.playerNumber){
                    int score = marriage.Item2.GetValue();
                    player.AddRoundScore(score);
                }
            }
        }
    }

    private bool IsNewTrickWinner(List<Card> cards)
    {
        int lastCard = cards.Count - 1;
        int maxAtu = -1, max = 0;

        if(cards.Count == 1){
            return false;
        }
            
        Card.Suit trump = GetAtuSuit();
        if (trump != Card.Suit.None){
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].GetSuit() == trump)
                    if (maxAtu == -1) maxAtu = i;
                    else if (cards[i].GetValue() > cards[maxAtu].GetValue()){
                        maxAtu = i;
                    }
            }

            if(maxAtu == -1){
                if (cards[lastCard].GetSuit() == trump){
                    return true;
                }
                else{
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
            else {
                if (cards[lastCard].GetSuit() == trump && lastCard == maxAtu){
                    return true;
                }
                return false;
            }
        }
        else{
            int maxc = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[0].GetSuitToString() == cards[i].GetSuitToString() && cards[i].GetValue() > cards[maxc].GetValue()){
                    maxc = i;
                }   
            }
            if (maxc == cards.Count - 1) {
                return true;
            }
        }
        return false;
    }

    private IEnumerator Gameplay()
    {
        yield return new WaitUntil(() => auctionFinished);

        //Debug.Log("Gameplay started");

        Player currentPlayer = currentBidder;
        Player trickWinner = currentBidder;
        List<Card> currentTrick = new List<Card>();

        gameplayFinished = false;

        int numberOfTurns = currentPlayer.GetCardsInHand();

        for (int i = 0; i < numberOfTurns; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                yield return new WaitUntil(() => played);
                played = false;
                currentTrick.Add(playedCard);
                if (IsNewTrickWinner(currentTrick))
                {
                    trickWinner = currentPlayer;
                }
                currentPlayer = GetNextPlayer(currentPlayer);
            }
            UpdatePlayerScore(currentTrick, trickWinner);
            currentTrick.Clear();
            DisplayTrumpText();
            int playerNum = currentPlayer.playerNumber;
            currentPlayer = trickWinner;
            MovePlayerToPosition(trickWinner, Player.Position.down, true);
            UpdateCardVisibility();
        }
        UpdateMarriageScore();
        marriages.Clear();

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
        firstPlayer = (firstPlayer + 1) % players.Count;
        currentPlayer = players[firstPlayer];
        MovePlayerToPosition(currentPlayer, Player.Position.down);
        gamePhase = GamePhase.Start;

        ChangePlayer();
        //Auction();
        //DealCardsToOtherPlayers();
        StartCoroutine(Gameplay());
        yield return new WaitUntil(() => gameplayFinished);
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
        Debug.Log("Ending Round " + roundNumber);
        DisplayTrumpText();
        CalculateRoundScores();
        CheckForGameEnd();
        roundNumber++;
        ResetDeck();
        ResetCardsVariables();

        firstPlayer = (firstPlayer + 1) % players.Count;
        //Debug.Log(firstPlayer);
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
        int bidderTeam = currentBidder.GetTeam();
        List<int> tempTeamScore = new List<int>();
        tempTeamScore.Add(0);
        tempTeamScore.Add(0);

        foreach (Player player in players){
            tempTeamScore[player.GetTeam() - 1] += player.GetRoundScore();
            foreach((Player p, Card.Suit suit) in marriages){
                if(p==player){
                    tempTeamScore[player.GetTeam() - 1] += suit.GetValue();
                }
            }
        }
        marriages.Clear();

        tempTeamScore[0]=RoundToNearestTen(tempTeamScore[0]);
        tempTeamScore[1]=RoundToNearestTen(tempTeamScore[1]);

        for (int i=0;i<2;i++){
            if(i == bidderTeam-1){
                if (tempTeamScore[bidderTeam-1] < currentBid)
                {
                    GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> scores " + tempTeamScore[i]);
                    GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> lost round. [-" + currentBid + " points]");
                    teamScore[i]-=currentBid;
                }
                else
                {
                    GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> scores " + tempTeamScore[i]);
                    GameManager.Instance.runLog.logText("<Team " + bidderTeam + "> won round.");
                    teamScore[i]+=tempTeamScore[i];
                }
            }
            else{
                GameManager.Instance.runLog.logText("<Team " + (i+1) + "> scores " + tempTeamScore[i]);
                teamScore[i]+=tempTeamScore[i];
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

    public void AddRestToCurrentPlayer()
    {
        Card[] leftOvers = restOfTheDeck.GetComponentsInChildren<Card>();
        if (leftOvers.Length != 0)
        {
            foreach (Card card in leftOvers)
            {
                currentPlayer.AddCardToHand(card);
                card.gameObject.transform.SetParent(currentPlayer.transform);
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
    }

    public void HideAllCards ()
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
        using (StreamWriter sw = File.CreateText(savePath))
        {
            sw.WriteLine($"{roundNumber} {firstPlayer}");
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

                while ((line = sr.ReadLine()) != null)
                {
                    lineSplit = line.Split(' ');
                    int pNum = int.Parse(lineSplit[0]);
                    string pName = lineSplit[1];
                    int team = int.Parse(lineSplit[2]);
                    int points = int.Parse(lineSplit[3]);

                    players[pNum-1].playerName = pName;
                    players[pNum - 1].team = team;
                    players[pNum - 1].SetScore(points);
                    teamScore[team-1] = points;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        
    }
}