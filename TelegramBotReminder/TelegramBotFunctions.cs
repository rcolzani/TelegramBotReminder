using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace TelegramBotReminder
{
    public class TelegramBotFunctions
    {
        private string _token;
        private static TelegramBotClient bot;
        public TelegramBotFunctions(string token)
        {
            _token = token;
            bot = new TelegramBotClient(token);
            bot.OnMessage += botMessageReceiver;
            bot.StartReceiving();
        }
        private static void botMessageReceiver(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                PrepareQuestionnaires(e);
        }
        public void stopReceiving()
        {
            bot.StopReceiving();
        }
        public static void PrepareQuestionnaires(MessageEventArgs e)
        {
            long chatId = e.Message.Chat.Id;
            Functions.LogEvent($"Mensagem recebida {e.Message.Text} - do chat {chatId}");

            if (e.Message.Text.ToLower() == "Olá" || e.Message.Text.ToLower() == "/start")
                bot.SendTextMessageAsync(e.Message.Chat.Id, $"Olá {e.Message.Chat.Username}!" + Environment.NewLine + "Digite 'iniciar' para começar a receber lembretes e 'parar' para não receber mais");
            if (e.Message.Text.ToLower() == "iniciar")
            {
                var idExist = Functions.chatsIds.FirstOrDefault(c => c == chatId);

                if (idExist == 0)
                {
                    Functions.chatsIds.Add(chatId);
                }
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Adicionado a fila de envio.");
            }
            if (e.Message.Text.ToLower() == "parar")
            {
                Functions.chatsIds.Remove(chatId);
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Removido da fila de envio.");
            }

        }
        public bool sendMessage(string text)
        {
            HttpClient client = new HttpClient();
            foreach (var chatId in Functions.chatsIds)
            {
                var responseString = client.GetStringAsync($"https://api.telegram.org/bot{_token}/sendMessage?chat_id={chatId}&text={text}");
                Functions.LogEvent(responseString.Result.ToString());
            }

            return true;
        }
    }
}
