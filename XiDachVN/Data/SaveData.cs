using System;

namespace XiDachVN.Data
{
    public class GameSettings
    {
        public int BotCount { get; set; } = 3;
        public int DefaultBet { get; set; } = 1000;
        public bool SoundEnabled { get; set; } = true;
    }

    public class SaveData
    {
        public string PlayerName { get; set; } = "Người chơi";
        public int Money { get; set; } = 50000; // Khởi đầu 50.000 VNĐ
        public int GamesPlayed { get; set; } = 0;
        public int GamesWon { get; set; } = 0;
        public int GamesLost { get; set; } = 0;
        public int GamesDrawn { get; set; } = 0;

        public double WinRate => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed * 100.0 : 0.0;

        public GameSettings Settings { get; set; } = new GameSettings();

        public void RecordGame(bool isWin, bool isDraw)
        {
            GamesPlayed++;
            if (isDraw)
                GamesDrawn++;
            else if (isWin)
                GamesWon++;
            else
                GamesLost++;
        }
    }
}
