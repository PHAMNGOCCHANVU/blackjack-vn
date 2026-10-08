using System;
using System.Collections.Generic;
using System.Linq;
using XiDachVN.Models;

namespace XiDachVN.Engine
{
    public static class ScoreCalculator
    {
        /// <summary>
        /// Tính điểm tối ưu cho tay bài theo luật Xì Dách Việt Nam (document-rule-game.md):
        /// - 2 - 3 lá: Lá Át có giá trị linh hoạt (1, 10, hoặc 11 điểm).
        /// - 4 - 5 lá: Lá Át BẮT BUỘC chỉ tính 1 điểm (chống gian lận Ngũ Linh).
        /// </summary>
        public static int CalculateScore(List<Card> hand)
        {
            if (hand == null || hand.Count == 0)
                return 0;

            int cardCount = hand.Count;
            int aceCount = hand.Count(c => c.IsAce);
            int nonAceSum = hand.Where(c => !c.IsAce).Sum(c => c.GetBaseValue());

            // Nếu không có lá Át nào, tổng điểm chính là tổng các lá thường
            if (aceCount == 0)
                return nonAceSum;

            // 4 hoặc 5 lá bài: Tất cả lá Át bắt buộc nhận giá trị 1 điểm
            if (cardCount >= 4)
            {
                return nonAceSum + aceCount * 1;
            }

            // 2 hoặc 3 lá bài: Lá Át có thể linh hoạt là 1, 10, hoặc 11 điểm
            return FindBestAceCombination(nonAceSum, aceCount);
        }

        /// <summary>
        /// Tìm tổ hợp giá trị của các lá Át (1, 10, 11) sao cho tổng điểm gần 21 nhất mà không bị Quắc.
        /// Nếu mọi tổ hợp đều > 21, chọn tổng điểm nhỏ nhất (mỗi Át = 1).
        /// </summary>
        private static int FindBestAceCombination(int baseSum, int aceCount)
        {
            int[] possibleAceValues = { 11, 10, 1 };
            int bestValidScore = 0;

            if (aceCount == 1)
            {
                foreach (int v in possibleAceValues)
                {
                    int total = baseSum + v;
                    if (total <= 21 && total > bestValidScore)
                    {
                        bestValidScore = total;
                    }
                }
            }
            else if (aceCount == 2)
            {
                // Ví dụ 2 lá bài: A + A => Xì Bàng (HandStatus sẽ phát hiện riêng)
                // Hoặc 3 lá bài: A + A + 9 => A(1) + A(1) + 9 = 11, hoặc A(11) + A(1) + 9 = 21!
                foreach (int v1 in possibleAceValues)
                {
                    foreach (int v2 in possibleAceValues)
                    {
                        int total = baseSum + v1 + v2;
                        if (total <= 21 && total > bestValidScore)
                        {
                            bestValidScore = total;
                        }
                    }
                }
            }
            else if (aceCount == 3)
            {
                // Trường hợp 3 lá Át: A + A + A => A(11) + A(1) + A(1) = 13, hoặc A(10) + A(10) + A(1) = 21
                foreach (int v1 in possibleAceValues)
                {
                    foreach (int v2 in possibleAceValues)
                    {
                        foreach (int v3 in possibleAceValues)
                        {
                            int total = baseSum + v1 + v2 + v3;
                            if (total <= 21 && total > bestValidScore)
                            {
                                bestValidScore = total;
                            }
                        }
                    }
                }
            }

            // Nếu không có tổ hợp nào <= 21 (ví dụ bài đã quắc), gán mỗi Át = 1 để điểm nhỏ nhất có thể
            if (bestValidScore == 0)
            {
                bestValidScore = baseSum + aceCount * 1;
            }

            return bestValidScore;
        }

        /// <summary>
        /// Xác định trạng thái của tay bài (Xì Bàng, Xì Dách, Ngũ Linh, Quắc, Thường, Chưa Đủ Tuổi)
        /// isDealer: true nếu là Nhà cái (đủ tuổi từ 15 điểm), false nếu là Nhà con (đủ tuổi từ 16 điểm)
        /// </summary>
        public static HandStatus GetHandStatus(List<Card> hand, bool isDealer = false)
        {
            if (hand == null || hand.Count == 0)
                return HandStatus.ChuaDuTuoi;

            int score = CalculateScore(hand);

            // Kiểm tra bài đặc biệt 2 lá đầu
            if (hand.Count == 2)
            {
                // Xì Bàng: 2 lá Át (A + A)
                if (hand[0].IsAce && hand[1].IsAce)
                    return HandStatus.XiBang;

                // Xì Dách: 1 lá Át + 1 lá 10 điểm (10, J, Q, K)
                if ((hand[0].IsAce && hand[1].IsTenValue) || (hand[1].IsAce && hand[0].IsTenValue))
                    return HandStatus.XiDach;
            }

            // Ngũ Linh: Có đủ 5 lá bài và tổng điểm <= 21
            if (hand.Count == 5 && score <= 21)
                return HandStatus.NguLinh;

            // Quắc: Điểm vượt quá 21
            if (score > 21)
                return HandStatus.Quac;

            // Kiểm tra đủ tuổi dằn
            int minStandScore = isDealer ? 15 : 16;
            if (score < minStandScore)
                return HandStatus.ChuaDuTuoi;

            // Bài thường hợp lệ (16-21 với nhà con, 15-21 với nhà cái)
            return HandStatus.Normal;
        }

        /// <summary>
        /// Lấy mô tả chi tiết của tay bài bằng tiếng Việt
        /// </summary>
        public static string GetHandDescription(List<Card> hand, bool isDealer = false)
        {
            var status = GetHandStatus(hand, isDealer);
            int score = CalculateScore(hand);

            switch (status)
            {
                case HandStatus.XiBang:
                    return "Xì Bàng (Thắng x2)";
                case HandStatus.XiDach:
                    return "Xì Dách (Thắng x1.5)";
                case HandStatus.NguLinh:
                    return $"Ngũ Linh ({score} điểm - Thắng x2)";
                case HandStatus.Quac:
                    return $"Quắc ({score} điểm)";
                case HandStatus.ChuaDuTuoi:
                    return $"Chưa đủ tuổi ({score} điểm)";
                case HandStatus.Normal:
                default:
                    return $"{score} điểm";
            }
        }
    }
}
