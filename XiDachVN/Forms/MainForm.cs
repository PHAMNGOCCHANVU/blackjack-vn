using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using XiDachVN.Data;

namespace XiDachVN.Forms
{
    public class MainForm : Form
    {
        private SaveData _saveData;
        private Label lblPlayerInfo;

        public MainForm()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            _saveData = DataManager.Load();
            InitializeComponent();
            UpdatePlayerDisplay();
        }

        private void InitializeComponent()
        {
            this.Text = "Xì Dách Việt Nam (Blackjack VN)";
            this.Size = new Size(600, 680);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(14, 28, 20); // Emerald Deep Green
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10f);

            // Nền vẽ gradient sang trọng
            this.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    this.ClientRectangle,
                    Color.FromArgb(16, 42, 28),
                    Color.FromArgb(8, 20, 14),
                    90f))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }

                // Viền vàng kim tinh tế
                using (var pen = new Pen(Color.FromArgb(120, 212, 175, 55), 2f))
                {
                    e.Graphics.DrawRectangle(pen, 10, 10, this.ClientSize.Width - 20, this.ClientSize.Height - 20);
                }
            };

            // Tiêu đề
            var lblTitle = new Label
            {
                Text = "♠ ♥ XÌ DÁCH VIỆT NAM ♦ ♣",
                Font = new Font("Segoe UI", 21f, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 200, 70),
                Location = new Point(0, 45),
                Size = new Size(this.ClientSize.Width, 48),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "TRÒ CHƠI DÂN GIAN – ĐỐI ĐẦU TRÍ TUỆ NHÂN TẠO",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(170, 210, 185),
                Location = new Point(0, 95),
                Size = new Size(this.ClientSize.Width, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSubtitle);

            // Thông tin người chơi hiện tại
            lblPlayerInfo = new Label
            {
                Location = new Point(50, 135),
                Size = new Size(490, 42),
                BackColor = Color.FromArgb(180, 22, 38, 28),
                ForeColor = Color.FromArgb(255, 230, 130),
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblPlayerInfo);

            // Các nút Menu chính
            int btnY = 195;
            int btnW = 320;
            int btnH = 50;
            int btnX = (this.ClientSize.Width - btnW) / 2;

            Button CreateMenuButton(string text, Color baseColor, EventHandler onClick)
            {
                var btn = new Button
                {
                    Text = text,
                    Location = new Point(btnX, btnY),
                    Size = new Size(btnW, btnH),
                    BackColor = baseColor,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(baseColor, 0.2f);
                btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
                btn.Click += onClick;
                this.Controls.Add(btn);
                btnY += 62;
                return btn;
            }

            CreateMenuButton("🎮  CHƠI MỚI", Color.FromArgb(34, 139, 34), BtnNewGame_Click);
            CreateMenuButton("▶  TIẾP TỤC", Color.FromArgb(25, 95, 160), BtnContinue_Click);
            CreateMenuButton("📊  THÀNH TÍCH", Color.FromArgb(60, 65, 110), (s, e) => new StatsForm(_saveData).ShowDialog(this));
            CreateMenuButton("⚙  CÀI ĐẶT", Color.FromArgb(85, 75, 45), (s, e) =>
            {
                var sf = new SettingsForm(_saveData);
                if (sf.ShowDialog(this) == DialogResult.OK)
                {
                    _saveData = DataManager.Load();
                    UpdatePlayerDisplay();
                }
            });
            CreateMenuButton("📖  LUẬT CHƠI", Color.FromArgb(90, 60, 45), (s, e) => new HelpForm().ShowDialog(this));
            CreateMenuButton("✕  THOÁT", Color.FromArgb(120, 35, 35), (s, e) => Application.Exit());

            // Footer bản quyền
            var lblFooter = new Label
            {
                Text = "© 2026 Xì Dách Việt Nam | Đồ án Lập trình Windows C# WinForms",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(120, 150, 135),
                Dock = DockStyle.Bottom,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblFooter);
        }

        private void UpdatePlayerDisplay()
        {
            if (_saveData != null)
            {
                lblPlayerInfo.Text = $"👤 {_saveData.PlayerName}   |   💰 {_saveData.Money:#,##0} VNĐ   |   🏆 Thắng: {_saveData.GamesWon}/{_saveData.GamesPlayed} ({_saveData.WinRate:F1}%)";
            }
        }

        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            // Hộp thoại nhập tên người chơi và vốn
            using (var prompt = new Form())
            {
                prompt.Width = 380;
                prompt.Height = 260;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Khởi Tạo Người Chơi Mới";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.BackColor = Color.FromArgb(20, 24, 33);
                prompt.ForeColor = Color.White;
                prompt.Font = new Font("Segoe UI", 9.5f);
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                var lblName = new Label { Left = 30, Top = 25, Text = "Họ và tên người chơi:", AutoSize = true };
                var txtName = new TextBox
                {
                    Left = 30,
                    Top = 50,
                    Width = 300,
                    Text = _saveData?.PlayerName ?? "Người chơi",
                    BackColor = Color.FromArgb(35, 42, 55),
                    ForeColor = Color.White
                };

                var lblFunds = new Label { Left = 30, Top = 90, Text = "Số tiền vốn khởi đầu (VNĐ):", AutoSize = true };
                var numFunds = new NumericUpDown
                {
                    Left = 30,
                    Top = 115,
                    Width = 300,
                    Minimum = 5000,
                    Maximum = 1000000,
                    Increment = 5000,
                    Value = 50000,
                    BackColor = Color.FromArgb(35, 42, 55),
                    ForeColor = Color.White
                };

                var btnConfirm = new Button
                {
                    Text = "Vào Sòng Bài",
                    Left = 50,
                    Width = 120,
                    Top = 165,
                    Height = 36,
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(34, 139, 34),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };

                var btnCancel = new Button
                {
                    Text = "Hủy Bỏ",
                    Left = 190,
                    Width = 120,
                    Top = 165,
                    Height = 36,
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(70, 75, 85),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White
                };

                prompt.Controls.Add(lblName);
                prompt.Controls.Add(txtName);
                prompt.Controls.Add(lblFunds);
                prompt.Controls.Add(numFunds);
                prompt.Controls.Add(btnConfirm);
                prompt.Controls.Add(btnCancel);
                prompt.AcceptButton = btnConfirm;
                prompt.CancelButton = btnCancel;

                if (prompt.ShowDialog(this) == DialogResult.OK)
                {
                    _saveData.PlayerName = string.IsNullOrWhiteSpace(txtName.Text) ? "Người chơi" : txtName.Text.Trim();
                    _saveData.Money = (int)numFunds.Value;
                    _saveData.GamesPlayed = 0;
                    _saveData.GamesWon = 0;
                    _saveData.GamesLost = 0;
                    _saveData.GamesDrawn = 0;
                    DataManager.Save(_saveData);
                    UpdatePlayerDisplay();

                    OpenGameTable();
                }
            }
        }

        private void BtnContinue_Click(object sender, EventArgs e)
        {
            _saveData = DataManager.Load();
            UpdatePlayerDisplay();
            OpenGameTable();
        }

        private void OpenGameTable()
        {
            this.Hide();
            using (var gameForm = new GameForm(_saveData))
            {
                gameForm.ShowDialog(this);
            }
            // Khi đóng GameForm, quay lại MainForm và reload data
            _saveData = DataManager.Load();
            UpdatePlayerDisplay();
            this.Show();
        }
    }
}
