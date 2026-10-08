namespace XiDachVN.Models
{
    public enum AIBehavior
    {
        Cautious,    // Thận trọng: >= 16 dằn luôn
        Normal,      // Cân bằng: 16 rút, >= 17 dằn
        Aggressive   // Liều lĩnh: 16-17 rút, >= 18 dằn
    }

    public enum HandStatus
    {
        ChuaDuTuoi,  // < 16 điểm (nhà con) hoặc < 15 điểm (nhà cái)
        Normal,      // Bài thường đủ điểm (16-21)
        XiBang,      // 2 lá Át (A + A)
        XiDach,      // 1 Át + 1 lá 10/J/Q/K
        NguLinh,     // 5 lá có tổng điểm <= 21
        Quac         // Tổng điểm > 21
    }

    public enum GameResult
    {
        Win,
        Lose,
        Draw
    }

    public enum GamePhase
    {
        Waiting,         // Chờ bắt đầu ván mới
        Betting,         // Đặt cược
        Dealing,         // Chia 2 lá đầu
        CheckSpecial,    // Xét bài đặc biệt 2 lá đầu (Xì Bàng, Xì Dách)
        PlayerTurn,      // Lượt người chơi rút/dằn
        BotTurns,        // Lượt các Bot rút/dằn
        DealerTurn,      // Lượt Nhà cái rút bài và xét bài sớm
        Comparing,       // So bài tất cả
        RoundEnd         // Tổng kết ván, cập nhật tiền, kiểm tra phá sản/đổi cái
    }

    public enum CardSuit
    {
        Hearts,      // Cơ (♥)
        Diamonds,    // Rô (♦)
        Clubs,       // Chuồn/Tép (♣)
        Spades       // Bích (♠)
    }
}
