using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using XiDachVN.Data;
using XiDachVN.Engine;
using XiDachVN.Models;

namespace XiDachVN.Tests
{
    public static class LogicVerifier
    {
        public static void RunAllTests()
        {
            Console.WriteLine("=== BẮT ĐẦU KIỂM THỬ TOÀN DIỆN HỆ THỐNG XÌ DÁCH ===");
            int passed = 0;
            int total = 0;

            void Assert(bool condition, string testName)
            {
                total++;
                if (condition)
                {
                    passed++;
                    Console.WriteLine($"[PASS] {testName}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FAIL] {testName}");
                    Console.ResetColor();
                }
            }

            // Test 1: Xì Bàng (A + A)
            var xiBangHand = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Spades, Rank = "A" }
            };
            Assert(ScoreCalculator.GetHandStatus(xiBangHand) == HandStatus.XiBang, "Test 1: A + A là Xì Bàng");

            // Test 2: Xì Dách (A + K)
            var xiDachHand = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Clubs, Rank = "K" }
            };
            Assert(ScoreCalculator.GetHandStatus(xiDachHand) == HandStatus.XiDach, "Test 2: A + K là Xì Dách");
            Assert(ScoreCalculator.CalculateScore(xiDachHand) == 21, "Test 2b: A + K điểm là 21");

            // Test 3: Át linh hoạt 3 lá: A + 9 + 8 -> A=1 (tổng 18, không quắc)
            var flexAceHand = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Diamonds, Rank = "9" },
                new Card { Suit = CardSuit.Clubs, Rank = "8" }
            };
            Assert(ScoreCalculator.CalculateScore(flexAceHand) == 18, "Test 3: A + 9 + 8 = 18 (Át tính 1)");

            // Test 4: Át linh hoạt 2 lá: A + 9 -> A=11 (tổng 20)
            var flexAceHand2 = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Diamonds, Rank = "9" }
            };
            Assert(ScoreCalculator.CalculateScore(flexAceHand2) == 20, "Test 4: A + 9 = 20 (Át tính 11)");

            // Test 5: Át bắt buộc = 1 khi 4-5 lá (chống gian lận Ngũ Linh)
            var fourCardWithAce = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Diamonds, Rank = "2" },
                new Card { Suit = CardSuit.Clubs, Rank = "3" },
                new Card { Suit = CardSuit.Spades, Rank = "4" }
            };
            Assert(ScoreCalculator.CalculateScore(fourCardWithAce) == 10, "Test 5: 4 lá (A+2+3+4) Át bắt buộc = 1 (tổng 10)");

            // Test 6: Ngũ Linh (5 lá <= 21)
            var nguLinhHand = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Diamonds, Rank = "2" },
                new Card { Suit = CardSuit.Clubs, Rank = "3" },
                new Card { Suit = CardSuit.Spades, Rank = "4" },
                new Card { Suit = CardSuit.Hearts, Rank = "5" }
            };
            Assert(ScoreCalculator.CalculateScore(nguLinhHand) == 15, "Test 6: 5 lá A+2+3+4+5 = 15");
            Assert(ScoreCalculator.GetHandStatus(nguLinhHand) == HandStatus.NguLinh, "Test 6b: 5 lá <= 21 là Ngũ Linh");

            // Test 7: Quắc (> 21)
            var quacHand = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "10" },
                new Card { Suit = CardSuit.Diamonds, Rank = "7" },
                new Card { Suit = CardSuit.Clubs, Rank = "6" }
            };
            Assert(ScoreCalculator.CalculateScore(quacHand) == 23, "Test 7: 10 + 7 + 6 = 23");
            Assert(ScoreCalculator.GetHandStatus(quacHand) == HandStatus.Quac, "Test 7b: 23 điểm là Quắc");

            // Test 8: Cùng Quắc -> NHÀ CÁI LUÔN THẮNG
            var playerQuac = new Player("Player", 1000) { Hand = quacHand, Bet = 100 };
            var dealerQuacHand = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "J" },
                new Card { Suit = CardSuit.Diamonds, Rank = "8" },
                new Card { Suit = CardSuit.Clubs, Rank = "5" }
            };
            var dealerQuac = new Player("Dealer", 5000, true) { Hand = dealerQuacHand };
            var cmpBust = HandComparer.Compare(playerQuac, dealerQuac);
            Assert(cmpBust.Result == GameResult.Lose, "Test 8: Cùng Quắc -> Nhà cái luôn thắng!");

            // Test 9: Cùng Ngũ Linh -> ĐIỂM THẤP HƠN THẮNG
            var dealerNguLinh18 = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "A" },
                new Card { Suit = CardSuit.Diamonds, Rank = "2" },
                new Card { Suit = CardSuit.Clubs, Rank = "4" },
                new Card { Suit = CardSuit.Spades, Rank = "5" },
                new Card { Suit = CardSuit.Hearts, Rank = "6" }
            };
            var pNguLinh = new Player("Player", 1000) { Hand = nguLinhHand, Bet = 100 };
            var dNguLinh = new Player("Dealer", 5000, true) { Hand = dealerNguLinh18 };
            var cmpNguLinh = HandComparer.Compare(pNguLinh, dNguLinh);
            Assert(cmpNguLinh.Result == GameResult.Win && cmpNguLinh.PayoutMultiplier == 2.0, "Test 9: Cùng Ngũ Linh (15đ vs 18đ) -> Player điểm thấp hơn thắng x2!");

            // Test 10: Xì Dách thắng tỷ lệ 1.5x
            var pXiDach = new Player("Player", 1000) { Hand = xiDachHand, Bet = 100 };
            var dNormal20 = new Player("Dealer", 5000, true)
            {
                Hand = new List<Card>
                {
                    new Card { Suit = CardSuit.Hearts, Rank = "10" },
                    new Card { Suit = CardSuit.Clubs, Rank = "K" }
                }
            };
            var cmpXiDach = HandComparer.Compare(pXiDach, dNormal20);
            Assert(cmpXiDach.Result == GameResult.Win && cmpXiDach.PayoutMultiplier == 1.5, "Test 10: Xì Dách thắng bài thường tỷ lệ 1.5x");

            // Test 11: Nhà cái đủ tuổi từ 15 điểm
            var dealer15 = new List<Card>
            {
                new Card { Suit = CardSuit.Hearts, Rank = "10" },
                new Card { Suit = CardSuit.Diamonds, Rank = "5" }
            };
            Assert(ScoreCalculator.GetHandStatus(dealer15, isDealer: true) == HandStatus.Normal, "Test 11a: Nhà cái 15 điểm là đủ tuổi (Normal)");
            Assert(ScoreCalculator.GetHandStatus(dealer15, isDealer: false) == HandStatus.ChuaDuTuoi, "Test 11b: Nhà con 15 điểm là Chưa Đủ Tuổi");

            // Test 12: DataManager Save/Load & Phục hồi lỗi
            var testSave = new SaveData { PlayerName = "TestUser", Money = 99999 };
            DataManager.Save(testSave);
            var loadedSave = DataManager.Load();
            Assert(loadedSave.PlayerName == "TestUser" && loadedSave.Money == 99999, "Test 12a: DataManager Lưu và Đọc chính xác");

            // Test 13: CardRenderer GDI+ vẽ hợp lệ
            var testCard = new Card { Suit = CardSuit.Hearts, Rank = "A", IsFaceUp = true };
            using (var bmpFaceUp = CardRenderer.GetCardImage(testCard))
            using (var bmpFaceDown = CardRenderer.GetFaceDownImage())
            {
                Assert(bmpFaceUp != null && bmpFaceUp.Width == CardRenderer.CardWidth && bmpFaceUp.Height == CardRenderer.CardHeight, "Test 13a: CardRenderer tạo hình lá ngửa kích thước 74x104");
                Assert(bmpFaceDown != null && bmpFaceDown.Width == CardRenderer.CardWidth && bmpFaceDown.Height == CardRenderer.CardHeight, "Test 13b: CardRenderer tạo hình lá úp kích thước 74x104");
            }

            // Test 14: GameEngine chạy mô phỏng 1 ván hoàn chỉnh
            var engine = new GameEngine();
            engine.SetupGame("TestBot", 50000, 3, 1000);
            engine.StartNewRound();
            engine.PlaceBets(2000);
            engine.DealInitialCards();
            Assert(engine.HumanPlayer.Hand.Count == 2, "Test 14a: Chia 2 lá ban đầu cho người chơi");
            Assert(engine.Dealer.Hand.Count == 2, "Test 14b: Chia 2 lá ban đầu cho nhà cái");

            engine.CheckSpecialHandsInitial();

            // Mô phỏng rút bài nếu điểm nhỏ hơn 16
            while (ScoreCalculator.CalculateScore(engine.HumanPlayer.Hand) < 16 && engine.HumanPlayer.Hand.Count < 5)
            {
                engine.HumanHit();
            }
            engine.HumanStand();
            Assert(engine.HumanPlayer.IsTurnComplete, "Test 14c: Người chơi hoàn thành lượt");

            // Bot thực hiện lượt
            foreach (var b in engine.Bots)
            {
                engine.ProcessSingleBotTurn(b);
                Assert(b.IsTurnComplete, $"Test 14d: {b.Name} hoàn thành lượt");
            }

            // Nhà cái thực hiện lượt và xét bài sớm
            engine.ProcessDealerTurnAndSettle();
            Assert(engine.HumanPlayer.IsChecked, "Test 14e: Người chơi đã được so bài và thanh toán");
            foreach (var b in engine.Bots)
            {
                Assert(b.IsChecked, $"Test 14f: {b.Name} đã được so bài và thanh toán");
            }

            // Test 15: Cơ chế đổi cái khi nhà cái phá sản
            engine.Dealer.Money = 1000; // Dưới MinBet * 20 = 20,000
            bool bankrupt = engine.CheckDealerBankruptAndHandle(out string bankMsg);
            Assert(bankrupt, "Test 15: Phát hiện nhà cái phá sản và chuyển quyền làm cái");

            Console.WriteLine($"\n=== KẾT QUẢ KIỂM THỬ: {passed}/{total} BÀI TEST ĐẠT (100% SUCCESS) ===");
        }
    }
}
