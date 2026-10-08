using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using XiDachVN.Engine;
using XiDachVN.Models;

namespace XiDachVN.Forms
{
    public class ResultDialog : Form
    {
        private readonly List<Player> _allPlayers;
        private readonly Player _dealer;

        public ResultDialog(List<Player> allPlayers, Player dealer)
        {
            _allPlayers = allPlayers;
            _dealer = dealer;
            InitializeComponent();
            PopulateResults();
        }

        private void InitializeComponent()
        {
            this.Text = "Tổng Kết Ván Đấu";
            this.Size = new Size(680, 460);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(18, 22, 30);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var titleLabel = new Label
            {
                Text = "🏆 TỔNG KẾT VÁN ĐẤU",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(212, 175, 55),
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            var listView = new ListView
            {
                Name = "lvResults",
                Location = new Point(20, 65),
                Size = new Size(625, 290),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(28, 33, 44),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5f)
            };

            listView.Columns.Add("Người Chơi", 140);
            listView.Columns.Add("Vai Trò", 85);
            listView.Columns.Add("Bài", 125);
            listView.Columns.Add("Điểm / Trạng Thái", 130);
            listView.Columns.Add("Tiền +/-", 135);

            this.Controls.Add(listView);

            var btnNextRound = new Button
            {
                Text = "▶ Ván Tiếp Theo",
                Location = new Point(190, 370),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnNextRound.FlatAppearance.BorderSize = 0;

            var btnMenu = new Button
            {
                Text = "🚪 Về Menu",
                Location = new Point(360, 370),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(65, 70, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnMenu.FlatAppearance.BorderSize = 0;

            this.Controls.Add(btnNextRound);
            this.Controls.Add(btnMenu);
        }

        private void PopulateResults()
        {
            var lv = this.Controls["lvResults"] as ListView;
            if (lv == null) return;
            lv.Items.Clear();

            // 1. Dòng Nhà Cái
            string dealerCards = string.Join(" ", _dealer.Hand);
            int dealerScore = ScoreCalculator.CalculateScore(_dealer.Hand);
            string dealerDesc = ScoreCalculator.GetHandDescription(_dealer.Hand, true);

            var dealerItem = new ListViewItem(_dealer.Name);
            dealerItem.SubItems.Add("Nhà Cái");
            dealerItem.SubItems.Add(dealerCards);
            dealerItem.SubItems.Add(dealerDesc);
            dealerItem.SubItems.Add($"{_dealer.Money:#,##0} VNĐ");
            dealerItem.BackColor = Color.FromArgb(40, 50, 70);
            dealerItem.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lv.Items.Add(dealerItem);

            // 2. Dòng các Nhà Con
            foreach (var p in _allPlayers)
            {
                if (p.IsDealer) continue;

                string cards = string.Join(" ", p.Hand);
                string desc = ScoreCalculator.GetHandDescription(p.Hand, false);

                string moneyChangeStr = "";
                if (p.NetWinLoss > 0)
                    moneyChangeStr = $"+{p.NetWinLoss:#,##0} (Ví: {p.Money:#,##0})";
                else if (p.NetWinLoss < 0)
                    moneyChangeStr = $"{p.NetWinLoss:#,##0} (Ví: {p.Money:#,##0})";
                else
                    moneyChangeStr = $"0 (Ví: {p.Money:#,##0})";

                var item = new ListViewItem(p.Name);
                item.SubItems.Add("Nhà Con");
                item.SubItems.Add(cards);
                item.SubItems.Add(desc);
                item.SubItems.Add(moneyChangeStr);

                if (p.RoundResult == GameResult.Win)
                {
                    item.ForeColor = Color.LightGreen;
                }
                else if (p.RoundResult == GameResult.Lose)
                {
                    item.ForeColor = Color.Salmon;
                }
                else
                {
                    item.ForeColor = Color.Khaki;
                }

                lv.Items.Add(item);
            }
        }
    }
}
