using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameObject trick;
    private int sortingOrder = 1;
    private int cardsToDeal = 4;

    private bool ValidateCardOK(Card clickedCard, List<Card> hand) {
        Card[] trickCards = trick.GetComponentsInChildren<Card>();
        if (trickCards.Length > 0) {
            Card baseCard = trickCards[0];

            // Rule 1. Same suit
            if (clickedCard.GetSuit() == baseCard.GetSuit()) {
                // Rule 2. Overtrump (higher value of same suit)
                if (clickedCard.GetValue() > baseCard.GetValue()){
                    return true;
                }
                else {
                    // verify player can overtrump current trick winner
                    Card trickWinner = trickCards[0];
                    foreach (Card card in trickCards){
                        if(card.GetSuit() == trickWinner.GetSuit() && card.GetValue() > trickWinner.GetValue()){
                            trickWinner = card;
                        }
                    }
                    foreach (Card card in hand){
                        if(card.GetSuit() == trickWinner.GetSuit() && card.GetValue() > trickWinner.GetValue()){
                            Debug.LogWarning("Invalid move. Need to overtrump with higher value of " + baseCard.GetSuit() + "!");
                            return false;
                        }
                    }
                }
            }
            else {
                foreach (Card card in hand){
                    if(card.GetSuit() == baseCard.GetSuit()){
                            Debug.LogWarning("Invalid move. Need to lay card of " + baseCard.GetSuit() + "!");
                            return false;
                        }
                }
            }
            // Rule 3. Overtrump (atu)
            // TBD (after marriage implementation)

        }
        return true;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (hit.collider != null)
        {
            Card clickedCard = hit.collider.gameObject.GetComponent<Card>();
            if (GameManager.Instance.isGivingStage)
            {
                GameManager.Instance.currentCardReceiver.AddCardToHand(clickedCard);
                GameObject go = GameObject.Find(clickedCard.name);

                go.transform.SetParent(GameManager.Instance.currentCardReceiver.transform);

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

                    GameManager.Instance.EndDealingStage();
                }

            }
            else if (clickedCard != null && clickedCard.visible && clickedCard.transform.parent != trick.transform)
            {
                Player current = GameManager.Instance.GetPlayerForCurrentCard(clickedCard.gameObject);
                List<Card> hand = GameManager.Instance.GetPlayerHand(current);
                if(ValidateCardOK(clickedCard, hand)){
                    PlayCard(clickedCard, current);
                    GameManager.Instance.Play(clickedCard);
                    GameManager.Instance.MovePlayersToNextPositions();
                    GameManager.Instance.UpdateCardVisibility();
                }
            }
        }
    }

    private void PlayCard(Card card, Player current)
    {
        if (card.transform.parent != trick.transform)
        {
            card.transform.SetParent(trick.transform);
            card.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;
            sortingOrder++;
            Debug.Log("Played card: " + card.gameObject.name);
            current.RemoveCardFromHand(card);

            // end of turn (4 cards on table)
            if (trick.transform.childCount == 4) {
                Debug.Log("End of turn.");
                for(int i=0; i < 4; i++) {
                    GameObject trickCard = trick.transform.GetChild(0).gameObject;
                    Vector3 p = new Vector3(500, 300, 69);
                    trickCard.transform.position = p;
                    trickCard.transform.SetParent(null);
                }
            }
        }
        else
        {
            Debug.Log("Cannot add card already in the trick area.");
        }
    }
}
