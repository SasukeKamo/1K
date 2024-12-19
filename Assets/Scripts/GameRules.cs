using System;
using System.Collections.Generic;
using System.Linq;

public class GameRules
{
    public bool BiddingLimitation { get; set; }
    public bool Bomb { get; set; }
    public bool KingOnQueenMarriage { get; set; }
    public int[] BombCount;

    public GameRules(bool biddingLimitation = true, bool bomb = true, bool kingOnQueenMarriage = false, int playerN = 4)
    {
        BiddingLimitation = biddingLimitation;
        Bomb = bomb;
        KingOnQueenMarriage = kingOnQueenMarriage;
        BombCount = new int[playerN];
        if (bomb)
        {
            Array.Fill(BombCount, 1);
        }
    }

    public bool CanPlayerUseBomb(int playerIndex)
    {
        if (BombCount[playerIndex] > 0)
        {
            BombCount[playerIndex]--;
            return true;
        }
        return false;
    }

    public bool CanPlayerBid(int currentBid, List<Card> cards)
    {
        if (!BiddingLimitation) return true;

        int allowedBid = 120;
        var groupedBySuit = cards.GroupBy(card => card.GetSuit());

        foreach (var group in groupedBySuit)
        {
            bool hasQueen = group.Any(card => card.GetRankAsRank() == Card.Rank.Queen);
            bool hasKing = group.Any(card => card.GetRankAsRank() == Card.Rank.King);

            if (hasQueen && hasKing)
            {
                Card.Suit suit = group.FirstOrDefault().GetSuit();
                allowedBid += suit.GetMarriageValue();
            }
        }
        UnityEngine.Debug.Log("allowedBid=" + allowedBid);
        return currentBid < allowedBid;
    }
}
