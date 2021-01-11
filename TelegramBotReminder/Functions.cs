using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotReminder
{
    static class Functions
    {
        public static List<long> chatsIds { get; set; }
        public static void LogEvent(string LcTexto)
        {
            Console.WriteLine($"{System.DateTime.UtcNow} - {LcTexto}");
        }
    }
}
