using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace _1K_ComputerPlayer
{
	internal static class TurnOptimizingPlayer
	{
		private static List<Card> CreateWholeDeck()
		{
			var deck = new List<Card>();
			foreach (Card.Suit suit in Enum.GetValues(typeof(Card.Suit)))
			{
				foreach (Card.Rank rank in Enum.GetValues(typeof(Card.Rank)))
				{
					Card card = new Card(suit, rank);
					deck.Add(card);
				}
			}

			return deck;
		}
		
		public static Card GetBestCardToPlay(List<Card> hand, List<Card> cardsAlreadyPlayed, List<Card> outerTrick, Card.Suit? atu)
		{
			bool DEBUG = false;
			const int NumberOfSimulations = 1000;
			List<Card> _deck = new();
			Random _rand = new();

			_deck = CreateWholeDeck(); // init whole 24-card deck
			_deck.RemoveAll(cardsAlreadyPlayed.Contains);
			_deck.RemoveAll(hand.Contains);
			_deck.RemoveAll(outerTrick.Contains);
			if(DEBUG) Console.WriteLine("\nDECK REVISED: [" + _deck.Count + "]\n" + string.Join("\n", _deck));
			if (DEBUG) Console.WriteLine("\nHAND: [" + hand.Count + "]\n" + string.Join("\n", hand));
			if (DEBUG) Console.WriteLine("\nTRICK: [" + outerTrick.Count + "]\n" + string.Join("\n", outerTrick));

			var cardScore = new int[hand.Count];
			var trick = new List<Card>();

			for (var i = 0; i < NumberOfSimulations; i++)
			{
				for (var j = 0; j < hand.Count; j++)
				{
					trick.Clear();
					trick = new List<Card>(outerTrick);

					//Console.WriteLine("\nTrick:\n" + string.Join("\n", trick));
					var card = hand[j];
					var isWinning = false;

					if (Validate.IsLegalMove(card, hand, trick, atu))
					{
						trick.Add(card);
						if (Validate.IsNewTrickWinner(trick, atu)) isWinning = true;
						cardScore[j] += Validate.GetMarriageScore(card, hand, trick);

						while (trick.Count < 4)
						{
							//Console.WriteLine("in while" + card);
							var randCard = _deck[_rand.Next(0, _deck.Count)];
							trick.Add(randCard);
							if (Validate.IsNewTrickWinner(trick, atu)) isWinning = false;
						}

						var turnScore = Validate.GetTrickScore(trick);
						if (isWinning) cardScore[j] += turnScore;
						else cardScore[j] -= turnScore;
					}
					else
					{
						cardScore[j] = 0;
					}
				}
			}

			// evaluation
			if (DEBUG) Console.WriteLine("\nFINAL SCORE:\n" + string.Join("\n", cardScore));

			var maxValue = cardScore.Where(score => score != 0).DefaultIfEmpty(int.MinValue).Max();
			var maxIndex = Array.IndexOf(cardScore, maxValue);
			if (DEBUG) Console.WriteLine("\nbest to play: " + hand[maxIndex]);
			return hand[maxIndex];
		}
	}
}
