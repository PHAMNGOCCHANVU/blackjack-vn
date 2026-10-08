using System;
using System.Collections.Generic;
using System.Linq;
using XiDachVN.Models;

namespace XiDachVN.Engine
{
    public class GameEngine
    {
        public CardDeck Deck { get; private set; }
        public Player HumanPlayer { get; private set; }
        public List<BotPlayer> Bots { get; private set; } = new List<BotPlayer>();
        public Player Dealer { get; set; }

        public int MinBet { get; set; } = 100;
        public int MaxBet => MinBet * 20;
        public int DealerMinBank => MinBet * 20;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.Waiting;

        // Sự kiện gửi cho UI
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<Player, Card> OnCardDealt;
        public event Action<Player, string> OnPlayerAction;
        public event Action<Player, HandComparer.ComparisonResult, int> OnPlayerSettled;
        public event Action<string> OnGameMessage;
        public event Action<Player, Player> OnDealerChanged;

        public GameEngine()
        {
            Deck = new CardDeck();
        }

        /// <summary>
        /// Khởi tạo bàn chơi mới
        /// </summary>
        public void SetupGame(string humanName, int humanMoney, int botCount, int minBet)
        {
            MinBet = minBet;
            HumanPlayer = new Player(string.IsNullOrWhiteSpace(humanName) ? "Bạn" : humanName, humanMoney);

            Bots.Clear();
            string[] botNames = { "Tí (Thận trọng)", "Tèo (Cân bằng)", "Sửu (Liều lĩnh)", "Dần (Cân bằng)" };
            AIBehavior[] botBehaviors = { AIBehavior.Cautious, AIBehavior.Normal, AIBehavior.Aggressive, AIBehavior.Normal };

            int effectiveBotCount = Math.Min(botCount, 4);
            for (int i = 0; i < effectiveBotCount; i++)
            {
                var bot = new BotPlayer(botNames[i], botBehaviors[i], humanMoney);
                Bots.Add(bot);
            }

            // Nhà cái mặc định ban đầu là một Bot AI chuyên trách làm Cái
            Dealer = new BotPlayer("Nhà Cái (Hệ Thống)", AIBehavior.Normal, humanMoney * 2, isDealer: true);

            CurrentPhase = GamePhase.Waiting;
            OnPhaseChanged?.Invoke(CurrentPhase);
        }

        /// <summary>
        /// Lấy tất cả người chơi có mặt tại sòng bài (Nhà cái và tất cả Nhà con)
        /// </summary>
        public List<Player> GetAllParticipants()
        {
            var list = new List<Player>();
            if (HumanPlayer != null) list.Add(HumanPlayer);
            if (Bots != null)
            {
                foreach (var b in Bots)
                {
                    if (!list.Contains(b)) list.Add(b);
                }
            }
            if (Dealer != null && !list.Contains(Dealer))
            {
                list.Add(Dealer);
            }
            return list;
        }

        /// <summary>
        /// Lấy tất cả nhà con tham gia ván (loại trừ Nhà cái hiện tại)
        /// </summary>
        public List<Player> GetAllNonDealerPlayers()
        {
            return GetAllParticipants().Where(p => p != Dealer).ToList();
        }

        /// <summary>
        /// Bắt đầu ván mới: xáo bài, reset bài cũ của tất cả người chơi
        /// </summary>
        public void StartNewRound()
        {
            Deck.Initialize();
            Deck.Shuffle();

            foreach (var p in GetAllParticipants())
            {
                p.ResetHand();
            }

            SetPhase(GamePhase.Betting);
            if (HumanPlayer.IsDealer)
            {
                OnGameMessage?.Invoke("Ván mới: Bạn đang làm Nhà Cái! Các nhà con đang đặt cược.");
            }
            else
            {
                OnGameMessage?.Invoke("Ván mới bắt đầu. Vui lòng đặt cược!");
            }
        }

        /// <summary>
        /// Đặt cược cho người chơi và các bot
        /// </summary>
        public void PlaceBets(int humanBet)
        {
            if (CurrentPhase != GamePhase.Betting) return;

            // Đặt cược cho Human khi làm Nhà con
            if (!HumanPlayer.IsDealer)
            {
                HumanPlayer.Bet = Math.Max(MinBet, Math.Min(humanBet, HumanPlayer.Money));
                OnGameMessage?.Invoke($"{HumanPlayer.Name} đã cược {HumanPlayer.Bet:#,##0} VNĐ.");
            }
            else
            {
                HumanPlayer.Bet = 0; // Nhà cái không cược vào bài của mình
            }

            // Đặt cược tự động cho các Bot làm Nhà con
            foreach (var p in GetAllNonDealerPlayers())
            {
                if (p is BotPlayer bot)
                {
                    bot.Bet = bot.CalculateBet(MinBet, MaxBet);
                }
            }

            SetPhase(GamePhase.Dealing);
        }

        /// <summary>
        /// Chia 2 lá bài ban đầu cho tất cả mọi người
        /// </summary>
        public void DealInitialCards()
        {
            if (CurrentPhase != GamePhase.Dealing) return;

            var nonDealers = GetAllNonDealerPlayers();

            // Vòng 1
            foreach (var p in nonDealers)
            {
                var card = Deck.Draw();
                card.IsFaceUp = true;
                p.AddCard(card);
                OnCardDealt?.Invoke(p, card);
            }
            var dealerCard1 = Deck.Draw();
            dealerCard1.IsFaceUp = true; // Lá 1 của Nhà cái lật ngửa
            Dealer.AddCard(dealerCard1);
            OnCardDealt?.Invoke(Dealer, dealerCard1);

            // Vòng 2
            foreach (var p in nonDealers)
            {
                var card = Deck.Draw();
                card.IsFaceUp = true;
                p.AddCard(card);
                OnCardDealt?.Invoke(p, card);
            }
            var dealerCard2 = Deck.Draw();
            dealerCard2.IsFaceUp = false; // Lá 2 của Nhà cái úp mặt
            Dealer.AddCard(dealerCard2);
            OnCardDealt?.Invoke(Dealer, dealerCard2);

            SetPhase(GamePhase.CheckSpecial);
        }

        /// <summary>
        /// Xét bài đặc biệt 2 lá đầu (Xì Bàng, Xì Dách)
        /// Trả về true nếu ván kết thúc ngay lập tức (Nhà cái có Xì Bàng/Xì Dách)
        /// </summary>
        public bool CheckSpecialHandsInitial()
        {
            var dealerStatus = ScoreCalculator.GetHandStatus(Dealer.Hand, isDealer: true);

            // Nếu Nhà cái có Xì Bàng hoặc Xì Dách: Lật bài ngay và so với tất cả!
            if (dealerStatus == HandStatus.XiBang || dealerStatus == HandStatus.XiDach)
            {
                // Lật lá úp của nhà cái
                RevealDealerCards();
                OnGameMessage?.Invoke($"Nhà cái có {ScoreCalculator.GetHandDescription(Dealer.Hand, true)}! So bài ngay!");

                var nonDealers = GetAllNonDealerPlayers();
                foreach (var p in nonDealers)
                {
                    SettleSinglePlayer(p);
                }

                SetPhase(GamePhase.RoundEnd);
                return true;
            }

            // Nếu nhà cái không có đặc biệt, kiểm tra các nhà con xem ai có đặc biệt để thông báo
            var nonDealerPlayers = GetAllNonDealerPlayers();
            foreach (var p in nonDealerPlayers)
            {
                var pStatus = ScoreCalculator.GetHandStatus(p.Hand, false);
                if (pStatus == HandStatus.XiBang || pStatus == HandStatus.XiDach)
                {
                    OnGameMessage?.Invoke($"{p.Name} có {ScoreCalculator.GetHandDescription(p.Hand, false)}!");
                }
            }

            // Ván tiếp tục sang lượt người chơi rút/dằn
            SetPhase(GamePhase.PlayerTurn);
            return false;
        }

        /// <summary>
        /// Người chơi thực hiện Rút bài (Hit)
        /// </summary>
        public HandStatus HumanHit()
        {
            if (CurrentPhase != GamePhase.PlayerTurn && CurrentPhase != GamePhase.DealerTurn)
                return ScoreCalculator.GetHandStatus(HumanPlayer.Hand, HumanPlayer.IsDealer);

            if (HumanPlayer.Hand.Count >= 5)
            {
                HumanPlayer.IsTurnComplete = true;
                return ScoreCalculator.GetHandStatus(HumanPlayer.Hand, HumanPlayer.IsDealer);
            }

            var card = Deck.Draw();
            card.IsFaceUp = true;
            HumanPlayer.AddCard(card);
            OnCardDealt?.Invoke(HumanPlayer, card);

            var status = ScoreCalculator.GetHandStatus(HumanPlayer.Hand, HumanPlayer.IsDealer);
            int score = ScoreCalculator.CalculateScore(HumanPlayer.Hand);

            if (status == HandStatus.Quac)
            {
                HumanPlayer.IsTurnComplete = true;
                OnPlayerAction?.Invoke(HumanPlayer, $"Quắc ({score} điểm)!");
            }
            else if (status == HandStatus.NguLinh)
            {
                HumanPlayer.IsTurnComplete = true;
                OnPlayerAction?.Invoke(HumanPlayer, $"Ngũ Linh ({score} điểm)!");
            }
            else if (HumanPlayer.Hand.Count == 5)
            {
                HumanPlayer.IsTurnComplete = true;
                OnPlayerAction?.Invoke(HumanPlayer, $"Đã đủ 5 lá ({score} điểm)!");
            }

            return status;
        }

        /// <summary>
        /// Người chơi thực hiện Dằn bài (Stand)
        /// </summary>
        public bool HumanStand()
        {
            if (CurrentPhase != GamePhase.PlayerTurn && CurrentPhase != GamePhase.DealerTurn)
                return false;

            int score = ScoreCalculator.CalculateScore(HumanPlayer.Hand);
            int minStandScore = HumanPlayer.IsDealer ? 15 : 16;
            if (score < minStandScore && HumanPlayer.Hand.Count < 5)
            {
                string roleName = HumanPlayer.IsDealer ? "Nhà cái" : "Nhà con";
                OnGameMessage?.Invoke($"Chưa đủ {minStandScore} điểm ({roleName} chưa đủ tuổi) để dằn bài!");
                return false;
            }

            HumanPlayer.IsTurnComplete = true;
            OnPlayerAction?.Invoke(HumanPlayer, $"Dằn bài ({score} điểm)");
            return true;
        }

        /// <summary>
        /// Lượt của một Bot cụ thể khi làm Nhà con
        /// </summary>
        public void ProcessSingleBotTurn(BotPlayer bot)
        {
            while (!bot.IsTurnComplete && bot.Hand.Count < 5)
            {
                int score = ScoreCalculator.CalculateScore(bot.Hand);
                var status = ScoreCalculator.GetHandStatus(bot.Hand, false);

                bool willDraw = bot.DecideAction(score, status, bot.Hand.Count);
                if (willDraw)
                {
                    var card = Deck.Draw();
                    card.IsFaceUp = true;
                    bot.AddCard(card);
                    OnCardDealt?.Invoke(bot, card);

                    var newStatus = ScoreCalculator.GetHandStatus(bot.Hand, false);
                    if (newStatus == HandStatus.Quac || newStatus == HandStatus.NguLinh || bot.Hand.Count >= 5)
                    {
                        bot.IsTurnComplete = true;
                        break;
                    }
                }
                else
                {
                    bot.IsTurnComplete = true;
                    break;
                }
            }

            int finalScore = ScoreCalculator.CalculateScore(bot.Hand);
            var finalStatus = ScoreCalculator.GetHandStatus(bot.Hand, false);
            OnPlayerAction?.Invoke(bot, $"{bot.Name} đã dừng ({finalScore}đ - {finalStatus})");
        }

        /// <summary>
        /// Lật toàn bộ bài của Nhà cái
        /// </summary>
        public void RevealDealerCards()
        {
            foreach (var card in Dealer.Hand)
            {
                card.IsFaceUp = true;
            }
        }

        /// <summary>
        /// Thực thi lượt Nhà cái và quy trình Xét bài sớm / So bài
        /// </summary>
        public void ProcessDealerTurnAndSettle()
        {
            SetPhase(GamePhase.DealerTurn);
            RevealDealerCards();

            // Trường hợp 1: Nhà cái là Bot AI
            if (Dealer is BotPlayer botDealer)
            {
                // 1. Rút bài ban đầu nếu chưa đủ 15 điểm
                while (Dealer.Hand.Count < 5 && DealerAI.MustDraw(ScoreCalculator.CalculateScore(Dealer.Hand), Dealer.Hand.Count))
                {
                    var card = Deck.Draw();
                    card.IsFaceUp = true;
                    Dealer.AddCard(card);
                    OnCardDealt?.Invoke(Dealer, card);
                }

                int dealerScore = ScoreCalculator.CalculateScore(Dealer.Hand);
                var dealerStatus = ScoreCalculator.GetHandStatus(Dealer.Hand, isDealer: true);
                OnPlayerAction?.Invoke(Dealer, $"Nhà cái hiện có: {ScoreCalculator.GetHandDescription(Dealer.Hand, true)}");

                // 2. Xét bài sớm (Early Check) theo chiến thuật AI
                SetPhase(GamePhase.Comparing);
                var nonDealers = GetAllNonDealerPlayers();
                var orderedPlayers = DealerAI.OrderPlayersForEarlyCheck(nonDealers, dealerScore);

                foreach (var player in orderedPlayers)
                {
                    if (player.IsChecked) continue;

                    // Nếu nhà cái chưa Quắc và điểm < 21: xem xét rút thêm trước khi xét đối thủ này
                    if (dealerStatus != HandStatus.Quac && Dealer.Hand.Count < 5)
                    {
                        bool shouldDrawMore = DealerAI.ShouldDrawBeforeCheckingPlayer(dealerScore, Dealer.Hand.Count, player);
                        if (shouldDrawMore)
                        {
                            var newCard = Deck.Draw();
                            newCard.IsFaceUp = true;
                            Dealer.AddCard(newCard);
                            OnCardDealt?.Invoke(Dealer, newCard);

                            dealerScore = ScoreCalculator.CalculateScore(Dealer.Hand);
                            dealerStatus = ScoreCalculator.GetHandStatus(Dealer.Hand, isDealer: true);
                            OnPlayerAction?.Invoke(Dealer, $"Nhà cái rút thêm trước khi xét {player.Name}: {ScoreCalculator.GetHandDescription(Dealer.Hand, true)}");
                        }
                    }

                    SettleSinglePlayer(player);
                }
            }
            else
            {
                // Trường hợp 2: Nhà cái là Người chơi (HumanPlayer)
                // Tiến hành so bài với tất cả các Nhà con
                SetPhase(GamePhase.Comparing);
                var nonDealers = GetAllNonDealerPlayers();
                foreach (var player in nonDealers)
                {
                    SettleSinglePlayer(player);
                }
            }

            SetPhase(GamePhase.RoundEnd);
        }

        /// <summary>
        /// So bài và thanh toán tiền giữa một Nhà con và Nhà cái
        /// </summary>
        public void SettleSinglePlayer(Player player)
        {
            if (player.IsChecked) return;

            var cmp = HandComparer.Compare(player, Dealer);
            player.RoundResult = cmp.Result;
            player.PayoutMultiplier = cmp.PayoutMultiplier;
            player.IsChecked = true;

            int winLossAmount = 0;
            if (cmp.Result == GameResult.Win)
            {
                winLossAmount = (int)(player.Bet * cmp.PayoutMultiplier);
                player.Money += winLossAmount;
                Dealer.Money -= winLossAmount;
                player.NetWinLoss = winLossAmount;
                player.ResultNote = $"Thắng (+{winLossAmount:#,##0})";
            }
            else if (cmp.Result == GameResult.Lose)
            {
                winLossAmount = (int)(player.Bet * cmp.PayoutMultiplier);
                winLossAmount = Math.Min(winLossAmount, player.Money);
                player.Money -= winLossAmount;
                Dealer.Money += winLossAmount;
                player.NetWinLoss = -winLossAmount;
                player.ResultNote = $"Thua (-{winLossAmount:#,##0})";
            }
            else
            {
                player.NetWinLoss = 0;
                player.ResultNote = "Hòa bài";
            }

            OnPlayerSettled?.Invoke(player, cmp, winLossAmount);
            OnGameMessage?.Invoke($"{player.Name} vs Nhà Cái: {cmp.Description} [{player.ResultNote}]");
        }

        /// <summary>
        /// Logic luân phiên chuyển giao quyền làm Cái:
        /// Sau mỗi ván đấu, kiểm tra toàn bộ người chơi trong sòng bài (Nhà cái và tất cả Nhà con).
        /// Người có số tiền lớn nhất (Money cao nhất) sẽ nhận quyền làm Nhà Cái cho ván tiếp theo.
        /// </summary>
        public bool EvaluateAndTransferDealerRole(out string message)
        {
            var all = GetAllParticipants();
            if (all.Count == 0)
            {
                message = "";
                return false;
            }

            // Tìm số tiền lớn nhất hiện tại trong sòng bài
            int maxMoney = all.Max(p => p.Money);

            // Danh sách những người chơi có số tiền lớn nhất
            var richestPlayers = all.Where(p => p.Money == maxMoney).ToList();

            // Nếu Nhà cái hiện tại đã là người có số tiền lớn nhất (hoặc đồng hạng nhất)
            if (richestPlayers.Contains(Dealer))
            {
                message = $"{Dealer.Name} tiếp tục giữ quyền làm Nhà Cái với số tiền cao nhất ({Dealer.Money:#,##0} VNĐ).";
                return false;
            }

            // Có người chơi khác có số tiền lớn hơn Nhà cái hiện tại -> Chuyển giao quyền làm Cái!
            var oldDealer = Dealer;
            var newDealer = richestPlayers.First();

            oldDealer.IsDealer = false;
            newDealer.IsDealer = true;
            Dealer = newDealer;

            message = $"Sau ván đấu, {newDealer.Name} có số tiền lớn nhất ({newDealer.Money:#,##0} VNĐ) nên quyền làm Nhà Cái được chuyển giao cho {newDealer.Name} (thay cho {oldDealer.Name})!";
            OnDealerChanged?.Invoke(oldDealer, newDealer);
            OnGameMessage?.Invoke(message);
            return true;
        }

        /// <summary>
        /// Kiểm tra đổi cái tương thích ngược
        /// </summary>
        public bool CheckDealerBankruptAndHandle(out string message)
        {
            return EvaluateAndTransferDealerRole(out message);
        }

        private void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }
    }
}
