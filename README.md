# 🃏 Game Xì Dách Việt Nam (Vietnamese Blackjack)

> **Dự án Game Xì Dách dân gian Việt Nam phát triển trên nền tảng C# Windows Forms (.NET Framework 4.7.2), tích hợp AI Bot thông minh và đồ họa GDI+ chuẩn casino.**

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple.svg)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/C%23-Latest-blue.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20WinForms-green.svg)](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
[![Tests](https://img.shields.io/badge/Tests-29%2F29%20Passed-brightgreen.svg)]()
[![Status](https://img.shields.io/badge/Status-Completed-success.svg)]()

---

## 📌 1. Giới thiệu tổng quan

**Game Xì Dách Việt Nam** tái hiện trọn vẹn nét văn hóa giải trí dân gian quen thuộc của bài Xì Dách (Blackjack kiểu Việt Nam) trong các dịp lễ tết. Trò chơi vận hành hoàn toàn offline, người chơi sẽ tranh tài cùng các đối thủ ảo (Bot AI) và Nhà cái hệ thống trên giao diện bàn nỉ xanh casino sang trọng.

### ✨ Các điểm nổi bật của dự án:
- **Chuẩn xác luật chơi Việt Nam**: Tích hợp đầy đủ các luật đặc thù từ [document-rule-game.md](file:///d:/xi-dach-vn/document-rule-game.md) (Át linh hoạt, Át chống gian lận Ngũ Linh, cùng Quắc Nhà cái thắng, cùng Ngũ Linh điểm thấp hơn thắng, Nhà cái đủ tuổi từ 15 điểm).
- **Trí tuệ nhân tạo (AI Bot)**:
  - **Bot người chơi**: Sở hữu các tính cách đa dạng (*Thận trọng*, *Cân bằng*, *Liều lĩnh*) với thuật toán tính cược và quyết định rút/dằn linh hoạt.
  - **AI Nhà cái (Dealer)**: Tận dụng **đặc quyền Xét bài sớm (Early Check)** để chốt lời các nhà con yếu trước khi quyết định rút thêm đối đầu nhà con mạnh.
- **Đồ họa lá bài GDI+ độc quyền**: Toàn bộ 52 lá bài mặt ngửa và mặt úp hoa văn casino hoàng gia được vẽ trực tiếp bằng `System.Drawing` (GDI+ chống răng cưa), hoàn toàn không cần asset hình ảnh bên ngoài.
- **Cơ chế Đổi Cái & Phá sản**: Nhà cái sở hữu quỹ bàn tối thiểu; nếu phá sản sẽ tự động chuyển quyền làm Cái cho người giàu nhất tiếp theo trên bàn.
- **Lưu trữ & Thống kê**: Tự động lưu hồ sơ người chơi (`savegame.json`), ghi nhận lịch sử thắng/hòa/thua, tỷ lệ thắng (Win Rate %) với thanh tiến trình trực quan.
- **Hệ thống phím tắt mượt mà**: Hỗ trợ chơi nhanh bằng bàn phím (`Space`, `Enter`, `H`, `S`).

---

## 📜 2. Tóm tắt luật chơi áp dụng

| Tình huống / Bộ bài | Quy tắc tính điểm & Trả thưởng |
|----------------------|---------------------------------|
| **Lá Át (Khi 2 - 3 lá)** | **Linh hoạt**: Tự động tối ưu giữa 1, 10, hoặc 11 điểm gần 21 nhất. |
| **Lá Át (Khi 4 - 5 lá)** | **Bắt buộc = 1 điểm** (chống gian lận tạo Ngũ Linh). |
| **Xì Bàng** (A + A) | Bộ bài mạnh nhất (Hạng 1) – Thắng **1 ăn 2** (2.0x). |
| **Xì Dách** (A + 10/J/Q/K) | Bộ bài mạnh nhì (Hạng 2) – Thắng **1 ăn 1.5** (1.5x). |
| **Ngũ Linh** (5 lá $\le 21$ điểm) | Hạng 3 – Thắng **1 ăn 2** (2.0x). *Nếu cùng Ngũ Linh, bên có **điểm thấp hơn sẽ thắng**.* |
| **Bài Thường** (16 - 21 điểm) | Hạng 4 – Thắng **1 ăn 1** (1.0x). So sánh điểm số, cao hơn thắng. |
| **Quắc** ($> 21$ điểm) | Vượt quá 21 điểm bị xử thua. |
| **Cùng Quắc** | **Đặc quyền Nhà cái: Nhà cái LUÔN THẮNG** khi cả hai cùng Quắc. |
| **Tuổi dằn bài Nhà con** | Phải đạt $\ge 16$ điểm mới được phép dằn (nếu dằn non sẽ bị xử thua). |
| **Tuổi dằn bài Nhà cái** | Đủ tuổi từ $\ge 15$ điểm. |
| **Xét bài sớm (Early Check)** | Khi đạt $\ge 15$ điểm, Nhà cái có quyền chọn xét bài từng nhà con bất kỳ trước khi rút tiếp. |
| **Quỹ tối thiểu & Đổi Cái** | Quỹ Nhà cái $\ge \text{Mức cược bàn} \times 20$. Nếu dưới mức này, Nhà cái phá sản và chuyển quyền làm Cái. |

---

## 🏗️ 3. Cấu trúc thư mục dự án

```
d:\xi-dach-vn\
├── .gitignore                                # Cấu hình loại trừ Git (.NET/VS/Savegame)
├── README.md                                 # Tài liệu hướng dẫn dự án
├── SRS.md                                    # Tài liệu đặc tả yêu cầu phần mềm
├── document-rule-game.md                     # Tài liệu luật chơi Xì Dách Việt Nam
├── document.md                               # Đề tài bài tập lớn Windows Forms
├── XiDachVN.sln                              # Solution file Visual Studio
├── XiDachVN/
│   ├── XiDachVN.csproj                       # Project file (.NET Framework 4.7.2 SDK-style)
│   ├── Program.cs                            # Điểm khởi chạy ứng dụng (MainForm hoặc --test)
│   │
│   ├── Models/                               # [LAYER THỰC THỂ & DỮ LIỆU]
│   │   ├── Enums.cs                          # AIBehavior, HandStatus, GameResult, GamePhase, CardSuit
│   │   ├── Card.cs                           # Model lá bài (Rank, Suit, điểm cơ sở, ký hiệu, màu sắc)
│   │   ├── CardDeck.cs                       # Bộ bài 52 lá, xáo bài Fisher-Yates
│   │   ├── Player.cs                         # Người chơi cơ sở (ví tiền, bài, mức cược, kết quả)
│   │   ├── BotPlayer.cs                      # Bot AI với 3 tính cách (Cautious, Normal, Aggressive)
│   │   └── CardRenderer.cs                   # Vẽ lá bài bằng GDI+ sắc nét, kèm bộ nhớ cache
│   │
│   ├── Engine/                               # [LAYER LOGIC GAME LOOP]
│   │   ├── ScoreCalculator.cs                # Tính điểm tối ưu (Át 1/10/11 hoặc Át = 1)
│   │   ├── HandComparer.cs                   # So bài, phân định thứ hạng và tỷ lệ trả thưởng
│   │   ├── DealerAI.cs                       # Trí tuệ Nhà cái (đủ 15đ, chiến thuật xét bài sớm)
│   │   └── GameEngine.cs                     # Điều phối Game Loop, sự kiện, đổi cái, thanh toán
│   │
│   ├── Data/                                 # [LAYER LƯU TRỮ & ÂM THANH]
│   │   ├── SaveData.cs                       # Model hồ sơ người chơi, thống kê và cài đặt
│   │   ├── DataManager.cs                    # Đọc/ghi JSON, tự phục hồi lỗi & ghi log error.log
│   │   └── SoundManager.cs                   # Quản lý phát âm thanh hiệu ứng tùy chọn (non-blocking)
│   │
│   ├── Forms/                                # [LAYER GIAO DIỆN WINFORMS]
│   │   ├── MainForm.cs                       # Menu chính (Chơi mới, Tiếp tục, Cài đặt, Luật chơi)
│   │   ├── GameForm.cs                       # Bàn chơi casino trực quan cho Người chơi + Nhà cái + 4 Bot
│   │   ├── SettingsForm.cs                   # Cài đặt số lượng Bot (1-4), cược tối thiểu, âm thanh
│   │   ├── StatsForm.cs                      # Bảng thành tích (Số ván chơi, thắng/hòa/thua, Win Rate %)
│   │   ├── HelpForm.cs                       # Bảng hướng dẫn luật chơi chi tiết, tỷ lệ trả thưởng
│   │   └── ResultDialog.cs                   # Hộp thoại tổng kết ván đấu chi tiết từng người chơi
│   │
│   └── Tests/                                # [LAYER KIỂM THỬ TỰ ĐỘNG]
│       └── LogicVerifier.cs                  # Suite 29 bài kiểm thử bao quát 100% logic trò chơi
```

---

## 🚀 4. Cài đặt và Chạy chương trình

### Yêu cầu môi trường
- Hệ điều hành: **Windows 10 / 11**
- Công cụ phát triển: **Visual Studio 2019 / 2022** (với workload *.NET Desktop Development*) hoặc **.NET SDK** (phiên bản bất kỳ hỗ trợ build net472).

### Cách 1: Khởi chạy bằng .NET CLI (Nhanh nhất)
Mở terminal (PowerShell hoặc Command Prompt) tại thư mục gốc:
```powershell
# Chạy trực tiếp game WinForms
dotnet run --project XiDachVN/XiDachVN.csproj
```

### Cách 2: Khởi chạy bằng Visual Studio
1. Mở file [XiDachVN.sln](file:///d:/xi-dach-vn/XiDachVN.sln) trong Visual Studio.
2. Đặt `XiDachVN` làm dự án khởi động (Set as Startup Project).
3. Nhấn **F5** (hoặc `Ctrl + F5`) để biên dịch và chạy.

---

## 🧪 5. Kiểm thử tự động (Automated Tests)

Dự án tích hợp bộ kiểm thử tự động toàn diện gồm **29 bài test** kiểm tra từng trường hợp luật chơi đặc thù, thuật toán tính điểm và vòng lặp trò chơi:

Để chạy bộ kiểm thử qua Terminal:
```powershell
dotnet run --project XiDachVN/XiDachVN.csproj -- --test
```

**Kết quả kiểm thử:**
```text
=== BẮT ĐẦU KIỂM THỬ TOÀN DIỆN HỆ THỐNG XÌ DÁCH ===
[PASS] Test 1: A + A là Xì Bàng
[PASS] Test 2: A + K là Xì Dách
[PASS] Test 2b: A + K điểm là 21
[PASS] Test 3: A + 9 + 8 = 18 (Át tính 1)
[PASS] Test 4: A + 9 = 20 (Át tính 11)
[PASS] Test 5: 4 lá (A+2+3+4) Át bắt buộc = 1 (tổng 10)
[PASS] Test 6: 5 lá A+2+3+4+5 = 15
[PASS] Test 6b: 5 lá <= 21 là Ngũ Linh
[PASS] Test 7: 10 + 7 + 6 = 23
[PASS] Test 7b: 23 điểm là Quắc
[PASS] Test 8: Cùng Quắc -> Nhà cái luôn thắng!
[PASS] Test 9: Cùng Ngũ Linh (15đ vs 18đ) -> Player điểm thấp hơn thắng x2!
[PASS] Test 10: Xì Dách thắng bài thường tỷ lệ 1.5x
[PASS] Test 11a: Nhà cái 15 điểm là đủ tuổi (Normal)
[PASS] Test 11b: Nhà con 15 điểm là Chưa Đủ Tuổi
[PASS] Test 12a: DataManager Lưu và Đọc chính xác
[PASS] Test 13a: CardRenderer tạo hình lá ngửa kích thước 74x104
[PASS] Test 13b: CardRenderer tạo hình lá úp kích thước 74x104
[PASS] Test 14a: Chia 2 lá ban đầu cho người chơi
[PASS] Test 14b: Chia 2 lá ban đầu cho nhà cái
[PASS] Test 14c: Người chơi hoàn thành lượt
[PASS] Test 14d: Tí (Thận trọng) hoàn thành lượt
[PASS] Test 14d: Tèo (Cân bằng) hoàn thành lượt
[PASS] Test 14d: Sửu (Liều lĩnh) hoàn thành lượt
[PASS] Test 14e: Người chơi đã được so bài và thanh toán
[PASS] Test 14f: Tí (Thận trọng) đã được so bài và thanh toán
[PASS] Test 14f: Tèo (Cân bằng) đã được so bài và thanh toán
[PASS] Test 14f: Sửu (Liều lĩnh) đã được so bài và thanh toán
[PASS] Test 15: Phát hiện nhà cái phá sản và chuyển quyền làm cái

=== KẾT QUẢ KIỂM THỬ: 29/29 BÀI TEST ĐẠT (100% SUCCESS) ===
```

---

## 🎮 6. Hướng dẫn thao tác trong Bàn chơi

- **Chọn mức cược**: Nhấp các nút chip cược nhanh (`+1K`, `+5K`, `+10K`, `+50K`, `TẤT TAY`) hoặc chọn `LÀM LẠI`.
- **Bắt đầu ván**: Nhấp **🎲 BẮT ĐẦU VÁN (CHIA BÀI)** hoặc nhấn phím `Space` / `Enter`.
- **Rút bài**: Nhấp **🃏 RÚT BÀI (HIT)** hoặc nhấn phím `H` / `Space`.
- **Dằn bài**: Nhấp **✋ DẰN BÀI (STAND)** hoặc nhấn phím `S` / `Enter` *(Chỉ kích hoạt khi bài $\ge 16$ điểm hoặc đã đủ 5 lá)*.
- **Thanh menu nhanh trên cùng**:
  - `📊 Thành Tích`: Xem số ván thắng, tỷ lệ % và số dư ví.
  - `⚙ Cài Đặt`: Thay đổi số lượng Bot tham gia (1 đến 4 bot) và mức cược mặc định.
  - `❓ Luật Chơi`: Tra cứu cẩm nang luật chơi chi tiết.
  - `🚪 Về Menu`: Thoát ra sảnh chính (tự động lưu trạng thái).

---

## 📄 7. Bản quyền & Giấy phép

Dự án được xây dựng cho mục đích học tập và nghiên cứu lập trình ứng dụng Windows Forms trên nền tảng .NET Framework.
Phát hành theo giấy phép [MIT License (Modified - Lawful Use)](file:///d:/xi-dach-vn/LICENSE). Bạn có quyền tự do sử dụng cho mọi mục đích chính đáng, giữ nguyên thông tin bản quyền và **tuyệt đối không được sử dụng cho các mục đích trái pháp luật**.
