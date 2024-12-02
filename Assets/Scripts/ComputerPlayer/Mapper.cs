using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using PlayerCard = _1K_ComputerPlayer.Card;
using UnityCard = Card;
using UnityEngine;
using Debug = UnityEngine.Debug;

static class ComputerPlayer{

    private static List<PlayerCard> Map(List<UnityCard> unityCards){
        var playerCards = new List<PlayerCard>();
        foreach(UnityCard unityCard in unityCards){
            var suit = (PlayerCard.Suit)unityCard.GetSuit();
            var rank = (PlayerCard.Rank)unityCard.GetRankAsRank();
            var playerCard = new PlayerCard(suit, rank);
            playerCards.Add(playerCard);
        }
        return playerCards;
    }
    public static UnityCard GetBestCardToPlay(List<UnityCard> hand, List<UnityCard> cardsAlreadyPlayed, List<UnityCard> outerTrick, UnityCard.Suit? atu){

        List<PlayerCard> _hand = Map(hand);
        List<PlayerCard> _played = Map(cardsAlreadyPlayed);
        List<PlayerCard> _trick = Map(outerTrick);
        PlayerCard.Suit? _atu = (atu == UnityCard.Suit.None) ? null : (PlayerCard.Suit)atu;

        Debug.Log("Already played cards: " + _played.Count);
        PlayerCard _card = _1K_ComputerPlayer.TurnOptimizingPlayer.GetBestCardToPlay(_hand, _played, _trick, _atu);

        return hand.Find(c => (c.GetRankAsRank() == (UnityCard.Rank)_card.rank && c.GetSuit() == (UnityCard.Suit)_card.suit));
    }
}