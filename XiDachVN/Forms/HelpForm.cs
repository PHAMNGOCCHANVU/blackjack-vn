using System;
using System.Drawing;
using System.Windows.Forms;

namespace XiDachVN.Forms
{
    public class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Hướng Dẫn & Luật Chơi Xì Dách Việt Nam";
            this.Size = new Size(620, 560);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 24, 33);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            var titleLabel = new Label
            {
                Text = "📖 LUẬT CHƠI XÌ DÁCH VIỆT NAM",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(212, 175, 55),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);

            var richText = new RichTextBox
            {
                Location = new Point(20, 55),
                Size = new Size(565, 410),
                BackColor = Color.FromArgb(30, 35, 48),
                ForeColor = Color.FromArgb(235, 238, 245),
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Font = new Font("Segoe UI", 9.5f)
            };

            richText.Rtf = @"{\rtf1\ansi\deff0
{\colortbl ;\red212\green175\blue55;\red100\green200\blue100;\red255\green100\blue100;\red150\green200\blue255;}
\b\fs22\cf1 1. MỤC TIÊU TRÒ CHƠI\cf0\b0\par
Mục tiêu là đạt tổng điểm các lá bài càng gần 21 càng tốt nhưng KHÔNG ĐƯỢC VƯỢT QUÁ 21 điểm (Quắc).\par\par

\b\fs22\cf1 2. CÁCH TÍNH ĐIỂM VÀ LÁ ÁT (ACE)\cf0\b0\par
- Các lá 2 đến 10: Điểm tương ứng với số trên lá bài.\par
- Các lá J, Q, K: Tính là 10 điểm.\par
- \b\cf2 Lá Át (A) đặc biệt:\b0\cf0\par
  + \b Khi có 2 - 3 lá:\b0 Át linh hoạt nhận giá trị 1, 10 hoặc 11 điểm (tự động tối ưu).\par
  + \b Khi có 4 - 5 lá:\b0 Át \b BẮT BUỘC = 1 điểm\b0  (chống gian lận Ngũ Linh).\par\par

\b\fs22\cf1 3. THỨ TỰ BÀI ĐẶC BIỆT VÀ TỶ LỆ TRẢ THƯỞNG\cf0\b0\par
\b 1. Xì Bàng:\b0  2 lá Át (A + A) -> Thắng \b\cf2 x2 lần cược\cf0\b0 .\par
\b 2. Xì Dách:\b0  1 lá Át + 1 lá 10/J/Q/K -> Thắng \b\cf2 x1.5 lần cược\cf0\b0 .\par
\b 3. Ngũ Linh:\b0  5 lá bài có tổng điểm <= 21 -> Thắng \b\cf2 x2 lần cược\cf0\b0 .\par
   \i * Lưu ý: Cùng Ngũ Linh thì bên có tổng điểm THẤP HƠN sẽ THẮNG!\i0\par
\b 4. Bài Thường:\b0  Đạt từ 16 - 21 điểm -> Thắng \b\cf2 x1 lần cược\cf0\b0 .\par
\b 5. Quắc:\b0  Tổng điểm > 21 điểm (Xử thua).\par
   \b\cf3 * ĐẶC QUYỀN NHÀ CÁI: CÙNG QUẮC THÌ NHÀ CÁI LUÔN THẮNG!\cf0\b0\par\par

\b\fs22\cf1 4. ĐỘ TUỔI DẰN BÀI VÀ ĐẶC QUYỀN XÉT BÀI SỚM\cf0\b0\par
- \b Nhà con:\b0  Phải đủ từ \b 16 điểm\b0  trở lên mới được phép dằn bài.\par
- \b Nhà cái:\b0  Được phép dằn từ \b 15 điểm\b0 .\par
- \b Đặc quyền Xét bài sớm:\b0  Khi đạt >= 15 điểm, Nhà cái có quyền chọn xét bài các nhà con trước để thu tiền an toàn, sau đó mới quyết định có rút thêm để đối đầu với các nhà con còn lại hay không!\par\par

\b\fs22\cf1 5. CƠ CHẾ ĐỔI CÁI\cf0\b0\par
- Quỹ tối thiểu của Nhà cái = Mức cược x 20.\par
- Nếu quỹ dưới mức tối thiểu, Nhà cái bị phá sản và quyền làm Cái tự động chuyển cho người giàu nhất tiếp theo trên bàn.
}";

            this.Controls.Add(richText);

            var btnClose = new Button
            {
                Text = "Đã Hiểu",
                Location = new Point(230, 475),
                Size = new Size(140, 38),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(btnClose);
        }
    }
}
