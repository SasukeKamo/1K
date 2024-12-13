using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Photon.Pun;

public class InputHandler : MonoBehaviour
{
    const int trickOffset_Y = 20;
    [SerializeField] private GameObject trick;
    [SerializeField] private TrickManager trickManager;
    public int sortingOrder = 1;
    public int cardsToDeal = 4;
    private bool isAnyCardInAnim = false;

    private int leftPlayer = 1;
    private int rightPlayer = 3;

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


    private IEnumerator DisplayWrongMoveText()
    {
        if (GameManager.Instance.onePlayerMode && GameManager.Instance.GameplayCurrentPlayer == GameManager.Instance.players[GameManager.humanPlayer])
        {
            TextMeshProUGUI text = GameObject.Find("WrongMoveText").GetComponent<TextMeshProUGUI>();
            text.text = "illegal move";
            yield return new WaitForSeconds(0.5f);
            text.text = "";
        }
        else if (!GameManager.Instance.onePlayerMode)
        {
            TextMeshProUGUI text = GameObject.Find("WrongMoveText").GetComponent<TextMeshProUGUI>();
            text.text = "illegal move";
            yield return new WaitForSeconds(0.5f);
            text.text = "";
        }
    }

    public bool ValidateCardOK(Card clickedCard, List<Card> hand)
    {
        Card[] trickCards = trickManager.GetTrickCards();
        if (trickCards.Length > 0)
        {
            Card baseCard = trickCards[0];

            // Rule 1. Same suit
            if (clickedCard.GetSuitToString() == baseCard.GetSuitToString())
            {
                // Rule 2. Overtrump
                if (clickedCard.GetValue() > baseCard.GetValue())
                {
                    return true;
                }
                else
                {
                    // verify player can overtrump

                    // Case 1: Player has card of current suit
                    Card trickWinner = trickCards[0];
                    foreach (Card card in trickCards)
                    {
                        if (card.GetSuitToString() == trickWinner.GetSuitToString() && card.GetValue() > trickWinner.GetValue())
                        {
                            trickWinner = card;
                        }
                    }

                    foreach (Card card in hand)
                    {
                        if (card.GetSuitToString() == trickWinner.GetSuitToString() && card.GetValue() > trickWinner.GetValue())
                        {
                            Debug.LogWarning("Invalid move. Need to overtrump with higher value of " + baseCard.GetSuitToString() + "!");
                            StartCoroutine(DisplayWrongMoveText());
                            return false;
                        }
                    }
                }
            }
            else
            {
                // Player has cards of the current suit
                foreach (Card card in hand)
                {
                    if (card.GetSuitToString() == baseCard.GetSuitToString())
                    {
                        Debug.LogWarning("Invalid move. Need to lay card of " + baseCard.GetSuitToString() + "!");
                        StartCoroutine(DisplayWrongMoveText());
                        return false;
                    }
                }

                // Player can overtrump with atu
                Card.Suit currentAtuSuit = GameManager.Instance.GetAtuSuit();
                if (currentAtuSuit != Card.Suit.None)
                {
                    List<Card> handTrumps = new List<Card>();
                    foreach (Card card in hand)
                    {
                        if (card.GetSuit() == currentAtuSuit)
                        {
                            handTrumps.Add(card);
                        }
                    }
                    Card highestPlayerAtu;
                    if (handTrumps.Count > 0)
                    {
                        highestPlayerAtu = handTrumps[0];
                        foreach (Card card in handTrumps)
                        {
                            if (card.GetValue() > highestPlayerAtu.GetValue())
                            {
                                highestPlayerAtu = card;
                            }
                        }
                    }
                    else return true;

                    List<Card> trickTrumps = new List<Card>();
                    foreach (Card card in trickCards)
                    {
                        if (card.GetSuit() == currentAtuSuit)
                        {
                            trickTrumps.Add(card);
                        }
                    }
                    Card highestTrickAtu;
                    if (trickTrumps.Count > 0)
                    {
                        highestTrickAtu = trickTrumps[0];
                        foreach (Card card in trickTrumps)
                        {
                            if (card.GetValue() > highestTrickAtu.GetValue())
                            {
                                highestTrickAtu = card;
                            }
                        }
                    }
                    else
                    {
                        if (clickedCard.GetSuit() == currentAtuSuit)
                        {
                            return true;
                        }
                        else
                        {
                            Debug.Log("Invalid move. Player has to play with trump!");
                            StartCoroutine(DisplayWrongMoveText());
                            return false;
                        }
                    }
                    if (highestPlayerAtu.GetValue() > clickedCard.GetValue())
                    {
                        Debug.Log("Invalid move. Player has to overtrump with trump!");
                        StartCoroutine(DisplayWrongMoveText());
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        return true;
    }

    private bool VerifyMarriage(Card clickedCard, List<Card> hand, Player currentPlayer)
    {
        Card[] trickCards = trickManager.GetTrickCards();

        // hand marriage
        if (clickedCard.GetRank() == "Queen" && trickCards.Length == 1)
        {
            foreach (Card card in hand)
            {
                if (card.GetRank() == "King" && card.GetSuitToString() == clickedCard.GetSuitToString())
                {
                    Card.Suit suit = clickedCard.GetSuit();
                    GameManager.Instance.AddMarriage(currentPlayer, suit);

                    Debug.Log("Hand marriage: " + clickedCard.GetSuitToString() + " [+" + clickedCard.GetSuit().GetValue() + " points].");
                    GameManager.Instance.runLog.logText("(MARRIAGE) " + clickedCard.GetSuitToString() +
                    " [+" + clickedCard.GetSuit().GetValue() + " points].", Color.green);
                    return true;
                }
            }
        }
        else if (clickedCard.GetRank() == "King" && trickCards.Length == 1)
        {
            foreach (Card card in hand)
            {
                if (card.GetRank() == "Queen" && card.GetSuitToString() == clickedCard.GetSuitToString())
                {
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
        if (trickSize > 1)
        {
            if (trickCards[trickSize - 2].GetSuit() == trickCards[trickSize - 1].GetSuit() &&
                trickCards[trickSize - 2].GetRank() == "Queen" &&
                trickCards[trickSize - 1].GetRank() == "King")
            {
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

    /*public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (isAnyCardInAnim) return;

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (hit.collider != null)
        {
            Card clickedCard = hit.collider.gameObject.GetComponent<Card>();
            if (!clickedCard.selected) return;

            Player p = GameManager.Instance.GameplayCurrentPlayer;
            if (GameManager.Instance.onePlayerMode && p != GameManager.Instance.players[GameManager.humanPlayer])
            {
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
                if (ValidateCardOK(clickedCard, hand))
                {
                    PlayCard(clickedCard, hand, current);
                    GameManager.Instance.Play(clickedCard);
                    StartCoroutine(WaitForAnimEnd(clickedCard, true));

                }
            }
        }
    }*/

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (isAnyCardInAnim) return;

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (hit.collider != null)
        {
            Card clickedCard = hit.collider.gameObject.GetComponent<Card>();
            if (!clickedCard.selected) return;

            Player currentPlayer = GameManager.Instance.GameplayCurrentPlayer;

            if (GameManager.Instance.onePlayerMode && currentPlayer != GameManager.Instance.players[GameManager.humanPlayer])
            {
                Debug.LogWarning("It's not your move now!");
                return;
            }

            // rozdawanie kart
            if (GameManager.Instance.isGivingStage && clickedCard.visible)
            {
                if (GameManager.IsMultiplayerMode)
                {
                    GameManager.Instance.photonView.RPC("SyncDistributeCardToPlayer", RpcTarget.All,
                        clickedCard.name);
                }
                else
                {
                    HandleCardDistribution(clickedCard);
                }
            }
            // zagrywanie kart
            else if (clickedCard != null && clickedCard.visible && clickedCard.transform.parent != trick.transform
                     && GameManager.Instance.auctionFinished && trick.GetComponentsInChildren<Card>().Length <= 3)
            {
                if (GameManager.IsMultiplayerMode && PhotonNetwork.LocalPlayer.ActorNumber !=
                    GameManager.Instance.GameplayCurrentPlayer.playerNumber)
                {
                    Debug.LogError($"Cannot click card IsMultiplayerMode={GameManager.IsMultiplayerMode}, P{PhotonNetwork.LocalPlayer.ActorNumber} != {GameManager.Instance.GameplayCurrentPlayer.playerNumber}");
                    return;
                }

                Player cardOwner = GameManager.Instance.GetPlayerForCurrentCard(clickedCard.gameObject);
                List<Card> hand = GameManager.Instance.GetPlayerHand(cardOwner);

                if (ValidateCardOK(clickedCard, hand))
                {
                    if (GameManager.IsMultiplayerMode)
                    {
                        // GameManager.Instance.photonView.RPC("PlayCardOnTable", RpcTarget.All,
                        //     clickedCard.name,
                        //     cardOwner.playerName);
                        GameManager.Instance.photonView.RPC("SyncGameplayTurn", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, clickedCard.name);
                    }
                    else
                    {
                        HandleCardPlay(clickedCard, hand, cardOwner);
                    }
                }
            }
        }
    }

    void HandleCardDistribution(Card clickedCard)
    {
        GameManager.Instance.currentCardReceiver.AddCardToHand(clickedCard);

        if (GameManager.Instance.currentPlayer.hand.Contains(clickedCard))
            GameManager.Instance.currentPlayer.hand.Remove(clickedCard);

        GameObject cardObject = GameObject.Find(clickedCard.name);
        cardObject.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);
        cardObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        clickedCard.SetVisible(GameManager.Instance.currentCardReceiver == GameManager.Instance.currentPlayer);
        GameManager.Instance.otherCards.cards.Remove(clickedCard);

        cardsToDeal--;
        GameManager.Instance.currentCardReceiver = GameManager.Instance.GetNextPlayer(GameManager.Instance.currentCardReceiver);

        TextMeshProUGUI currentCardReceiverText = GameObject.Find("CurrentCardReceiverText").GetComponent<TextMeshProUGUI>();
        currentCardReceiverText.text = "Choose card for player: " + GameManager.Instance.currentCardReceiver.playerName;

        if (cardsToDeal == 1)
        {
            Card lastCard = GameManager.Instance.otherCards.cards[0];
            GameManager.Instance.currentCardReceiver.AddCardToHand(lastCard);

            GameObject lastCardObject = GameObject.Find(lastCard.name);
            lastCardObject.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);

            GameManager.Instance.otherCards.cards.Remove(lastCard);
            GameManager.Instance.AddRestToCurrentPlayer();

            GameManager.Instance.EndDealingStage();
        }
    }

    void HandleCardPlay(Card clickedCard, List<Card> hand, Player cardOwner)
    {
        PlayCard(clickedCard, hand, cardOwner);
        GameManager.Instance.Play(clickedCard);
        StartCoroutine(WaitForAnimEnd(clickedCard, true));
    }


    private void AfterClickUpdate(bool moveToNextPos)
    {
        if (!GameManager.Instance.onePlayerMode && moveToNextPos) GameManager.Instance.MovePlayersToNextPositions();
        GameManager.Instance.UpdateCardVisibility();
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

            GameManager.Instance.UpdateCardVisibility();
            if (cardsToDeal == 1)
            {
                GameManager.Instance.AddRestToCurrentPlayer();
                GameManager.Instance.EndDealingStage();
                GameManager.Instance.UpdateCardVisibility();
            }

        }
        else
        {
            clickedCard.ForceScale();
            Player current = GameManager.Instance.GetPlayerForCurrentCard(clickedCard.gameObject);
            List<Card> hand = GameManager.Instance.GetPlayerHand(current);
            if (ValidateCardOK(clickedCard, hand))
            {
                PlayCard(clickedCard, hand, current);
                GameManager.Instance.Play(clickedCard);
                //GameManager.Instance.MovePlayersToNextPositions();
                StartCoroutine(WaitForAnimEnd(clickedCard, false));
            }
        }
    }


    public void PlayCard(Card card, List<Card> hand, Player current)
    {
        Debug.LogError("<P" + PhotonNetwork.LocalPlayer.ActorNumber + "> entered PlayCard()");

        if (card.transform.parent != trick.transform)
        {
            trickManager.AddCard(card);
            trickManager.AddCardToPlayed(card);
            card.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            sortingOrder++;

            card.SetVisible(true);
            Debug.Log("Played card: " + card.gameObject.name);
            VerifyMarriage(card, hand, current);
            current.RemoveCardFromHand(card);

            GameManager.Instance.runLog.logText("<" + current.playerName + "> plays " + card.GetCardName() + ".");

            AnimateCardToCenter(card, current);
        }
        else
        {
            Debug.Log("Cannot add card already in the trick area.");
        }

        Debug.LogError("<P" + PhotonNetwork.LocalPlayer.ActorNumber + "> exit PlayCard()");
    }

    private void AnimateCardToCenter(Card card, Player currentPlayer)
    {
        Vector3 originalScale = card.transform.localScale;
        Vector3 targetPosition = new Vector3(trick.transform.position.x, trick.transform.position.y + trickOffset_Y, trick.transform.position.z);
        int originalSO = card.spriteRenderer.sortingOrder;
        card.spriteRenderer.sortingOrder = 100; //always on the first place;
        card.isDotweenAnimStarted = true;
        isAnyCardInAnim = true;

        DG.Tweening.Sequence mySequence = DOTween.Sequence();
        mySequence.Append(card.transform.DOMove(targetPosition, 0.5f))
                  .Join(card.transform.DOScale(originalScale * 1.2f, 0.25f))
                  .Append(card.transform.DOScale(originalScale, 0.25f))
                  .OnComplete(() =>
                  {
                      card.spriteRenderer.sortingOrder = originalSO;
                      card.readyForDissolve = true;
                      card.transform.SetParent(trick.transform);

                      //HERE
                      //if (GameManager.Instance.onePlayerMode && (currentPlayer == GameManager.Instance.players[leftPlayer] || currentPlayer == GameManager.Instance.players[rightPlayer]))
                      //    card.gameObject.transform.Rotate(0, 0, 90);
                      card.gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

                      card.isDotweenAnimEnded = true;
                      isAnyCardInAnim = false;
                  });
    }

    public IEnumerator WaitForAnimEnd(Card card, bool move)
    {
        yield return new WaitUntil(() => card.isDotweenAnimEnded);

        if(!GameManager.IsMultiplayerMode)
            AfterClickUpdate(move);

        Debug.LogError("<P" + PhotonNetwork.LocalPlayer.ActorNumber + "> animation ended");
    }


    public void ResetCardsToDeal()
    {
        cardsToDeal = 4;
    }
}
