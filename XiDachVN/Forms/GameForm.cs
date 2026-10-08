using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using XiDachVN.Data;
using XiDachVN.Engine;
using XiDachVN.Models;

namespace XiDachVN.Forms
{
    public class GameForm : Form
    {
        private readonly SaveData _saveData;
        private GameEngine _engine;

        // Bảng điều khiển cược và thao tác
        private Panel pnlBetControls;
        private Panel pnlPlayControls;
        private Label lblCurrentBetDisplay;
        private Button btnStartDeal;
        private Button btnHit;
        private Button btnStand;

        // Banner thông báo trung tâm
        private Label lblStatusBanner;
        private Label lblDeckCount;

        // UI người chơi
        private Label lblHumanName;
        private Label lblHumanMoney;
        private Label lblHumanBet;
        private Label lblHumanScore;
        private Label lblHumanStatus;
        private FlowLayoutPanel flpHumanCards;

        // UI Nhà cái
        private Label lblDealerName;
        private Label lblDealerBank;
        private Label lblDealerScore;
        private Label lblDealerStatus;
        private FlowLayoutPanel flpDealerCards;

        // UI các Bot (tối đa 4 bot)
        private Panel[] botPanels = new Panel[4];
        private Label[] botNameLabels = new Label[4];
        private Label[] botMoneyLabels = new Label[4];
        private Label[] botBetLabels = new Label[4];
        private Label[] botScoreLabels = new Label[4];
        private Label[] botStatusLabels = new Label[4];
        private FlowLayoutPanel[] botCardPanels = new FlowLayoutPanel[4];

        // Mức cược đang chuẩn bị đặt
        private int _stagedBet = 1000;

        public GameForm(SaveData saveData)
        {
            _saveData = saveData;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            InitializeComponent();
            SetupGameEngine();
        }

        private void InitializeComponent()
        {
            this.Text = "Xì Dách Việt Nam – Sòng Bài Dân Gian";
            this.Size = new Size(1140, 780);
            this.MinimumSize = new Size(1080, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(12, 45, 24); // Xanh nỉ bàn casino
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);
            this.KeyPreview = true;

            // Sự kiện phím tắt (H: Hit, S: Stand, Space: Deal)
            this.KeyDown += GameForm_KeyDown;

            // 1. Top Bar (Các nút chức năng nhỏ và tiêu đề)
            CreateTopBar();

            // 2. Khu vực Nhà cái (Ở trên cùng)
            CreateDealerArea();

            // 3. Khu vực Bot (Hai bên cánh bàn)
            CreateBotAreas();

            // 4. Khu vực Trung tâm (Khay bài & Thông báo)
            CreateCenterArea();

            // 5. Khu vực Người chơi chính (Ở dưới cùng)
            CreateHumanPlayerArea();

            // 6. Khu vực Điều khiển (Cược & Rút/Dằn)
            CreateActionControlPanels();
        }

        private void CreateTopBar()
        {
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(180, 8, 25, 15)
            };

            var lblLogo = new Label
            {
                Text = "♠ ♥ XÌ DÁCH VIỆT NAM ♦ ♣",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(235, 195, 65),
                Location = new Point(16, 10),
                AutoSize = true
            };
            topPanel.Controls.Add(lblLogo);

            int btnX = 640;
            void AddQuickButton(string text, EventHandler onClick)
            {
                var btn = new Button
                {
                    Text = text,
                    Location = new Point(btnX, 6),
                    Size = new Size(88, 30),
                    BackColor = Color.FromArgb(30, 40, 55),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += onClick;
                topPanel.Controls.Add(btn);
                btnX += 94;
            }

            AddQuickButton("📊 Thành Tích", (s, e) => new StatsForm(_saveData).ShowDialog(this));
            AddQuickButton("⚙ Cài Đặt", (s, e) =>
            {
                var sf = new SettingsForm(_saveData);
                if (sf.ShowDialog(this) == DialogResult.OK)
                {
                    // Áp dụng cược mặc định mới nếu cần
                    _stagedBet = _saveData.Settings.DefaultBet;
                    UpdateBetDisplay();
                }
            });
            AddQuickButton("❓ Luật Chơi", (s, e) => new HelpForm().ShowDialog(this));
            AddQuickButton("🚪 Về Menu", (s, e) => this.Close());

            this.Controls.Add(topPanel);
        }

        private void CreateDealerArea()
        {
            var dealerPanel = new Panel
            {
                Location = new Point(360, 50),
                Size = new Size(420, 160),
                BackColor = Color.FromArgb(160, 15, 30, 20)
            };
            dealerPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(212, 175, 55), 1.5f))
                    e.Graphics.DrawRectangle(pen, 0, 0, dealerPanel.Width - 1, dealerPanel.Height - 1);
            };

            var badge = new Label
            {
                Text = "👑 NHÀ CÁI",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(12, 8),
                AutoSize = true
            };

            lblDealerName = new Label
            {
                Text = "Hệ Thống AI",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.White,
                Location = new Point(120, 9),
                AutoSize = true
            };

            lblDealerBank = new Label
            {
                Text = "Quỹ: 250,000 VNĐ",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(144, 238, 144),
                Location = new Point(230, 9),
                AutoSize = true
            };

            lblDealerScore = new Label
            {
                Text = "Điểm: --",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(12, 33),
                AutoSize = true
            };

            lblDealerStatus = new Label
            {
                Text = "Chờ ván mới",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.Khaki,
                Location = new Point(180, 33),
                Size = new Size(225, 18),
                TextAlign = ContentAlignment.TopRight
            };

            flpDealerCards = new FlowLayoutPanel
            {
                Location = new Point(10, 54),
                Size = new Size(400, 102),
                AutoScroll = false,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            dealerPanel.Controls.Add(badge);
            dealerPanel.Controls.Add(lblDealerName);
            dealerPanel.Controls.Add(lblDealerBank);
            dealerPanel.Controls.Add(lblDealerScore);
            dealerPanel.Controls.Add(lblDealerStatus);
            dealerPanel.Controls.Add(flpDealerCards);

            this.Controls.Add(dealerPanel);
        }

        private void CreateBotAreas()
        {
            // Bot 0, Bot 1 bên trái
            int[] leftY = { 50, 225 };
            // Bot 2, Bot 3 bên phải
            int[] rightY = { 50, 225 };

            for (int i = 0; i < 4; i++)
            {
                int posX = (i < 2) ? 16 : 800;
                int posY = (i < 2) ? leftY[i] : rightY[i - 2];

                var panel = new Panel
                {
                    Location = new Point(posX, posY),
                    Size = new Size(325, 165),
                    BackColor = Color.FromArgb(140, 12, 28, 20)
                };

                int botIndex = i;
                panel.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Color.FromArgb(100, 180, 140), 1f))
                        e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                };

                botNameLabels[i] = new Label
                {
                    Text = $"Bot {i + 1}",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(8, 6),
                    AutoSize = true
                };

                botMoneyLabels[i] = new Label
                {
                    Text = "Tiền: 50,000",
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.LightGreen,
                    Location = new Point(130, 7),
                    AutoSize = true
                };

                botBetLabels[i] = new Label
                {
                    Text = "Cược: 0",
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.Gold,
                    Location = new Point(230, 7),
                    AutoSize = true
                };

                botScoreLabels[i] = new Label
                {
                    Text = "Điểm: --",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.Cyan,
                    Location = new Point(8, 28),
                    AutoSize = true
                };

                botStatusLabels[i] = new Label
                {
                    Text = "Chờ",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = Color.Khaki,
                    Location = new Point(110, 28),
                    Size = new Size(205, 16),
                    TextAlign = ContentAlignment.TopRight
                };

                botCardPanels[i] = new FlowLayoutPanel
                {
                    Location = new Point(6, 48),
                    Size = new Size(312, 110),
                    AutoScroll = false,
                    WrapContents = false,
                    BackColor = Color.Transparent
                };

                panel.Controls.Add(botNameLabels[i]);
                panel.Controls.Add(botMoneyLabels[i]);
                panel.Controls.Add(botBetLabels[i]);
                panel.Controls.Add(botScoreLabels[i]);
                panel.Controls.Add(botStatusLabels[i]);
                panel.Controls.Add(botCardPanels[i]);

                botPanels[i] = panel;
                this.Controls.Add(panel);
            }
        }

        private void CreateCenterArea()
        {
            var centerPanel = new Panel
            {
                Location = new Point(360, 218),
                Size = new Size(420, 172),
                BackColor = Color.FromArgb(120, 8, 20, 14)
            };

            // Khay bài
            var picDeck = new PictureBox
            {
                Location = new Point(18, 12),
                Size = new Size(CardRenderer.CardWidth, CardRenderer.CardHeight),
                Image = CardRenderer.GetFaceDownImage(),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            centerPanel.Controls.Add(picDeck);

            lblDeckCount = new Label
            {
                Text = "Cọc bài: 52 lá",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(200, 210, 225),
                Location = new Point(14, 122),
                Width = 85,
                TextAlign = ContentAlignment.MiddleCenter
            };
            centerPanel.Controls.Add(lblDeckCount);

            // Bảng thông báo trạng thái bàn chơi
            lblStatusBanner = new Label
            {
                Location = new Point(110, 15),
                Size = new Size(295, 135),
                BackColor = Color.FromArgb(160, 20, 30, 42),
                ForeColor = Color.FromArgb(255, 235, 160),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Chào mừng đến sòng bài Xì Dách!\nHãy đặt mức cược và nhấn [BẮT ĐẦU VÁN]."
            };
            centerPanel.Controls.Add(lblStatusBanner);

            this.Controls.Add(centerPanel);
        }

        private void CreateHumanPlayerArea()
        {
            var humanPanel = new Panel
            {
                Location = new Point(230, 396),
                Size = new Size(680, 195),
                BackColor = Color.FromArgb(170, 18, 36, 26)
            };
            humanPanel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(235, 195, 65), 1.8f))
                    e.Graphics.DrawRectangle(pen, 0, 0, humanPanel.Width - 1, humanPanel.Height - 1);
            };

            lblHumanName = new Label
            {
                Text = $"👤 {_saveData.PlayerName}",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(14, 8),
                AutoSize = true
            };

            lblHumanMoney = new Label
            {
                Text = $"Ví: {_saveData.Money:#,##0} VNĐ",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 240, 140),
                Location = new Point(200, 9),
                AutoSize = true
            };

            lblHumanBet = new Label
            {
                Text = "Cược ván này: 1,000 VNĐ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.Orange,
                Location = new Point(440, 9),
                AutoSize = true
            };

            lblHumanScore = new Label
            {
                Text = "Điểm: --",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(14, 38),
                AutoSize = true
            };

            lblHumanStatus = new Label
            {
                Text = "Đang chờ cược",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.LightYellow,
                Location = new Point(200, 38),
                Size = new Size(460, 22),
                TextAlign = ContentAlignment.TopRight
            };

            flpHumanCards = new FlowLayoutPanel
            {
                Location = new Point(14, 66),
                Size = new Size(650, 120),
                AutoScroll = false,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            humanPanel.Controls.Add(lblHumanName);
            humanPanel.Controls.Add(lblHumanMoney);
            humanPanel.Controls.Add(lblHumanBet);
            humanPanel.Controls.Add(lblHumanScore);
            humanPanel.Controls.Add(lblHumanStatus);
            humanPanel.Controls.Add(flpHumanCards);

            this.Controls.Add(humanPanel);
        }

        private void CreateActionControlPanels()
        {
            // Panel đặt cược (hiện khi phase == Betting)
            pnlBetControls = new Panel
            {
                Location = new Point(180, 600),
                Size = new Size(780, 125),
                BackColor = Color.FromArgb(190, 16, 26, 36)
            };

            var lblBetPrompt = new Label
            {
                Text = "CHỌN MỨC CƯỢC:",
                Location = new Point(20, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.LightGray
            };
            pnlBetControls.Controls.Add(lblBetPrompt);

            lblCurrentBetDisplay = new Label
            {
                Text = "CƯỢC: 1,000 VNĐ",
                Location = new Point(170, 12),
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.Gold
            };
            pnlBetControls.Controls.Add(lblCurrentBetDisplay);

            int chipX = 20;
            void AddChip(string title, int value)
            {
                var btn = new Button
                {
                    Text = title,
                    Location = new Point(chipX, 42),
                    Size = new Size(82, 34),
                    BackColor = Color.FromArgb(32, 60, 90),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) =>
                {
                    _stagedBet = Math.Min(_saveData.Money, _stagedBet + value);
                    UpdateBetDisplay();
                };
                pnlBetControls.Controls.Add(btn);
                chipX += 88;
            }

            AddChip("+1K", 1000);
            AddChip("+5K", 5000);
            AddChip("+10K", 10000);
            AddChip("+50K", 50000);

            var btnAllIn = new Button
            {
                Text = "TẤT TAY",
                Location = new Point(chipX, 42),
                Size = new Size(82, 34),
                BackColor = Color.FromArgb(160, 40, 40),
                ForeColor = Color.Gold,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnAllIn.FlatAppearance.BorderSize = 0;
            btnAllIn.Click += (s, e) =>
            {
                _stagedBet = _saveData.Money;
                UpdateBetDisplay();
            };
            pnlBetControls.Controls.Add(btnAllIn);

            var btnResetBet = new Button
            {
                Text = "LÀM LẠI",
                Location = new Point(chipX + 88, 42),
                Size = new Size(82, 34),
                BackColor = Color.FromArgb(70, 75, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            btnResetBet.FlatAppearance.BorderSize = 0;
            btnResetBet.Click += (s, e) =>
            {
                _stagedBet = _engine?.MinBet ?? 1000;
                UpdateBetDisplay();
            };
            pnlBetControls.Controls.Add(btnResetBet);

            btnStartDeal = new Button
            {
                Text = "🎲 BẮT ĐẦU VÁN (CHIA BÀI)",
                Location = new Point(230, 82),
                Size = new Size(320, 38),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStartDeal.FlatAppearance.BorderSize = 0;
            btnStartDeal.Click += BtnStartDeal_Click;
            pnlBetControls.Controls.Add(btnStartDeal);

            this.Controls.Add(pnlBetControls);

            // Panel rút/dằn (hiện khi phase == PlayerTurn)
            pnlPlayControls = new Panel
            {
                Location = new Point(230, 600),
                Size = new Size(680, 110),
                BackColor = Color.FromArgb(190, 16, 26, 36),
                Visible = false
            };

            btnHit = new Button
            {
                Text = "🃏 RÚT BÀI (HIT) [H / Space]",
                Location = new Point(60, 28),
                Size = new Size(250, 55),
                BackColor = Color.FromArgb(24, 100, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnHit.FlatAppearance.BorderSize = 0;
            btnHit.Click += BtnHit_Click;

            btnStand = new Button
            {
                Text = "✋ DẰN BÀI (STAND) [S / Enter]",
                Location = new Point(360, 28),
                Size = new Size(250, 55),
                BackColor = Color.FromArgb(190, 80, 20),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnStand.FlatAppearance.BorderSize = 0;
            btnStand.Click += BtnStand_Click;

            pnlPlayControls.Controls.Add(btnHit);
            pnlPlayControls.Controls.Add(btnStand);

            this.Controls.Add(pnlPlayControls);
        }

        private void SetupGameEngine()
        {
            _engine = new GameEngine();
            _engine.SetupGame(
                _saveData.PlayerName,
                _saveData.Money,
                _saveData.Settings.BotCount,
                _saveData.Settings.DefaultBet);

            _stagedBet = _engine.MinBet;
            UpdateBetDisplay();

            // Đăng ký sự kiện
            _engine.OnGameMessage += msg =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    lblStatusBanner.Text = msg;
                });
            };

            // Cập nhật giao diện ban đầu
            RefreshAllUI();
        }

        private void UpdateBetDisplay()
        {
            lblCurrentBetDisplay.Text = $"CƯỢC: {_stagedBet:#,##0} VNĐ";
            lblHumanBet.Text = $"Cược ván này: {_stagedBet:#,##0} VNĐ";
        }

        private void RefreshAllUI()
        {
            // Human
            lblHumanName.Text = $"👤 {_engine.HumanPlayer.Name}";
            lblHumanMoney.Text = $"Ví: {_engine.HumanPlayer.Money:#,##0} VNĐ";
            int humanScore = ScoreCalculator.CalculateScore(_engine.HumanPlayer.Hand);
            lblHumanScore.Text = _engine.HumanPlayer.Hand.Count > 0 ? $"Điểm: {humanScore}" : "Điểm: --";
            lblHumanStatus.Text = _engine.HumanPlayer.ResultNote != "" ? _engine.HumanPlayer.ResultNote :
                (_engine.HumanPlayer.Hand.Count > 0 ? ScoreCalculator.GetHandDescription(_engine.HumanPlayer.Hand, false) : "Sẵn sàng");
            RenderCardsToPanel(flpHumanCards, _engine.HumanPlayer.Hand);

            // Dealer
            lblDealerName.Text = _engine.Dealer.Name;
            lblDealerBank.Text = $"Quỹ: {_engine.Dealer.Money:#,##0} VNĐ";
            bool dealerHasHidden = _engine.Dealer.Hand.Any(c => !c.IsFaceUp);
            if (_engine.Dealer.Hand.Count > 0 && !dealerHasHidden)
            {
                int dScore = ScoreCalculator.CalculateScore(_engine.Dealer.Hand);
                lblDealerScore.Text = $"Điểm: {dScore}";
                lblDealerStatus.Text = ScoreCalculator.GetHandDescription(_engine.Dealer.Hand, true);
            }
            else if (_engine.Dealer.Hand.Count > 0)
            {
                lblDealerScore.Text = "Điểm: ??";
                lblDealerStatus.Text = "Lá 2 đang úp";
            }
            else
            {
                lblDealerScore.Text = "Điểm: --";
                lblDealerStatus.Text = "Chờ ván";
            }
            RenderCardsToPanel(flpDealerCards, _engine.Dealer.Hand);

            // Bots
            for (int i = 0; i < 4; i++)
            {
                if (i < _engine.Bots.Count)
                {
                    var bot = _engine.Bots[i];
                    botPanels[i].Visible = true;
                    botNameLabels[i].Text = bot.Name;
                    botMoneyLabels[i].Text = $"Tiền: {bot.Money:#,##0}";
                    botBetLabels[i].Text = $"Cược: {bot.Bet:#,##0}";

                    int bScore = ScoreCalculator.CalculateScore(bot.Hand);
                    botScoreLabels[i].Text = bot.Hand.Count > 0 ? $"Điểm: {bScore}" : "Điểm: --";
                    botStatusLabels[i].Text = bot.ResultNote != "" ? bot.ResultNote :
                        (bot.Hand.Count > 0 ? ScoreCalculator.GetHandDescription(bot.Hand, false) : "Chờ");

                    RenderCardsToPanel(botCardPanels[i], bot.Hand);
                }
                else
                {
                    botPanels[i].Visible = false;
                }
            }

            // Cọc bài
            lblDeckCount.Text = $"Cọc bài: {_engine.Deck.RemainingCards} lá";
        }

        private void RenderCardsToPanel(FlowLayoutPanel panel, List<Card> cards)
        {
            panel.SuspendLayout();
            panel.Controls.Clear();
            foreach (var card in cards)
            {
                var pic = new PictureBox
                {
                    Size = new Size(CardRenderer.CardWidth, CardRenderer.CardHeight),
                    Image = CardRenderer.GetCardImage(card),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Margin = new Padding(2, 2, 2, 2)
                };
                panel.Controls.Add(pic);
            }
            panel.ResumeLayout();
        }

        // ================= XỬ LÝ LUỒNG CHƠI (GAME LOOP) =================

        private async void BtnStartDeal_Click(object sender, EventArgs e)
        {
            if (_engine.HumanPlayer.Money <= 0)
            {
                MessageBox.Show("Bạn đã hết tiền! Bàn chơi sẽ cấp cho bạn 10,000 VNĐ tiền viện trợ.", "Hết Tiền", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _engine.HumanPlayer.Money = 10000;
                _saveData.Money = 10000;
                RefreshAllUI();
            }

            if (_stagedBet > _engine.HumanPlayer.Money)
            {
                _stagedBet = _engine.HumanPlayer.Money;
                UpdateBetDisplay();
            }

            pnlBetControls.Visible = false;
            SoundManager.Play("deal");

            // 1. Bắt đầu ván
            _engine.StartNewRound();
            RefreshAllUI();

            // 2. Đặt cược
            _engine.PlaceBets(_stagedBet);
            RefreshAllUI();
            await Task.Delay(400);

            // 3. Chia 2 lá đầu
            _engine.DealInitialCards();
            RefreshAllUI();
            await Task.Delay(600);

            // 4. Kiểm tra bài đặc biệt 2 lá đầu
            bool roundEndedEarly = _engine.CheckSpecialHandsInitial();
            RefreshAllUI();

            if (roundEndedEarly)
            {
                // Ván kết thúc sớm do Nhà cái có Xì Bàng/Xì Dách
                await CompleteRoundAsync();
                return;
            }

            // Kiểm tra xem bài của người chơi có Xì Bàng/Xì Dách hay không
            var humanStatus = ScoreCalculator.GetHandStatus(_engine.HumanPlayer.Hand, false);
            if (humanStatus == HandStatus.XiBang || humanStatus == HandStatus.XiDach)
            {
                lblStatusBanner.Text = $"Bạn sở hữu {ScoreCalculator.GetHandDescription(_engine.HumanPlayer.Hand, false)}! Đang chờ lượt nhà cái...";
                _engine.HumanPlayer.IsTurnComplete = true;
                await ProcessRemainingGameLoopAsync();
                return;
            }

            // Chuyển sang lượt người chơi rút bài
            pnlPlayControls.Visible = true;
            UpdatePlayerActionButtons();
            lblStatusBanner.Text = "LƯỢT CỦA BẠN!\nChọn [Rút bài] hoặc [Dằn bài] (phải >= 16 điểm).";
        }

        private async void BtnHit_Click(object sender, EventArgs e)
        {
            SoundManager.Play("draw");
            var status = _engine.HumanHit();
            RefreshAllUI();

            int score = ScoreCalculator.CalculateScore(_engine.HumanPlayer.Hand);

            if (status == HandStatus.Quac)
            {
                SoundManager.Play("bust");
                lblStatusBanner.Text = $"Ôi không! Bạn bị QUẮC ({score} điểm)!";
                pnlPlayControls.Visible = false;
                await Task.Delay(800);
                await ProcessRemainingGameLoopAsync();
            }
            else if (status == HandStatus.NguLinh)
            {
                SoundManager.Play("win");
                lblStatusBanner.Text = $"XUẤT SẮC! BẠN ĐẠT NGŨ LINH ({score} điểm)!";
                pnlPlayControls.Visible = false;
                await Task.Delay(800);
                await ProcessRemainingGameLoopAsync();
            }
            else if (_engine.HumanPlayer.Hand.Count >= 5)
            {
                lblStatusBanner.Text = $"Bạn đã rút đủ 5 lá ({score} điểm)!";
                pnlPlayControls.Visible = false;
                await Task.Delay(800);
                await ProcessRemainingGameLoopAsync();
            }
            else
            {
                UpdatePlayerActionButtons();
            }
        }

        private async void BtnStand_Click(object sender, EventArgs e)
        {
            if (_engine.HumanStand())
            {
                pnlPlayControls.Visible = false;
                int score = ScoreCalculator.CalculateScore(_engine.HumanPlayer.Hand);
                lblStatusBanner.Text = $"Bạn đã DẰN BÀI tại {score} điểm.\nĐang xử lý các lượt tiếp theo...";
                await Task.Delay(500);
                await ProcessRemainingGameLoopAsync();
            }
            else
            {
                MessageBox.Show("Bạn chưa đủ 16 điểm (Chưa đủ tuổi) để dằn bài!", "Cảnh Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdatePlayerActionButtons()
        {
            int score = ScoreCalculator.CalculateScore(_engine.HumanPlayer.Hand);
            // Dằn bài chỉ sáng khi >= 16 điểm hoặc đã 5 lá
            btnStand.Enabled = (score >= 16 || _engine.HumanPlayer.Hand.Count >= 5);
            btnHit.Enabled = (_engine.HumanPlayer.Hand.Count < 5 && score <= 21);
        }

        private async Task ProcessRemainingGameLoopAsync()
        {
            // 1. Lượt của các Bot
            foreach (var bot in _engine.Bots)
            {
                if (bot.IsDealer || bot.IsTurnComplete) continue;

                lblStatusBanner.Text = $"{bot.Name} đang suy nghĩ...";
                await Task.Delay(600);
                _engine.ProcessSingleBotTurn(bot);
                RefreshAllUI();
                await Task.Delay(400);
            }

            // 2. Lượt của Nhà cái và Xét bài sớm
            lblStatusBanner.Text = "Lượt Nhà Cái: Đang lật bài và tính toán...";
            await Task.Delay(800);

            _engine.ProcessDealerTurnAndSettle();
            RefreshAllUI();
            await Task.Delay(1000);

            // 3. Kết thúc ván và hiển thị hộp thoại
            await CompleteRoundAsync();
        }

        private async Task CompleteRoundAsync()
        {
            RefreshAllUI();

            // Ghi nhận thành tích
            bool isWin = _engine.HumanPlayer.RoundResult == GameResult.Win;
            bool isDraw = _engine.HumanPlayer.RoundResult == GameResult.Draw;
            _saveData.RecordGame(isWin, isDraw);
            _saveData.Money = _engine.HumanPlayer.Money;
            DataManager.Save(_saveData);

            if (isWin) SoundManager.Play("win");
            else if (!isDraw) SoundManager.Play("lose");

            // Hiển thị ResultDialog
            var allParticipants = new List<Player> { _engine.HumanPlayer };
            allParticipants.AddRange(_engine.Bots);

            var dlg = new ResultDialog(allParticipants, _engine.Dealer);
            var res = dlg.ShowDialog(this);

            // Kiểm tra phá sản Nhà Cái
            if (_engine.CheckDealerBankruptAndHandle(out string bankMsg))
            {
                MessageBox.Show(bankMsg, "Đổi Nhà Cái", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            RefreshAllUI();

            if (res == DialogResult.Cancel)
            {
                // Người chơi bấm Về Menu
                this.Close();
                return;
            }

            // Bật lại panel cược cho ván mới
            pnlPlayControls.Visible = false;
            pnlBetControls.Visible = true;
            lblStatusBanner.Text = "Ván mới sẵn sàng. Hãy chọn mức cược và nhấn [BẮT ĐẦU VÁN].";
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (pnlPlayControls.Visible)
            {
                if (e.KeyCode == Keys.H)
                {
                    if (btnHit.Enabled) btnHit.PerformClick();
                }
                else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Enter)
                {
                    if (btnStand.Enabled) btnStand.PerformClick();
                }
            }
            else if (pnlBetControls.Visible)
            {
                if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
                {
                    btnStartDeal.PerformClick();
                }
            }
        }
    }
}
