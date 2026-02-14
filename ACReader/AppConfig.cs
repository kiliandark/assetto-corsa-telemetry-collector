using System;
using System.IO;
using Newtonsoft.Json;

namespace ACReader
{
    public class AppConfig
    {
        public string logstashHost { get; set; } = "81.22.47.106";
        public int logstashPort { get; set; } = 10051;

        public static AppConfig Load(string filePath = "config.json")
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[CONFIG] Файл конфигурации '{filePath}' не найден, создаю с настройками по умолчанию.");
                    Console.ResetColor();

                    var defaultConfig = new AppConfig();
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
                    return defaultConfig;
                }

                var json = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<AppConfig>(json);

                if (config == null)
                    throw new Exception("Ошибка чтения конфигурации (config == null)");

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[CONFIG] Конфигурация загружена. Smart Monitor Data Collector host: {config.logstashHost}, Port: {config.logstashPort}");
                Console.ResetColor();

                return config;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[CONFIG ERROR] {ex.Message}");
                Console.ResetColor();

                return new AppConfig(); // fallback на дефолт
            }
        }
    }
}
