using System;
using System.Drawing;
using System.Windows.Forms;
using XiDachVN.Data;

namespace XiDachVN.Forms
{
    public class StatsForm : Form
    {
        private readonly SaveData _saveData;
        private Label lblPlayed;
        private Label lblWon;
        private Label lblLost;
        private Label lblDrawn;
        private Label lblWinRate;
        private ProgressBar prgWinRate;
        private Label lblMoney;

        public StatsForm(SaveData saveData)
        {
            _saveData = saveData;
            InitializeComponent();
            RefreshStats();
        }

        private void InitializeComponent()
        {
            this.Text = "Bảng Thành Tích & Thống Kê";
            this.Size = new Size(460, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 24, 33);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10f);

            var titleLabel = new Label
            {
                Text = "📊 BẢNG THÀNH TÍCH",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(212, 175, 55),
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            var panel = new Panel
            {
                Location = new Point(30, 65),
                Size = new Size(390, 260),
                BackColor = Color.FromArgb(30, 35, 48)
            };

            lblMoney = new Label { Location = new Point(25, 20), AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Gold };
            lblPlayed = new Label { Location = new Point(25, 55), AutoSize = true };
            lblWon = new Label { Location = new Point(25, 85), AutoSize = true, ForeColor = Color.LightGreen };
            lblDrawn = new Label { Location = new Point(25, 115), AutoSize = true, ForeColor = Color.LightYellow };
            lblLost = new Label { Location = new Point(25, 145), AutoSize = true, ForeColor = Color.Salmon };

            lblWinRate = new Label { Location = new Point(25, 180), AutoSize = true, Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = Color.Cyan };
            prgWinRate = new ProgressBar
            {
                Location = new Point(25, 210),
                Size = new Size(340, 22),
                Minimum = 0,
                Maximum = 100
            };

            panel.Controls.Add(lblMoney);
            panel.Controls.Add(lblPlayed);
            panel.Controls.Add(lblWon);
            panel.Controls.Add(lblDrawn);
            panel.Controls.Add(lblLost);
            panel.Controls.Add(lblWinRate);
            panel.Controls.Add(prgWinRate);

            this.Controls.Add(panel);

            var btnReset = new Button
            {
                Text = "Đặt Lại",
                Location = new Point(90, 335),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(160, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += BtnReset_Click;

            var btnClose = new Button
            {
                Text = "Đóng",
                Location = new Point(240, 335),
                Size = new Size(130, 38),
                BackColor = Color.FromArgb(70, 75, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnReset);
            this.Controls.Add(btnClose);
        }

        private void RefreshStats()
        {
            if (_saveData == null) return;

            lblMoney.Text = $"💰 Số dư ví: {_saveData.Money:#,##0} VNĐ";
            lblPlayed.Text = $"Tổng số ván đã chơi: {_saveData.GamesPlayed}";
            lblWon.Text = $"Ván thắng: {_saveData.GamesWon}";
            lblDrawn.Text = $"Ván hòa: {_saveData.GamesDrawn}";
            lblLost.Text = $"Ván thua: {_saveData.GamesLost}";

            double rate = _saveData.WinRate;
            lblWinRate.Text = $"Tỷ lệ chiến thắng: {rate:F1}%";
            prgWinRate.Value = Math.Max(0, Math.Min(100, (int)Math.Round(rate)));
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            var res = MessageBox.Show(
                "Bạn có chắc muốn đặt lại toàn bộ thống kê (số ván chơi, tỷ lệ thắng)?",
                "Xác Nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res == DialogResult.Yes && _saveData != null)
            {
                _saveData.GamesPlayed = 0;
                _saveData.GamesWon = 0;
                _saveData.GamesLost = 0;
                _saveData.GamesDrawn = 0;
                DataManager.Save(_saveData);
                RefreshStats();
            }
        }
    }
}
