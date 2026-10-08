using System;
using System.Collections.Generic;

namespace XiDachVN.Models
{
    public class CardDeck
    {
        private readonly List<Card> _cards = new List<Card>();
        private readonly Random _rng = new Random();

        public int RemainingCards => _cards.Count;

        public CardDeck()
        {
            Initialize();
        }

        public void Initialize()
        {
            _cards.Clear();
            CardSuit[] suits = { CardSuit.Hearts, CardSuit.Diamonds, CardSuit.Clubs, CardSuit.Spades };
            string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    _cards.Add(new Card
                    {
                        Suit = suit,
                        Rank = rank,
                        IsFaceUp = true
                    });
                }
            }
        }

        /// <summary>
        /// Xáo bài ngẫu nhiên theo thuật toán Fisher-Yates
        /// </summary>
        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var temp = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = temp;
            }
        }

        /// <summary>
        /// Rút một lá bài trên đầu cọc bài
        /// </summary>
        public Card Draw()
        {
            if (_cards.Count == 0)
            {
                // Nếu cọc bài hết trong lúc chơi, tự động xáo lại bộ mới
                Initialize();
                Shuffle();
            }

            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }
    }
}
