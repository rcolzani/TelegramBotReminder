using System;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TelegramBotReminder
{
    class Program
    {
        public static TelegramBotFunctions bot = new TelegramBotFunctions("youapikey");
        private static Timer _timer;
        private static DateTime lastMessageTime;
        static void Main(string[] args)
        {
            Functions.chatsIds = new System.Collections.Generic.List<long>();
            SetTimer();
            Console.ReadLine();
            Functions.LogEvent("Finalizando aplicação");
        }
        private static void SetTimer()
        {
            _timer = new System.Timers.Timer(10000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (lastMessageTime <= DateTime.UtcNow.AddHours(-2))
            {
                bot.sendMessage("Beba água");
                lastMessageTime = DateTime.UtcNow;
            }

        }
    
      
    }
}
