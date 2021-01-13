using Microsoft.Extensions.Configuration;
using System;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TelegramBotReminder
{
    class Program
    {
        public static TelegramBotFunctions bot;
        private static Timer _timer;
        private static DateTime lastMessageTime;
        static void Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.BaseDirectory}appsettings.json", true, true).Build();
            var token = configuration["TelegramBotToken"];
            bot = new TelegramBotFunctions(token);
            SetTimer();
            Console.ReadLine();
            Functions.LogEvent("Finalizando aplicação");
            bot.stopReceiving();
        }
        private static void SetTimer()
        {
            _timer = new System.Timers.Timer(60000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            bot.sendMessagesIfNeeded();
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (lastMessageTime <= DateTime.Now)
            {
                bot.sendMessagesIfNeeded();
                lastMessageTime = DateTime.Now;
            }

        }


    }
}
