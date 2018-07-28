namespace FallBack
{
    using System;
    using System.IO;

    public static class Logger
    {
        private const string LogFileName = "log.txt";

        public static void Log(string text)
        {
            Console.WriteLine(text);

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ss HH:mm:ss");
            File.AppendAllText(LogFileName, $"[{timestamp}] {text}\r\n");
        }

        public static void LogError(string text)
        {
            ConsoleColor origColour = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;

            Logger.Log($"[X] {text}");

            Console.ForegroundColor = origColour;
        }

        public static void LogException(string text, Exception ex)
        {
            if (ex == null)
            {
                Logger.LogError(text);
                return;
            }

            string innerEx = ex.InnerException?.Message;

            if (string.IsNullOrEmpty(innerEx))
            {
                Logger.LogError($"{text}\n{ex.Message}");
                return;
            }

            Logger.LogError($"{text}\n{ex.Message}\n{innerEx}");
        }

        public static void BlankLine() => Logger.Log(string.Empty);
    }
}
