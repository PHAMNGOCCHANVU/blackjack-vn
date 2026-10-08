using System;
using System.Collections.Generic;
using System.Linq;
using XiDachVN.Models;

namespace XiDachVN.Engine
{
    /// <summary>
    /// Trí tuệ nhân tạo (AI) điều khiển hành vi của Nhà cái (Dealer):
    /// - Đủ tuổi dằn: >= 15 điểm (khác với nhà con phải >= 16 điểm)
    /// - Tận dụng đặc quyền XÉT BÀI SỚM (Early Check) theo luật Xì Dách Việt Nam
    /// </summary>
    public static class DealerAI
    {
        public const int DEALER_MIN_STAND_SCORE = 15;

        /// <summary>
        /// Quyết định xem Nhà cái có bắt buộc phải rút bài ban đầu hay không
        /// </summary>
        public static bool MustDraw(int dealerScore, int cardCount)
        {
            if (cardCount >= 5) return false;
            return dealerScore < DEALER_MIN_STAND_SCORE;
        }

        /// <summary>
        /// Sắp xếp thứ tự các nhà con mà Nhà cái nên ưu tiên xét bài sớm:
        /// Chiến thuật AI:
        /// 1. Xét các nhà con đã Quắc trước (thu tiền thắng chắc, bảo toàn vốn)
        /// 2. Xét các nhà con có điểm hoặc số lá mà nhà cái có tỷ lệ thắng cao với điểm hiện tại
        /// 3. Giữ lại các nhà con "nguy hiểm" (ví dụ nghi Ngũ Linh hoặc điểm cao) để quyết định rút thêm bài sau
        /// </summary>
        public static List<Player> OrderPlayersForEarlyCheck(List<Player> activePlayers, int dealerScore)
        {
            var uncheckedPlayers = activePlayers.Where(p => !p.IsChecked).ToList();
            if (uncheckedPlayers.Count <= 1)
                return uncheckedPlayers;

            var result = new List<Player>();

            // Nhóm 1: Các nhà con đã chắc chắn Quắc
            var bustedPlayers = uncheckedPlayers
                .Where(p => ScoreCalculator.GetHandStatus(p.Hand, isDealer: false) == HandStatus.Quac)
                .ToList();
            result.AddRange(bustedPlayers);

            // Nhóm 2: Các nhà con có 2-3 lá mà điểm của họ nhỏ hơn điểm hiện tại của nhà cái (nếu biết hoặc đoán)
            var weakPlayers = uncheckedPlayers
                .Except(result)
                .Where(p =>
                {
                    int pScore = ScoreCalculator.CalculateScore(p.Hand);
                    return pScore <= dealerScore && p.Hand.Count <= 3;
                })
                .ToList();
            result.AddRange(weakPlayers);

            // Nhóm 3: Các nhà con còn lại (nghi ngờ Ngũ Linh 4-5 lá, hoặc điểm cao hơn nhà cái)
            var remainingPlayers = uncheckedPlayers.Except(result).ToList();
            result.AddRange(remainingPlayers);

            return result;
        }

        /// <summary>
        /// Nhà cái quyết định có nên rút thêm bài trước khi xét một nhà con cụ thể hay không.
        /// </summary>
        public static bool ShouldDrawBeforeCheckingPlayer(int dealerScore, int cardCount, Player targetPlayer)
        {
            if (cardCount >= 5) return false;
            if (dealerScore >= 21) return false;

            // Nếu nhà cái chưa đủ 15 điểm -> Bắt buộc rút
            if (dealerScore < DEALER_MIN_STAND_SCORE)
                return true;

            var targetStatus = ScoreCalculator.GetHandStatus(targetPlayer.Hand, isDealer: false);
            int targetScore = ScoreCalculator.CalculateScore(targetPlayer.Hand);

            // 1. Nếu đối thủ đã Quắc -> Nhà cái không rút thêm, xét ngay để ăn tiền!
            if (targetStatus == HandStatus.Quac)
                return false;

            // 2. Nếu điểm nhà cái đã là 18, 19, 20 -> Quá đẹp, không nên mạo hiểm rút thêm vì xác suất Quắc rất cao
            if (dealerScore >= 18)
            {
                // Trừ trường hợp đối thủ có Ngũ Linh và nhà cái điểm thấp hơn, nhưng 18+ vẫn nên dằn
                return false;
            }

            // 3. Nếu điểm nhà cái đang là 15-16-17:
            // Nếu đối thủ điểm cao hơn hẳn nhà cái (hoặc có Ngũ Linh):
            if (targetScore > dealerScore || targetStatus == HandStatus.NguLinh)
            {
                // Nếu giữ nguyên thì chắc chắn thua, rút thêm có hy vọng lật ngược (hoặc quắc)
                return true;
            }

            // Nếu nhà cái >= targetScore: Dằn và xét luôn
            return false;
        }
    }
}
