using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades, None };
    public enum Rank { Nine, Ten, Jack, Queen, King, Ace };

    [SerializeField] private Sprite front;
    [SerializeField] private Sprite back;

    public bool visible;
    public SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Vector3 startingScale;
    private Vector3 originalPosition;
    [SerializeField] private GameObject currentPlayerHand;
    [SerializeField] private GameObject auctionLeftOvers;
    private float dissolveRate = 0.01f;
    private float refreshRate = 0.01f;
    public bool readyForDissolve = false;
    public bool isDotweenAnimStarted = false;
    public bool isDotweenAnimEnded = false;
    public bool selected = false;

    [SerializeField] private Suit suit;
    [SerializeField] private Rank rank;
    private int value;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = front;
        visible = false;

        switch (rank)
        {
            case Rank.Nine:
                value = 0;
                break;
            case Rank.Ten:
                value = 10;
                break;
            case Rank.Jack:
                value = 2;
                break;
            case Rank.Queen:
                value = 3;
                break;
            case Rank.King:
                value = 4;
                break;
            case Rank.Ace:
                value = 11;
                break;
        }
    }

    private void Start()
    {

    }

    public void SetVisible(bool visible)
    {
        this.visible = visible;
        spriteRenderer.sprite = visible ? front : back;
    }

    public string GetCardFullName() //mozna uzyc do debugu
    {
        string rankString = rank.ToString();
        string suitString = suit.ToString();
        return rankString + " of " + suitString;
    }

    public string GetCardName()
    {
        return rank.AsSymbol() + suit.AsSymbol();
    }

    public string GetSuitToString()
    {
        return suit.ToString();
    }

    public Suit GetSuit()
    {
        return suit;
    }

    public string GetRank()
    {
        return rank.ToString();
    }
    public Rank GetRankAsRank()
    {
        return rank;
    }

    public int GetValue()
    {
        return value;
    }

    private void OnMouseEnter()
    {
        if (!isDotweenAnimStarted)
        {
            originalScale = transform.localScale;
            if (transform.parent.parent == currentPlayerHand.transform || transform.parent == auctionLeftOvers.transform)
            {
                AudioManager.Instance.PlaySelectCardSound();
                transform.localScale = originalScale * 1.1f;
                this.spriteRenderer.sortingOrder++;
                selected = true;
            }
        }
    }

    public void SetSortingOrder(int i)
    {
        spriteRenderer.sortingOrder = i;
    }

    public void ForceScale()
    {
        originalScale = transform.localScale;
        transform.localScale = originalScale * 1.1f;
        this.spriteRenderer.sortingOrder++;
    }


    public void ResetCard()
    {
        spriteRenderer.sortingOrder = 0;
        startingScale = new Vector3(6, 6, 6);
        transform.localScale = startingScale;
        isDotweenAnimStarted = false;
        isDotweenAnimEnded = false;
    }

    public void Dissolve()
    {
        StartCoroutine(DissolveIEnumerator());
    }

    public IEnumerator DissolveIEnumerator()
    {

        float counter = 0.8f;
        while (spriteRenderer.material.GetFloat("_Progress") > 0)
        {
            counter -= dissolveRate;
            spriteRenderer.material.SetFloat("_Progress", counter);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    public void ResetDissolve()
    {
        spriteRenderer.material.SetFloat("_Progress", 0.8f);
        readyForDissolve = false;
    }

    private void OnMouseExit()
    {
        if (!isDotweenAnimStarted)
        {
            if (transform.localScale != originalScale)
            {
                transform.localScale = originalScale;
                if (spriteRenderer.sortingOrder > 0)
                    this.spriteRenderer.sortingOrder--;
                selected = false;
            }
        }
    }

}

public static class SuitValue
{
    public static int GetValue(this Card.Suit suit)
    {
        switch (suit)
        {
            case Card.Suit.Hearts:
                return 100;
            case Card.Suit.Diamonds:
                return 80;
            case Card.Suit.Clubs:
                return 60;
            case Card.Suit.Spades:
                return 40;
            default:
                Debug.LogError("Cannot obtain suit value.");
                return 0;
        }
    }
}