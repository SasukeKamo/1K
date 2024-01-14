using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades };
    public enum Rank { Nine, Ten, Jack, Queen, King, Ace };

    //mozecie tez dodac tutaj np image albo sprite i jakas wartosc np czyOdwrocona i zmieniac zdjecie jesli jest i jesli nie jest
    [SerializeField] private Sprite front;
    [SerializeField] private Sprite back;

    private bool visible;
    private SpriteRenderer spriteRenderer;


    [SerializeField] private Suit suit;
    [SerializeField] private Rank rank;
    private int value;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = front;
        visible = true;
    }

    public Card(Suit cardSuit, Rank cardRank)
    {
        suit = cardSuit;
        rank = cardRank;

        switch (rank)
        {
            case Rank.Ace:
                value = 11;
                break;
            case Rank.Ten:
                value = 10;
                break;
            case Rank.King:
                value = 4;
                break;
            case Rank.Queen:
                value = 3;
                break;
            case Rank.Jack:
                value = 2;
                break;
            case Rank.Nine:
                value = 0;
                break;
            default:
                value = 0;
                break;
        }

        //przypisac sprite i ustawic na visible
        AssignSprites("Assets/Cards2D/Sprites");

        spriteRenderer = GetComponent<SpriteRenderer>();
        SetVisible(true);

    }

    private string GenerateSpriteName()
    {
        //string spriteName = Enum.GetName(typeof(Suit), suit);     //do ewentualnej optymalizacji - branie nazwy bez switch case
        string spriteName = "";

        switch (suit)
        {
            case Suit.Hearts:
                spriteName += "Heart";
                break;
            case Suit.Diamonds:
                spriteName += "Diamond";
                break;
            case Suit.Clubs:
                spriteName += "Club";
                break;
            case Suit.Spades:
                spriteName += "Spade";
                break;
        }

        switch (rank)
        {
            case Rank.Ace:
                spriteName += 01;
                break;
            case Rank.Ten:
                spriteName += 10;
                break;
            case Rank.King:
                spriteName += 13;
                break;
            case Rank.Queen:
                spriteName += 12;
                break;
            case Rank.Jack:
                spriteName += 11;
                break;
            case Rank.Nine:
                spriteName += 09;
                break;
            default:
                spriteName += 01;
                break;
        }

        return spriteName;
    }

    private bool AssignSprites(string folderPath)    //path: Assets/Cards2D/Sprites
    {
        string spriteName = GenerateSpriteName();

        // Pobranie wszystkich obrazow
        Sprite[] allSprites = Resources.LoadAll<Sprite>(folderPath);

        // Filtr obrazow
        front = System.Array.Find(allSprites, s => s.name.Contains(spriteName));
        back = System.Array.Find(allSprites, s => s.name.Contains("BackColor_Red"));

        if (front == null || back == null)
        {
            Debug.LogError($"No sprites with the name '{spriteName}' found in the specified folder: {folderPath}");
            return false;
        }

        return true;
    }

    public void SetVisible(bool visible)
    {
        if(visible)
        {
            this.visible = true;
            spriteRenderer.sprite = front;
        }
        else
        {
            this.visible = false;
            spriteRenderer.sprite = back;
        }
    }

    public string GetCardFullName() //mozna uzyc do debugu
    {
        string rankString = rank.ToString();
        string suitString = suit.ToString();
        return rankString + " of " + suitString;
    }

    public string GetSuit()
    {
        return suit.ToString();
    }

    public int GetValue()
    {
        return value;
    }
}
