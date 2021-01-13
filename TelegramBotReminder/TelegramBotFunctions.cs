using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotReminder
{
    public class TelegramBotFunctions
    {
        public static List<chatClient> chatClients = new List<chatClient>();
        private string _token;
        private static TelegramBotClient bot;
        private static string chatHistoryFullPath;
        public TelegramBotFunctions(string token)
        {
            _token = token;
            bot = new TelegramBotClient(_token);
            chatHistoryFullPath = $@"{AppDomain.CurrentDomain.BaseDirectory}chatHistory.json";
            string jsonContent = Functions.ReadFromFile(chatHistoryFullPath);
            if (jsonContent != "")
            {
                chatClients = JsonSerializer.Deserialize<List<chatClient>>(jsonContent); ;
            }
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
            var jsonContent = JsonSerializer.Serialize(chatClients);
            Functions.SaveToFile(chatHistoryFullPath, jsonContent);
        }
        public async static void PrepareQuestionnaires(MessageEventArgs e)
        {
            long chatId = e.Message.Chat.Id;
            Functions.LogEvent($"Mensagem recebida {e.Message.Text} - do chat {chatId}");
            addClientToList(chatId);
            var clientChat = chatClients.FirstOrDefault(c => c.ChatId == chatId);

            string mensagem = Functions.RemoveAccents(e.Message.Text.ToLower());
            

            if (mensagem == "ola" || mensagem == "/start")
            {

                var keyboard = new ReplyKeyboardMarkup
                {
                    Keyboard = new[]
                    {
                        new[]
                        {
                            new KeyboardButton("Iniciar")
                        },
                        new[]
                        {
                            new KeyboardButton("Consultar")
                        },
                        new[]
                        {
                            new KeyboardButton("Parar")
                        },
                        new[]
                        {
                            new KeyboardButton("Sair")
                        }
                    }
                };
                
                string texto = $"Olá {e.Message.From.FirstName}, {Environment.NewLine}{Environment.NewLine}Selecione uma das opções no teclado que apareceu para você ou digite:{Environment.NewLine}" +
                    $"*Iniciar* - para começar a cadastrar um lembrete{Environment.NewLine}" +
                    $"*Consultar* - para consultar os lembretes ativos{Environment.NewLine}" +
                    $"*Parar* - para não receber mais{Environment.NewLine}" +
                    "*Sair* - para sair do menu";

                await sendMessage(e.Message.Chat.Id, texto, keyboard);
                clientChat.Status = clientStatus.newCliente;
            }
            else if (mensagem == "iniciar")
            {
                clientChat.Activated = true;
                clientChat.Status = clientStatus.waitingForTextMessage;
                await sendMessage(e.Message.Chat.Id, "Qual mensagem você deseja receber ao ser lembrado?");
            }
            else if (mensagem == "consultar")
            {
                string lembretes = "";

                foreach (var chat in chatClients)
                {
                    if (chat.ChatId == chatId)
                    {
                        if (lembretes != "")
                            lembretes += Environment.NewLine;
                        lembretes += $"Lembrete {chat.TextMessage} às {chat.TimeToSend}";
                    }
                }
                await sendMessage(e.Message.Chat.Id, lembretes);
            }
            else if (mensagem == "parar")
            {
                clientChat.Activated = false;
                await sendMessage(e.Message.Chat.Id, "Removido da fila de envio.");
                chatClients.Remove(chatClients.FirstOrDefault(c => c.ChatId == chatId));
            }
            else if (mensagem == "sair")
            {
                await sendMessage(e.Message.Chat.Id, $"Feito :D{Environment.NewLine}{Environment.NewLine}Para voltar a conversar comigo diga Olá");
                chatClients.Remove(chatClients.FirstOrDefault(c => c.ChatId == chatId));
            }
            else if (clientChat.Status == clientStatus.waitingForTextMessage)
            {
                if (mensagem != null)
                {
                    clientChat.TextMessage = e.Message.Text;
                    clientChat.Status = clientStatus.waitingForTime;
                    await sendMessage(e.Message.Chat.Id, "Qual horário você deseja ser lembrado? Precisa ser no formato HH:MM!");
                }
            }
            else if (clientChat.Status == clientStatus.waitingForTime)
            {
                TimeSpan sendTime = new TimeSpan();
                if (TimeSpan.TryParse(mensagem, out sendTime))
                {
                    clientChat.TimeToSend = sendTime;
                    clientChat.Status = clientStatus.complete;
                    await sendMessage(e.Message.Chat.Id, $"Cadastro criado com sucesso!!!{Environment.NewLine}{Environment.NewLine}Você receberá a mensagem: {clientChat.TextMessage}{Environment.NewLine}Todos os dias as {clientChat.TimeToSend.ToString()}");
                }
                else
                {
                    await sendMessage(e.Message.Chat.Id, "Não reconheço este formato de horário. O horário precisa estar no formato HH:MM");
                }
            }
            else
            {
                await sendMessage(e.Message.Chat.Id, $"Não consegui entender este comando :/ Os comandos disponíveis são:{Environment.NewLine}olá - para iniciar a conversa{Environment.NewLine}iniciar - para iniciar o cadastro de um lembrete{Environment.NewLine}parar - para parar o recebimento de lembretes");
            }
            clientChat.MessageHistory.Add(new Message { dateTimeMessage = e.Message.Date, MessageText = e.Message.Text, MessageId = e.Message.MessageId });
        }
        /// <summary>
        /// Add client do List of clients
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns>It's a new client</returns>
        private static bool addClientToList(long chatId)
        {
            var idExist = chatClients.FirstOrDefault(c => c.ChatId == chatId);

            if (idExist == null)
            {
                chatClients.Add(new chatClient { ChatId = chatId, TextMessage = "", TimeToSend = new TimeSpan(08, 00, 00) });
                return true;
            }

            return false;
        }
        public async static Task<bool> sendMessage(long chatId, string text, IReplyMarkup replyMarkup = null)
        {
            try
            {
                if (text == null)
                {
                    return false;
                }
                if (replyMarkup == null)
                {
                    replyMarkup = new ReplyKeyboardRemove() { };
                }
                // var responseString = client.GetStringAsync($"https://api.telegram.org/bot{_token}/sendMessage?chat_id={chatClient.ChatId}&text={chatClient.TextMessage}");
                //Functions.LogEvent(responseString.Result.ToString());
                var messageSent = await bot.SendTextMessageAsync(chatId, text,Telegram.Bot.Types.Enums.ParseMode.Markdown, false,false, 0, replyMarkup);

                if (chatClients.FirstOrDefault(c => c.ChatId == chatId).MessageHistory == null)
                {
                    chatClients.FirstOrDefault(c => c.ChatId == chatId).MessageHistory = new List<Message>();
                }
                chatClients.FirstOrDefault(c => c.ChatId == chatId).MessageHistory.Add(new Message { dateTimeMessage = DateTime.Now, MessageText = text, MessageId = messageSent.MessageId });
                return true;
            }
            catch (Exception e)
            {
                Functions.LogException(e);
                return false;
            }
        }
        public async void sendMessagesIfNeeded()
        {
            try
            {
                foreach (var client in chatClients)
                {
                    if (client.TextMessage != "" && client.Status == clientStatus.complete) //apenas clientes com o cadastro de um lembrete completo
                    {
                        if ((DateTime.Now.Date + client.TimeToSend) <= DateTime.Now && client.LastSend.Date < DateTime.Now.Date) //considerar enviar 
                        {
                            if (client.LastSend == new DateTime())
                            {
                                await sendMessage(client.ChatId, client.TextMessage);
                            }
                            else if (client.LastSend.AddMinutes(-5) < DateTime.Now)
                            {
                                await sendMessage(client.ChatId, client.TextMessage);
                            }
                            client.LastSend = DateTime.Now;
                        }
                    }

                }
                //if (text == null)
                //{
                //    return false;
                //}
                //// var responseString = client.GetStringAsync($"https://api.telegram.org/bot{_token}/sendMessage?chat_id={chatClient.ChatId}&text={chatClient.TextMessage}");
                ////Functions.LogEvent(responseString.Result.ToString());
                //bot.SendTextMessageAsync(chatId, text);
                //chatClients.FirstOrDefault(c => c.ChatId == chatId).MessageHistory.Add(new Message { dateTimeMessage = DateTime.UtcNow, MessageText = text });
                //return true;
            }
            catch (Exception e)
            {
                Functions.LogException(e);
            }
        }

        public class chatClient
        {
            public long ChatId { get; set; }
            public string TextMessage { get; set; }
            public clientStatus Status { get; set; }
            public List<Message> MessageHistory { get; set; }
            public TimeSpan TimeToSend { get; set; }
            public DateTime LastSend { get; set; }
            public bool Activated { get; set; }
        }
        public class Message
        {
            public string MessageText { get; set; }
            public int MessageId { get; set; }
            public DateTime dateTimeMessage { get; set; }
        }
        public enum clientStatus
        {
            newCliente = 0,
            waitingForTextMessage = 1,
            waitingForTime = 2,
            complete = 3
        }
    }
}
