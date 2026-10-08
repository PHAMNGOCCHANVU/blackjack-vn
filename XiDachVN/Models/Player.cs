using System.Collections.Generic;

namespace XiDachVN.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Money { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public int Bet { get; set; }
        public bool IsDealer { get; set; }
        public bool IsTurnComplete { get; set; }
        public bool IsChecked { get; set; } // Đã được Nhà cái xét bài chưa (cho cơ chế xét sớm)
        public GameResult? RoundResult { get; set; }
        public double PayoutMultiplier { get; set; } = 1.0;
        public int NetWinLoss { get; set; }
        public string ResultNote { get; set; } = "";

        public Player()
        {
        }

        public Player(string name, int money, bool isDealer = false)
        {
            Name = name;
            Money = money;
            IsDealer = isDealer;
        }

        public void AddCard(Card card)
        {
            Hand.Add(card);
        }

        public void ResetHand()
        {
            Hand.Clear();
            Bet = 0;
            IsTurnComplete = false;
            IsChecked = false;
            RoundResult = null;
            PayoutMultiplier = 1.0;
            NetWinLoss = 0;
            ResultNote = "";
        }

        public int CardCount => Hand.Count;
    }
}
