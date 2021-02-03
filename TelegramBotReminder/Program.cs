using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Timers;
using Telegram.Bot;
using Telegram.Bot.Args;
using TelegramBotReminder.Data;
using TelegramBotReminder.Models;

namespace TelegramBotReminder
{
    class Program
    {
        public static TelegramBotFunctions bot;
        private static Timer _timer;
        private static DateTime lastMessageTime;
        static void Main(string[] args)
        {
            Functions.LogEvent("Aplicação iniciada");
            var token = Functions.ConfigurationRead("TelegramBotToken");
            bot = new TelegramBotFunctions(token);

            // var botContext = new BotContext();
            // Cliente cliente = botContext.getClient(1114855651);
            // cliente.TextMessage = "Funcionou o teste";
            // botContext.updateClient(cliente);

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
