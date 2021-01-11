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

            if (e.Message.Text.ToLower() == "hi")
                bot.SendTextMessageAsync(e.Message.Chat.Id, "hello dude" + Environment.NewLine + "welcome to csharp corner chat bot." + Environment.NewLine + "How may i help you ?");
            if (e.Message.Text.ToLower().Contains("know about"))
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Yes sure..!!" + Environment.NewLine + "Mahesh Chand is the founder of C# Corner.Please go through for more detail." + Environment.NewLine + "https://www.c-sharpcorner.com/about");
            if (e.Message.Text.ToLower().Contains("csharpcorner logo?"))
            {
                bot.SendStickerAsync(e.Message.Chat.Id, "https://csharpcorner-mindcrackerinc.netdna-ssl.com/App_Themes/CSharp/Images/SiteLogo.png");
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Anything else?");
            }
            if (e.Message.Text.ToLower().Contains("list of featured"))
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Give me your profile link ?");
            if (e.Message.Text.ToLower().Contains("here it is"))
                bot.SendTextMessageAsync(e.Message.Chat.Id, Environment.NewLine + "https://www.c-sharpcorner.com/article/getting-started-with-ionic-framework-angular-and-net-core-3/" + Environment.NewLine + Environment.NewLine +
                    "https://www.c-sharpcorner.com/article/getting-started-with-ember-js-and-net-core-3/" + Environment.NewLine + Environment.NewLine +
                    "https://www.c-sharpcorner.com/article/getting-started-with-vue-js-and-net-core-32/");
            if (e.Message.Text.ToLower() == "iniciar")
            {
                var idExist = Functions.chatsIds.FirstOrDefault(c => c == chatId);

                if (idExist ==0 )
                {
                    Functions.chatsIds.Add(chatId);
                }
                bot.SendTextMessageAsync(e.Message.Chat.Id, "Adicionado a fila de envio.");
            }
            if (e.Message.Text.ToLower() =="parar")
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
