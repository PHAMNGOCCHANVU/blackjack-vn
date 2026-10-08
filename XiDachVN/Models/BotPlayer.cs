using System;

namespace XiDachVN.Models
{
    public class BotPlayer : Player
    {
        public AIBehavior Personality { get; set; }
        private static readonly Random Rng = new Random();

        public BotPlayer(string name, AIBehavior personality, int money, bool isDealer = false)
            : base(name, money, isDealer)
        {
            Personality = personality;
        }

        /// <summary>
        /// Quyết định rút bài (true) hay dằn (false)
        /// </summary>
        public bool DecideAction(int score, HandStatus status, int cardCount)
        {
            // Đã đủ 5 lá hoặc bài đã kết thúc (Quắc, Ngũ Linh) -> Không thể rút thêm
            if (cardCount >= 5 || status == HandStatus.Quac || status == HandStatus.NguLinh)
                return false;

            // Chưa đủ tuổi (< 16 điểm) -> Bắt buộc phải rút thêm
            if (score < 16)
                return true;

            // Nếu đang có 4 lá và điểm còn nhỏ (<= 17), có cơ hội cao ăn Ngũ Linh -> nên liều rút lá thứ 5
            if (cardCount == 4 && score <= 17)
                return true;

            // Đạt 20 hoặc 21 điểm -> Dằn luôn
            if (score >= 20)
                return false;

            // Điểm trong khoảng [16 - 19]: Phụ thuộc vào tính cách AI
            switch (Personality)
            {
                case AIBehavior.Cautious:
                    // Thận trọng: đủ 16 điểm là dằn ngay để bảo toàn bài
                    return false;

                case AIBehavior.Normal:
                    // Cân bằng: 16 điểm thì rút thêm, từ 17 điểm trở lên thì dằn
                    return score < 17;

                case AIBehavior.Aggressive:
                    // Liều lĩnh: 16-17 điểm vẫn cố rút thêm, từ 18 điểm mới dằn
                    return score < 18;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Bot tính toán mức cược tự động dựa trên túi tiền và tính cách
        /// </summary>
        public int CalculateBet(int minBet, int maxBet)
        {
            if (Money <= minBet)
                return Money;

            double fraction;
            switch (Personality)
            {
                case AIBehavior.Cautious:
                    fraction = 0.05 + Rng.NextDouble() * 0.05; // 5% - 10%
                    break;
                case AIBehavior.Aggressive:
                    fraction = 0.15 + Rng.NextDouble() * 0.15; // 15% - 30%
                    break;
                case AIBehavior.Normal:
                default:
                    fraction = 0.10 + Rng.NextDouble() * 0.10; // 10% - 20%
                    break;
            }

            int bet = (int)(Money * fraction);
            // Làm tròn đến bội số của minBet
            if (minBet > 0)
            {
                bet = (bet / minBet) * minBet;
            }

            bet = Math.Max(bet, minBet);
            bet = Math.Min(bet, maxBet);
            bet = Math.Min(bet, Money);

            return bet;
        }
    }
}
