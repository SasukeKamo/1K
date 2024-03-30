using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameObject trick;
    private int sortingOrder = 1;

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (hit.collider != null)
        {
            Card clickedCard = hit.collider.gameObject.GetComponent<Card>();
            if (clickedCard != null && clickedCard.visible)
            {
                Player current = GameManager.Instance.GetPlayerForCurrentCard(clickedCard.gameObject);
                PlayCard(clickedCard, current);
                GameManager.Instance.Play(clickedCard);
                GameManager.Instance.MovePlayersToNextPositions();
                Debug.LogError(current.playerNumber);
                GameManager.Instance.UpdateCardVisibility();
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
        }
        else
        {
            Debug.Log("Cannot add card already in the trick area.");
        }
    }
}
