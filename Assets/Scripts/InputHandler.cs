using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameObject trick;
    private int sortingOrder = 1;
    private int cardsToDeal = 4;

    private static InputHandler _instance;

    public static InputHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("InputHandler");
                _instance = go.AddComponent<InputHandler>();
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


    private IEnumerator DisplayWrongMoveText(){
        if(GameManager.Instance.onePlayerMode && GameManager.Instance.GameplayCurrentPlayer == GameManager.Instance.players[GameManager.humanPlayer]){
            TextMeshProUGUI text = GameObject.Find("WrongMoveText").GetComponent<TextMeshProUGUI>();
            text.text = "illegal move";
            yield return new WaitForSeconds(0.5f);
            text.text = "";
        }
        else if(!GameManager.Instance.onePlayerMode){
            TextMeshProUGUI text = GameObject.Find("WrongMoveText").GetComponent<TextMeshProUGUI>();
            text.text = "illegal move";
            yield return new WaitForSeconds(0.5f);
            text.text = "";
        }
        
    }

    public bool ValidateCardOK(Card clickedCard, List<Card> hand) {
        Card[] trickCards = trick.GetComponentsInChildren<Card>();
        if (trickCards.Length > 0) {
            Card baseCard = trickCards[0];

            // Rule 1. Same suit
            if (clickedCard.GetSuitToString() == baseCard.GetSuitToString()) {
                // Rule 2. Overtrump
                if (clickedCard.GetValue() > baseCard.GetValue()){
                    return true;
                }
                else {
                    // verify player can overtrump

                    // Case 1: Player has card of current suit
                    Card trickWinner = trickCards[0];
                    foreach (Card card in trickCards){
                        if(card.GetSuitToString() == trickWinner.GetSuitToString() && card.GetValue() > trickWinner.GetValue()){
                            trickWinner = card;
                        }
                    }

                    foreach (Card card in hand){
                        if(card.GetSuitToString() == trickWinner.GetSuitToString() && card.GetValue() > trickWinner.GetValue()){
                            Debug.LogWarning("Invalid move. Need to overtrump with higher value of " + baseCard.GetSuitToString() + "!");
                            StartCoroutine(DisplayWrongMoveText());
                            return false;
                        }
                    }
                }
            }
            else {
                // Player has cards of the current suit
                foreach (Card card in hand){
                    if(card.GetSuitToString() == baseCard.GetSuitToString()){
                            Debug.LogWarning("Invalid move. Need to lay card of " + baseCard.GetSuitToString() + "!");
                            StartCoroutine(DisplayWrongMoveText());
                            return false;
                        }
                }

                // Player can overtrump with atu
                Card.Suit currentAtuSuit = GameManager.Instance.GetAtuSuit();
                if (currentAtuSuit != Card.Suit.None){
                    List<Card> handTrumps = new List<Card>();
                    foreach (Card card in hand){
                        if (card.GetSuit() == currentAtuSuit){
                            handTrumps.Add(card);
                        }
                    }
                    Card highestPlayerAtu;
                    if(handTrumps.Count > 0){
                        highestPlayerAtu = handTrumps[0];
                        foreach (Card card in handTrumps){
                            if (card.GetValue() > highestPlayerAtu.GetValue()){
                                highestPlayerAtu = card;
                            }
                        }
                    }
                    else return true;

                    List<Card> trickTrumps = new List<Card>();
                    foreach (Card card in trickCards){
                        if (card.GetSuit() == currentAtuSuit){
                            trickTrumps.Add(card);
                        }
                    }
                    Card highestTrickAtu;
                    if(trickTrumps.Count > 0){
                        highestTrickAtu = trickTrumps[0];
                        foreach (Card card in trickTrumps){
                            if (card.GetValue() > highestTrickAtu.GetValue()){
                                highestTrickAtu = card;
                            }
                        }
                    }
                    else {
                        if(clickedCard.GetSuit() == currentAtuSuit){
                            return true;
                        }
                        else {
                            Debug.Log("Invalid move. Player has to play with trump!");
                            StartCoroutine(DisplayWrongMoveText());
                            return false;
                        }
                    }
                    if (highestPlayerAtu.GetValue() > clickedCard.GetValue()){
                        Debug.Log("Invalid move. Player has to overtrump with trump!");
                        StartCoroutine(DisplayWrongMoveText());
                        return false;
                    }
                    else {
                        return true;
                    }
                }
            }
        }
        return true;
    }

    private bool VerifyMarriage(Card clickedCard, List<Card> hand, Player currentPlayer) {
        Card[] trickCards = trick.GetComponentsInChildren<Card>();

        // hand marriage
        if (clickedCard.GetRank() == "Queen" && trickCards.Length == 1){
            foreach (Card card in hand){
                if(card.GetRank()== "King" && card.GetSuitToString() == clickedCard.GetSuitToString()) {
                    Card.Suit suit = clickedCard.GetSuit();
                    GameManager.Instance.AddMarriage(currentPlayer, suit);

                    Debug.Log("Hand marriage: " + clickedCard.GetSuitToString() + " [+" + clickedCard.GetSuit().GetValue() + " points].");
                    GameManager.Instance.runLog.logText("(MARRIAGE) " + clickedCard.GetSuitToString() + 
                    " [+" + clickedCard.GetSuit().GetValue() + " points].", Color.green);
                    return true;
                }
            }
        }
        else if (clickedCard.GetRank() == "King" && trickCards.Length == 1){
            foreach (Card card in hand){
                if(card.GetRank()== "Queen" && card.GetSuitToString() == clickedCard.GetSuitToString()) {
                    Card.Suit suit = clickedCard.GetSuit();
                    GameManager.Instance.AddMarriage(currentPlayer, suit);

                    Debug.Log("Hand marriage: " + clickedCard.GetSuitToString() + " [+" + clickedCard.GetSuit().GetValue() + " points].");
                    GameManager.Instance.runLog.logText("(MARRIAGE) " + clickedCard.GetSuitToString() + 
                    " [+" + clickedCard.GetSuit().GetValue() + " points].", Color.green);
                    return true;
                }
            }
        }

        // king-on-queen marriage
        int trickSize = trickCards.Length;
        if(trickSize > 1) {
            if( trickCards[trickSize-2].GetSuit() == trickCards[trickSize-1].GetSuit() &&
                trickCards[trickSize-2].GetRank() == "Queen" &&
                trickCards[trickSize-1].GetRank() == "King" ) {
                    Card.Suit suit = clickedCard.GetSuit();
                    GameManager.Instance.AddMarriage(currentPlayer, suit);

                    Debug.Log("King-on-queen marriage: " + clickedCard.GetSuitToString() + " [+" + clickedCard.GetSuit().GetValue() + " points].");
                    GameManager.Instance.runLog.logText("(MARRIAGE) " + clickedCard.GetSuitToString() + 
                    " [+" + clickedCard.GetSuit().GetValue() + " points].", Color.green);
                    return true;
                }
        }

        return false;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (hit.collider != null)
        {
            Card clickedCard = hit.collider.gameObject.GetComponent<Card>();
            
            Player p = GameManager.Instance.GameplayCurrentPlayer;
            if(GameManager.Instance.onePlayerMode && p != GameManager.Instance.players[GameManager.humanPlayer]){
                Debug.LogWarning("It's not your move now!");
                return;
            }

            if (GameManager.Instance.isGivingStage && clickedCard.visible)
            {
                Debug.Log(GameManager.Instance.currentPlayer.name);
                GameManager.Instance.currentCardReceiver.AddCardToHand(clickedCard);
                if (GameManager.Instance.currentPlayer.hand.Contains(clickedCard))
                    GameManager.Instance.currentPlayer.hand.Remove(clickedCard);
                GameObject go = GameObject.Find(clickedCard.name);

                go.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);
                go.transform.localRotation = Quaternion.Euler(0, 0, 0);

                clickedCard.SetVisible(GameManager.Instance.currentCardReceiver == GameManager.Instance.currentPlayer);
                GameManager.Instance.otherCards.cards.Remove(clickedCard);
                cardsToDeal--;
                GameManager.Instance.currentCardReceiver = GameManager.Instance.GetNextPlayer(GameManager.Instance.currentCardReceiver);

                TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
                currentCardReceiverText.text = "Choose card for player: " + GameManager.Instance.currentCardReceiver.playerName;

                if (cardsToDeal == 1)
                {
                    clickedCard = GameManager.Instance.otherCards.cards[0];
                    GameManager.Instance.currentCardReceiver.AddCardToHand(clickedCard);
                    GameObject lastCard = GameObject.Find(clickedCard.name);
                    lastCard.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);
                    GameManager.Instance.otherCards.cards.Remove(clickedCard);
                    GameManager.Instance.AddRestToCurrentPlayer();

                    GameManager.Instance.EndDealingStage();
                }

            }
            else if (clickedCard != null && clickedCard.visible && clickedCard.transform.parent != trick.transform && GameManager.Instance.auctionFinished
             && trick.GetComponentsInChildren<Card>().Length <= 3)
            {
                Player current = GameManager.Instance.GetPlayerForCurrentCard(clickedCard.gameObject);
                List<Card> hand = GameManager.Instance.GetPlayerHand(current);
                if(ValidateCardOK(clickedCard, hand)){
                    PlayCard(clickedCard, hand, current);
                    GameManager.Instance.Play(clickedCard);
                    if(!GameManager.Instance.onePlayerMode) GameManager.Instance.MovePlayersToNextPositions();
                    GameManager.Instance.UpdateCardVisibility();
                }
            }
        }
    }

    // handle bot move
    public void OnClickHandle(Card clickedCard)
    {
        if (GameManager.Instance.isGivingStage)
            {
                GameManager.Instance.runLog.logText("<" + GameManager.Instance.currentPlayer.playerName + "> handles card to <" + 
                GameManager.Instance.currentCardReceiver.playerName + ">");
                Debug.Log(GameManager.Instance.currentPlayer.name);
                GameManager.Instance.currentCardReceiver.AddCardToHand(clickedCard);
                if (GameManager.Instance.currentPlayer.hand.Contains(clickedCard))
                    GameManager.Instance.currentPlayer.hand.Remove(clickedCard);
                GameObject go = GameObject.Find(clickedCard.name);

                go.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);
                go.transform.localRotation = Quaternion.Euler(0, 0, 0);

                clickedCard.SetVisible(GameManager.Instance.currentCardReceiver == GameManager.Instance.currentPlayer);
                GameManager.Instance.otherCards.cards.Remove(clickedCard);
                cardsToDeal--;
                GameManager.Instance.currentCardReceiver = GameManager.Instance.GetNextPlayer(GameManager.Instance.currentCardReceiver);

                TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
                currentCardReceiverText.text = "Choose card for player: " + GameManager.Instance.currentCardReceiver.playerName;
                
                
                if (cardsToDeal == 1)
                {
                    clickedCard = GameManager.Instance.otherCards.cards[0];
                    GameManager.Instance.currentCardReceiver.AddCardToHand(clickedCard);
                    GameObject lastCard = GameObject.Find(clickedCard.name);
                    lastCard.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);
                    GameManager.Instance.otherCards.cards.Remove(clickedCard);
                    GameManager.Instance.AddRestToCurrentPlayer();
                    GameManager.Instance.EndDealingStage();
                }

            }
            else{
                Player current = GameManager.Instance.GetPlayerForCurrentCard(clickedCard.gameObject);
                List<Card> hand = GameManager.Instance.GetPlayerHand(current);
                if(ValidateCardOK(clickedCard, hand)){
                    PlayCard(clickedCard, hand, current);
                    GameManager.Instance.Play(clickedCard);
                    //GameManager.Instance.MovePlayersToNextPositions();
                    GameManager.Instance.UpdateCardVisibility();
                }
            }
    }


    private void PlayCard(Card card, List<Card> hand, Player current)
    {
        if (card.transform.parent != trick.transform)
        {
            card.transform.SetParent(trick.transform);
            card.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            sortingOrder++;
            card.SetVisible(true);
            Debug.Log("Played card: " + card.gameObject.name);
            VerifyMarriage(card, hand, current);
            current.RemoveCardFromHand(card);

            GameManager.Instance.runLog.logText("<" + current.playerName + "> plays " + card.GetCardFullName() + ".");
        
            // end of turn (4 cards on table)
            /*
            if (trick.transform.childCount == 4) {
                Debug.Log("End of turn.");
                for(int i=0; i < 4; i++) {
                    GameObject trickCard = trick.transform.GetChild(0).gameObject;
                    Vector3 p = new Vector3(500, 300, 69);
                    trickCard.transform.position = p;
                    trickCard.transform.SetParent(null);
                }
            }
            */

        }
        else
        {
            Debug.Log("Cannot add card already in the trick area.");
        }
    }

    public void ResetCardsToDeal()
    {
        cardsToDeal = 4;
    }
}
