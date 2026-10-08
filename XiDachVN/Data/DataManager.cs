using System;
using System.IO;
using Newtonsoft.Json;

namespace XiDachVN.Data
{
    public static class DataManager
    {
        private static readonly string SaveFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.json");

        private static readonly string LogFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

        /// <summary>
        /// Đọc dữ liệu savegame.json. Nếu file chưa có hoặc lỗi, trả về mặc định.
        /// </summary>
        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                {
                    var defaultData = new SaveData();
                    Save(defaultData);
                    return defaultData;
                }

                string json = File.ReadAllText(SaveFilePath);
                var data = JsonConvert.DeserializeObject<SaveData>(json);
                if (data == null)
                {
                    var fallback = new SaveData();
                    Save(fallback);
                    return fallback;
                }
                return data;
            }
            catch (Exception ex)
            {
                LogError("Lỗi khi đọc file savegame.json", ex);
                var fallback = new SaveData();
                Save(fallback);
                return fallback;
            }
        }

        /// <summary>
        /// Ghi dữ liệu vào file savegame.json
        /// </summary>
        public static bool Save(SaveData data)
        {
            try
            {
                if (data == null) return false;
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(SaveFilePath, json);
                return true;
            }
            catch (Exception ex)
            {
                LogError("Lỗi khi ghi file savegame.json", ex);
                return false;
            }
        }

        /// <summary>
        /// Ghi log lỗi vào error.log
        /// </summary>
        public static void LogError(string context, Exception ex)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}: {ex.Message}\nStack: {ex.StackTrace}\n\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Tuyệt đối không crash khi ghi log thất bại
            }
        }
    }
}
