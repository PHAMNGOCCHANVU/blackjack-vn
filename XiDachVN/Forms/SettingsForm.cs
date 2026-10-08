using System;
using System.Drawing;
using System.Windows.Forms;
using XiDachVN.Data;

namespace XiDachVN.Forms
{
    public class SettingsForm : Form
    {
        private NumericUpDown numBotCount;
        private NumericUpDown numDefaultBet;
        private CheckBox chkSound;
        private Button btnSave;
        private Button btnCancel;

        private SaveData _saveData;

        public SettingsForm(SaveData saveData)
        {
            _saveData = saveData;
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Cài Đặt Trò Chơi";
            this.Size = new Size(420, 360);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 24, 33);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10f);

            var titleLabel = new Label
            {
                Text = "⚙ CÀI ĐẶT TRÒ CHƠI",
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
                Size = new Size(350, 190),
                BackColor = Color.FromArgb(30, 35, 48)
            };

            // Số lượng Bot
            var lblBots = new Label
            {
                Text = "Số lượng người chơi ảo (Bot):",
                Location = new Point(20, 25),
                AutoSize = true
            };
            numBotCount = new NumericUpDown
            {
                Location = new Point(230, 23),
                Width = 90,
                Minimum = 1,
                Maximum = 4,
                Value = 3,
                BackColor = Color.FromArgb(40, 48, 64),
                ForeColor = Color.White
            };
            panel.Controls.Add(lblBots);
            panel.Controls.Add(numBotCount);

            // Mức cược mặc định
            var lblBet = new Label
            {
                Text = "Mức cược tối thiểu (VNĐ):",
                Location = new Point(20, 75),
                AutoSize = true
            };
            numDefaultBet = new NumericUpDown
            {
                Location = new Point(230, 73),
                Width = 90,
                Minimum = 500,
                Maximum = 100000,
                Increment = 500,
                Value = 1000,
                BackColor = Color.FromArgb(40, 48, 64),
                ForeColor = Color.White
            };
            panel.Controls.Add(lblBet);
            panel.Controls.Add(numDefaultBet);

            // Bật/tắt âm thanh
            chkSound = new CheckBox
            {
                Text = "Bật âm thanh hiệu ứng",
                Location = new Point(20, 125),
                AutoSize = true,
                Checked = true
            };
            panel.Controls.Add(chkSound);

            this.Controls.Add(panel);

            // Nút Lưu và Hủy
            btnSave = new Button
            {
                Text = "Lưu Cài Đặt",
                Location = new Point(70, 270),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Đóng",
                Location = new Point(220, 270),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(70, 75, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadCurrentSettings()
        {
            if (_saveData?.Settings != null)
            {
                numBotCount.Value = Math.Max(1, Math.Min(4, _saveData.Settings.BotCount));
                numDefaultBet.Value = Math.Max(500, _saveData.Settings.DefaultBet);
                chkSound.Checked = _saveData.Settings.SoundEnabled;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_saveData != null)
            {
                _saveData.Settings.BotCount = (int)numBotCount.Value;
                _saveData.Settings.DefaultBet = (int)numDefaultBet.Value;
                _saveData.Settings.SoundEnabled = chkSound.Checked;
                SoundManager.IsEnabled = chkSound.Checked;

                DataManager.Save(_saveData);
            }

            MessageBox.Show("Đã lưu cài đặt thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
