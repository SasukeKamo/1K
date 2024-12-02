using System;
using System.Collections.Generic;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace _1K_ComputerPlayer
{
	static class TurnOptimizingPlayer
	{
		const int NumberOfSimulations = 1000;
		const bool IsDebug = false;

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

		private static int CalculateExpectedScore(List<Card> hand)
		{
			List<Card> _hand = new List<Card>(hand);
			int cardScore = 0;
			for (int i = 0; i < hand.Count; i++)
			{
				int score;
				Card card;
				(score, card) = GetBestCardToPlay(_hand, new List<Card>(), new List<Card>(), null);
				//if (IsDebug) Console.WriteLine(card + " -> score: " + score / NumberOfSimulations);
				if (score < 0) break;
				cardScore += score / NumberOfSimulations;
				_hand.Remove(card);
			}
			//if(IsDebug)Console.WriteLine("Expected total score: " + cardScore);
			return cardScore;
		}

		public static Card GetCardToDeal(List<Card> hand, bool isMaximizingPlayer)
		{
			var _hand = new List<Card>(hand);
			var _sortedHand = new List<Card>();
			var cardScore = new int[_hand.Count];

			for (int i = 0; i < hand.Count; i++)
			{
				int score;
				Card card;
				(score, card) = GetBestCardToPlay(_hand, new List<Card>(), new List<Card>(), null);
				//if (IsDebug) Console.WriteLine(card + " -> score: " + score / NumberOfSimulations);
				cardScore[i] = score;
				_hand.Remove(card);
				_sortedHand.Add(card);
			}

			if (!isMaximizingPlayer) return _sortedHand.Last();
			var averageIndex = (int)Math.Ceiling((decimal)(hand.Count / 2));
			return _sortedHand[averageIndex];
		}

		public static bool ShouldBid(List<Card> hand, int expectedBid)
		{
			const double RiskFactor = 2.0;

			var expectedScore = CalculateExpectedScore(hand);
			Debug.Log(expectedScore * RiskFactor);
			return (expectedScore * RiskFactor >= expectedBid);
		}

		public static (int, Card) GetBestCardToPlay(List<Card> hand, List<Card> cardsAlreadyPlayed, List<Card> outerTrick, Card.Suit? atu)
		{
			List<Card> _deck = new();
			Random _rand = new();

			_deck = CreateWholeDeck(); // init whole 24-card deck
			_deck.RemoveAll(cardsAlreadyPlayed.Contains);
			_deck.RemoveAll(hand.Contains);
			_deck.RemoveAll(outerTrick.Contains);
			//if (IsDebug) Console.WriteLine("\nDECK REVISED: [" + _deck.Count + "]\n" + string.Join("\n", _deck));
			//if (IsDebug) Console.WriteLine("\nHAND: [" + hand.Count + "]\n" + string.Join("\n", hand));
			//if (IsDebug) Console.WriteLine("\nTRICK: [" + outerTrick.Count + "]\n" + string.Join("\n", outerTrick));

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
			//if (IsDebug) Console.WriteLine("\nFINAL SCORE:\n" + string.Join("\n", cardScore));

			var maxValue = cardScore.Where(score => score != 0).DefaultIfEmpty(int.MinValue).Max();
			var maxIndex = Array.IndexOf(cardScore, maxValue);
			//if (IsDebug) Console.WriteLine("\nbest to play: " + hand[maxIndex]);
			return (cardScore[maxIndex], hand[maxIndex]);
		}
	}
}
