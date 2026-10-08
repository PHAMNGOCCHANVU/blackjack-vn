using System;
using XiDachVN.Models;

namespace XiDachVN.Engine
{
    public static class HandComparer
    {
        public class ComparisonResult
        {
            public GameResult Result { get; set; }
            public double PayoutMultiplier { get; set; } // Hệ số ăn (1.0, 1.5, 2.0)
            public string Description { get; set; }
        }

        /// <summary>
        /// So bài giữa Nhà con (player) và Nhà cái (dealer) theo luật document-rule-game.md
        /// </summary>
        public static ComparisonResult Compare(Player player, Player dealer)
        {
            var pStatus = ScoreCalculator.GetHandStatus(player.Hand, isDealer: false);
            var dStatus = ScoreCalculator.GetHandStatus(dealer.Hand, isDealer: true);

            int pScore = ScoreCalculator.CalculateScore(player.Hand);
            int dScore = ScoreCalculator.CalculateScore(dealer.Hand);

            // 1. Kiểm tra trường hợp phạm luật / Chưa đủ tuổi
            if (pStatus == HandStatus.ChuaDuTuoi)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Lose,
                    PayoutMultiplier = 1.0,
                    Description = "Nhà con chưa đủ tuổi (< 16 điểm) -> Xử thua!"
                };
            }

            // 2. Quy tắc Quắc
            // LUẬT ĐẶC BIỆT: Cùng Quắc -> Nhà cái LUÔN THẮNG (document-rule-game.md §III)
            if (pStatus == HandStatus.Quac && dStatus == HandStatus.Quac)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Lose,
                    PayoutMultiplier = 1.0,
                    Description = "Cùng Quắc -> Đặc quyền: Nhà cái luôn thắng!"
                };
            }

            // Nhà con Quắc (Nhà cái không Quắc) -> Nhà con thua
            if (pStatus == HandStatus.Quac)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Lose,
                    PayoutMultiplier = 1.0,
                    Description = $"Nhà con Quắc ({pScore}đ) -> Thua!"
                };
            }

            // Nhà cái Quắc (Nhà con không Quắc) -> Nhà con thắng
            if (dStatus == HandStatus.Quac)
            {
                double multiplier = GetWinMultiplier(pStatus);
                return new ComparisonResult
                {
                    Result = GameResult.Win,
                    PayoutMultiplier = multiplier,
                    Description = $"Nhà cái Quắc ({dScore}đ) -> Nhà con thắng (x{multiplier})!"
                };
            }

            // 3. So sánh thứ hạng bài (Xì Bàng > Xì Dách > Ngũ Linh > Bài thường)
            int pRank = GetRankOrder(pStatus);
            int dRank = GetRankOrder(dStatus);

            if (pRank < dRank) // Hạng nhỏ hơn nghĩa là mạnh hơn
            {
                double multiplier = GetWinMultiplier(pStatus);
                return new ComparisonResult
                {
                    Result = GameResult.Win,
                    PayoutMultiplier = multiplier,
                    Description = $"Nhà con có bài mạnh hơn ({pStatus} vs {dStatus}) -> Thắng x{multiplier}!"
                };
            }
            else if (pRank > dRank) // Nhà cái mạnh hơn
            {
                double multiplier = GetWinMultiplier(dStatus);
                return new ComparisonResult
                {
                    Result = GameResult.Lose,
                    PayoutMultiplier = multiplier,
                    Description = $"Nhà cái có bài mạnh hơn ({dStatus} vs {pStatus}) -> Nhà con thua x{multiplier}!"
                };
            }

            // 4. Cùng thứ hạng bài (Đối đầu cùng cấp)
            if (pStatus == HandStatus.XiBang)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Draw,
                    PayoutMultiplier = 0.0,
                    Description = "Cùng Xì Bàng -> Hòa bài!"
                };
            }

            if (pStatus == HandStatus.XiDach)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Draw,
                    PayoutMultiplier = 0.0,
                    Description = "Cùng Xì Dách -> Hòa bài!"
                };
            }

            if (pStatus == HandStatus.NguLinh)
            {
                // Cùng Ngũ Linh -> Bên nào tổng điểm THẤP HƠN sẽ THẮNG (document-rule-game.md §III)
                if (pScore < dScore)
                {
                    return new ComparisonResult
                    {
                        Result = GameResult.Win,
                        PayoutMultiplier = 2.0,
                        Description = $"Cùng Ngũ Linh: Điểm nhà con ({pScore}đ) thấp hơn nhà cái ({dScore}đ) -> Thắng x2!"
                    };
                }
                else if (pScore > dScore)
                {
                    return new ComparisonResult
                    {
                        Result = GameResult.Lose,
                        PayoutMultiplier = 2.0,
                        Description = $"Cùng Ngũ Linh: Điểm nhà cái ({dScore}đ) thấp hơn nhà con ({pScore}đ) -> Thua x2!"
                    };
                }
                else
                {
                    return new ComparisonResult
                    {
                        Result = GameResult.Draw,
                        PayoutMultiplier = 0.0,
                        Description = $"Cùng Ngũ Linh và cùng {pScore} điểm -> Hòa bài!"
                    };
                }
            }

            // Cùng là Bài Thường (16-21 điểm) -> So điểm số
            if (pScore > dScore)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Win,
                    PayoutMultiplier = 1.0,
                    Description = $"Nhà con ({pScore}đ) cao hơn Nhà cái ({dScore}đ) -> Thắng x1!"
                };
            }
            else if (pScore < dScore)
            {
                return new ComparisonResult
                {
                    Result = GameResult.Lose,
                    PayoutMultiplier = 1.0,
                    Description = $"Nhà con ({pScore}đ) thấp hơn Nhà cái ({dScore}đ) -> Thua x1!"
                };
            }
            else
            {
                return new ComparisonResult
                {
                    Result = GameResult.Draw,
                    PayoutMultiplier = 0.0,
                    Description = $"Cùng {pScore} điểm -> Hòa bài!"
                };
            }
        }

        private static int GetRankOrder(HandStatus status)
        {
            switch (status)
            {
                case HandStatus.XiBang: return 1;
                case HandStatus.XiDach: return 2;
                case HandStatus.NguLinh: return 3;
                case HandStatus.Normal: return 4;
                case HandStatus.Quac: return 5;
                case HandStatus.ChuaDuTuoi: return 6;
                default: return 99;
            }
        }

        public static double GetWinMultiplier(HandStatus status)
        {
            switch (status)
            {
                case HandStatus.XiBang: return 2.0;   // 1 ăn 2
                case HandStatus.XiDach: return 1.5;   // 1 ăn 1.5 (document-rule-game.md)
                case HandStatus.NguLinh: return 2.0;  // 1 ăn 2
                default: return 1.0;                   // 1 ăn 1
            }
        }
    }
}
