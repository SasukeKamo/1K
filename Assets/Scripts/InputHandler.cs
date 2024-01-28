using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameObject trick;
    private int sOrder = 1;
    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        var rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) return;

        Card clickedCard = rayHit.collider.gameObject.GetComponent<Card>();
        if (clickedCard != null && clickedCard.visible)
        {
            Debug.Log(clickedCard.gameObject.name);
            rayHit.collider.gameObject.transform.SetParent(trick.transform);
            rayHit.collider.gameObject.GetComponent<SpriteRenderer>().sortingOrder = sOrder;
            sOrder++;
        }
    }
}
