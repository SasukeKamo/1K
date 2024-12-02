using System;

namespace _1K_ComputerPlayer
{
	class Card
	{
		public enum Suit { Hearts, Diamonds, Clubs, Spades };
		public enum Rank { Nine, Ten, Jack, Queen, King, Ace };

		public static int MarriageValue(Suit _suit)
		{
			return _suit switch
			{
				Suit.Hearts => 100,
				Suit.Diamonds => 80,
				Suit.Clubs => 60,
				Suit.Spades => 40,
				_ => 40
			};
		}

		public Suit suit;
		public Rank rank;
		public int value;

		public override bool Equals(object obj)
		{
			return obj is Card card &&
			       suit == card.suit &&
			       rank == card.rank;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(suit, rank);
		}

		public override string ToString()
		{
			return "card " + suit + " " + rank;
		}

		public Card(Suit _suit, Rank _rank)
		{
			suit = _suit;
			rank = _rank;
			switch (rank)
			{
				case Rank.Nine:
					value = 0;
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
				case Rank.Ten:
					value = 10;
					break;
				case Rank.Ace:
					value = 11;
					break;
			}
		}
	}
}
