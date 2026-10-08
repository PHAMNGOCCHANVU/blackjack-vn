using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace XiDachVN.Models
{
    /// <summary>
    /// Vẽ lá bài chuyên nghiệp, sắc nét bằng GDI+ (System.Drawing)
    /// Hỗ trợ cả mặt ngửa và mặt úp, tự động tối ưu bộ nhớ qua Card Cache.
    /// </summary>
    public static class CardRenderer
    {
        public const int CardWidth = 74;
        public const int CardHeight = 104;
        private const int CornerRadius = 7;

        private static readonly Dictionary<string, Bitmap> CardCache = new Dictionary<string, Bitmap>();
        private static Bitmap _cachedFaceDown = null;

        /// <summary>
        /// Lấy hình ảnh lá bài (mặt ngửa hoặc mặt úp)
        /// </summary>
        public static Bitmap GetCardImage(Card card)
        {
            if (card == null) return GetFaceDownImage();
            return card.IsFaceUp ? GetFaceUpImage(card) : GetFaceDownImage();
        }

        /// <summary>
        /// Tạo hoặc lấy từ cache hình ảnh mặt ngửa lá bài
        /// </summary>
        public static Bitmap GetFaceUpImage(Card card)
        {
            string key = $"{card.Rank}_{card.Suit}";
            if (CardCache.TryGetValue(key, out var cachedImg))
            {
                return cachedImg;
            }

            var bmp = new Bitmap(CardWidth, CardHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // 1. Nền thẻ bài trắng ngà có đổ bóng viền
                var rect = new Rectangle(1, 1, CardWidth - 3, CardHeight - 3);
                using (var path = CreateRoundedRectangle(rect, CornerRadius))
                {
                    // Đổ nền trắng mịn
                    using (var brush = new SolidBrush(Color.FromArgb(252, 252, 254)))
                    {
                        g.FillPath(brush, path);
                    }

                    // Viền xám mềm mại
                    using (var pen = new Pen(Color.FromArgb(170, 175, 185), 1.2f))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // 2. Màu sắc theo chất bài (Đỏ: Cơ/Rô, Đen: Chuồn/Bích)
                Color mainColor = card.IsRed ? Color.FromArgb(215, 25, 35) : Color.FromArgb(25, 25, 30);
                using (var brush = new SolidBrush(mainColor))
                {
                    // Góc trên bên trái: Rank
                    using (var rankFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
                    {
                        g.DrawString(card.Rank, rankFont, brush, 4, 3);
                    }

                    // Biểu tượng chất nhỏ dưới Rank
                    using (var suitSmallFont = new Font("Segoe UI", 8.5f, FontStyle.Regular))
                    {
                        int suitY = card.Rank == "10" ? 17 : 16;
                        g.DrawString(card.SuitSymbol, suitSmallFont, brush, 4, suitY);
                    }

                    // Biểu tượng chất lớn chính giữa
                    using (var centerFont = new Font("Segoe UI Symbol", 26f, FontStyle.Bold))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(card.SuitSymbol, centerFont, brush, new RectangleF(0, 8, CardWidth, CardHeight - 16), sf);
                    }

                    // Góc dưới bên phải (nhỏ, trang nhã)
                    using (var miniFont = new Font("Segoe UI", 8f, FontStyle.Bold))
                    {
                        var sfRight = new StringFormat { Alignment = StringAlignment.Far };
                        g.DrawString($"{card.Rank}{card.SuitSymbol}", miniFont, brush, new RectangleF(0, CardHeight - 17, CardWidth - 4, 16), sfRight);
                    }
                }
            }

            CardCache[key] = bmp;
            return bmp;
        }

        /// <summary>
        /// Tạo hoặc lấy từ cache hình ảnh mặt úp lá bài (hoa văn casino sang trọng)
        /// </summary>
        public static Bitmap GetFaceDownImage()
        {
            if (_cachedFaceDown != null)
                return _cachedFaceDown;

            var bmp = new Bitmap(CardWidth, CardHeight);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                var rect = new Rectangle(1, 1, CardWidth - 3, CardHeight - 3);
                using (var path = CreateRoundedRectangle(rect, CornerRadius))
                {
                    // Gradient xanh sapphire casino
                    using (var bgBrush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(16, 37, 76),
                        Color.FromArgb(8, 20, 48),
                        45f))
                    {
                        g.FillPath(bgBrush, path);
                    }

                    // Viền vàng kim sang trọng (Gold)
                    using (var pen = new Pen(Color.FromArgb(212, 175, 55), 1.6f))
                    {
                        g.DrawPath(pen, path);
                    }

                    // Viền lót hoa văn bên trong
                    var innerRect = new Rectangle(4, 4, CardWidth - 9, CardHeight - 9);
                    using (var innerPath = CreateRoundedRectangle(innerRect, CornerRadius - 2))
                    {
                        using (var innerPen = new Pen(Color.FromArgb(100, 212, 175, 55), 1f))
                        {
                            innerPen.DashStyle = DashStyle.Dash;
                            g.DrawPath(innerPen, innerPath);
                        }
                    }

                    // Hoa văn họa tiết trung tâm
                    using (var textFont = new Font("Segoe UI Symbol", 16f, FontStyle.Bold))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        using (var goldBrush = new SolidBrush(Color.FromArgb(212, 175, 55)))
                        {
                            g.DrawString("🂠", textFont, goldBrush, new RectangleF(0, 0, CardWidth, CardHeight), sf);
                        }
                    }
                }
            }

            _cachedFaceDown = bmp;
            return _cachedFaceDown;
        }

        private static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            var arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Góc trên trái
            path.AddArc(arc, 180, 90);

            // Góc trên phải
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Góc dưới phải
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Góc dưới trái
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}
