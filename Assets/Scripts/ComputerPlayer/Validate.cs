using System.Collections.Generic;
using System.Linq;

namespace _1K_ComputerPlayer
{
	internal static class Validate
	{
		public static bool IsLegalMove(Card clickedCard, List<Card> hand, List<Card> trick, Card.Suit? atu)
		{
			List<Card> trickCards = trick;
			if (trickCards.Count > 0)
			{
				Card baseCard = trickCards[0];

				// Rule 1. Same suit
				if (clickedCard.suit == baseCard.suit)
				{
					// Rule 2. Overtrump
					if (clickedCard.value > baseCard.value)
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
							if (card.suit == trickWinner.suit && card.value > trickWinner.value)
							{
								trickWinner = card;
							}
						}

						foreach (Card card in hand)
						{
							if (card.suit == trickWinner.suit && card.value > trickWinner.value)
							{
								//Debug.LogWarning("Invalid move. Need to overtrump with higher value of " + baseCard.suit+ "!");
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
						if (card.suit == baseCard.suit)
						{
							//Debug.LogWarning("Invalid move. Need to lay card of " + baseCard.suit+ "!");
							return false;
						}
					}

					// Player can overtrump with atu
					var currentAtuSuit = atu;
					if (currentAtuSuit != null)
					{
						List<Card> handTrumps = new List<Card>();
						foreach (Card card in hand)
						{
							if (card.suit == currentAtuSuit)
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
								if (card.value > highestPlayerAtu.value)
								{
									highestPlayerAtu = card;
								}
							}
						}
						else return true;

						List<Card> trickTrumps = new List<Card>();
						foreach (Card card in trickCards)
						{
							if (card.suit == currentAtuSuit)
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
								if (card.value > highestTrickAtu.value)
								{
									highestTrickAtu = card;
								}
							}
						}
						else
						{
							if (clickedCard.suit == currentAtuSuit)
							{
								return true;
							}
							else
							{
								//Debug.Log("Invalid move. Player has to play with trump!");
								return false;
							}
						}
						if (highestPlayerAtu.value > clickedCard.value)
						{
							//Debug.Log("Invalid move. Player has to overtrump with trump!");
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

		public static bool IsNewTrickWinner(List<Card> cards, Card.Suit? atu)
		{
			int lastCard = cards.Count - 1;
			int maxAtu = -1, max = 0;

			if (cards.Count == 1)
			{
				return true;
			}

			Card.Suit? trump = atu;
			if (trump != null)
			{
				for (int i = 0; i < cards.Count; i++)
				{
					if (cards[i].suit == trump)
						if (maxAtu == -1) maxAtu = i;
						else if (cards[i].value > cards[maxAtu].value)
						{
							maxAtu = i;
						}
				}

				if (maxAtu == -1)
				{
					if (cards[lastCard].suit == trump)
					{
						return true;
					}
					else
					{
						for (int i = 0; i < cards.Count - 1; i++)
						{
							if (cards[i].value > max && cards[0].suit == cards[i].suit)
								max = cards[i].value;
						}

						for (int i = 0; i < cards.Count - 1; i++)
						{
							if (cards[0].suit == cards[lastCard].suit && max < cards[lastCard].value)
							{
								return true;
							}
						}
					}
				}
				else
				{
					if (cards[lastCard].suit == trump && lastCard == maxAtu)
					{
						return true;
					}
					return false;
				}
			}
			else
			{
				int maxc = 0;
				for (int i = 0; i < cards.Count; i++)
				{
					if (cards[0].suit == cards[i].suit && cards[i].value > cards[maxc].value)
					{
						maxc = i;
					}
				}
				if (maxc == cards.Count - 1)
				{
					return true;
				}
			}
			return false;
		}

		public static int GetTrickScore(List<Card> trick)
		{
			return trick.Sum(t => t.value);
		}
		
		public static int GetMarriageScore(Card card, List<Card> hand, List<Card> trick)
		{
			// hand marriage
			if (trick.Count == 1 && card.rank == Card.Rank.Queen)
			{
				if (hand.Any(c => (c.suit == card.suit && c.rank == Card.Rank.King)))
				{
					return Card.MarriageValue(card.suit);
				}
			}
			// king-on-queen marriage (meldunek w biegu)
			else if (trick.Count > 1 && card.rank == Card.Rank.King)
			{
				var kingIndex = trick.IndexOf(card);
				var potentialQueen = trick[kingIndex - 1];
				if (potentialQueen.rank == Card.Rank.Queen && potentialQueen.suit == card.suit)
				{
					return Card.MarriageValue(card.suit);
				}
			}

			return 0;
		}
	}
}
