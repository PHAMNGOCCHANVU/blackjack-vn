using System;
using System.IO;
using System.Media;

namespace XiDachVN.Data
{
    public static class SoundManager
    {
        public static bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Phát âm thanh hiệu ứng từ thư mục Resources/Sounds/{soundName}.wav
        /// Nếu không có file hoặc lỗi, không làm gián đoạn game (đáp ứng NFR-06)
        /// </summary>
        public static void Play(string soundName)
        {
            if (!IsEnabled || string.IsNullOrWhiteSpace(soundName))
                return;

            try
            {
                string soundsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sounds");
                string soundPath = Path.Combine(soundsDir, soundName + ".wav");

                if (File.Exists(soundPath))
                {
                    using (var player = new SoundPlayer(soundPath))
                    {
                        player.Play();
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi âm thanh, không làm văng ứng dụng
            }
        }
    }
}
