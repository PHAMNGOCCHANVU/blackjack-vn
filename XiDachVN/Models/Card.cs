using System;

namespace XiDachVN.Models
{
    public class Card
    {
        public CardSuit Suit { get; set; }
        public string Rank { get; set; }
        public bool IsFaceUp { get; set; } = true;

        public bool IsAce => Rank == "A";

        public bool IsTenValue => Rank == "10" || Rank == "J" || Rank == "Q" || Rank == "K";

        public int GetBaseValue()
        {
            if (IsAce) return 11;
            if (IsTenValue) return 10;
            if (int.TryParse(Rank, out int val))
                return val;
            return 0;
        }

        public string SuitSymbol
        {
            get
            {
                switch (Suit)
                {
                    case CardSuit.Hearts: return "♥";
                    case CardSuit.Diamonds: return "♦";
                    case CardSuit.Clubs: return "♣";
                    case CardSuit.Spades: return "♠";
                    default: return "?";
                }
            }
        }

        public string SuitVietnameseName
        {
            get
            {
                switch (Suit)
                {
                    case CardSuit.Hearts: return "Cơ";
                    case CardSuit.Diamonds: return "Rô";
                    case CardSuit.Clubs: return "Chuồn";
                    case CardSuit.Spades: return "Bích";
                    default: return "";
                }
            }
        }

        public bool IsRed => Suit == CardSuit.Hearts || Suit == CardSuit.Diamonds;

        public override string ToString()
        {
            return $"{Rank}{SuitSymbol}";
        }
    }
}
