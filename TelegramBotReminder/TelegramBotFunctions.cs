using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotReminder.Data;
using TelegramBotReminder.Model;

namespace TelegramBotReminder
{
    public class TelegramBotFunctions
    {
        private static BotContext _context = new BotContext();
        public static List<chatClient> xchatClients = new List<chatClient>();
        private string _token;
        private static TelegramBotClient bot;
        private static string lastRiverLevel;
        public TelegramBotFunctions(string token)
        {
            _token = token;
            bot = new TelegramBotClient(_token);
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
        public async static void PrepareQuestionnaires(MessageEventArgs e)
        {
            long chatId = e.Message.Chat.Id;

            string jsonString = JsonSerializer.Serialize(e);
            Functions.LogEvent($"MessageEvent: {jsonString}");
            Functions.LogEvent($"Mensagem recebida {e.Message.Text} - do chat {chatId}");
            bool isNewCliente = _context.addCliente(new Cliente { ClientId = chatId, TextMessage = "", RemindTimeToSend = new TimeSpan(08, 00, 00) }); ;
            var clientChat = _context.Conversas.FirstOrDefault(c => c.ClientId == chatId);
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
                        new []{
                            new KeyboardButton("Nível do rio")
                        },
                        new[]
                        {
                            new KeyboardButton("Sair")
                        }
                    }
                };

                string texto = $"Olá {e.Message.From.FirstName}, {Environment.NewLine}{Environment.NewLine}";

                if (isNewCliente)
                {
                    texto += $"Vejo que é sua primeira vez aqui. Seja muito bem vindo!!!{Environment.NewLine}Sou um robô criado para lembrar você do que for preciso. Basta você criar um lembrete que eu" +
                        $" aviso você para tomar água, se medicar, tirar o lixo... Você só precisar seguir as instruções abaixo e não esquecerei de você hehe{Environment.NewLine}{Environment.NewLine}";
                }

                texto += $"Selecione uma das opções no teclado que apareceu para você ou digite:{Environment.NewLine}" +
                    $"*Iniciar* - para começar a cadastrar um lembrete{Environment.NewLine}" +
                    $"*Consultar* - para consultar os lembretes ativos{Environment.NewLine}" +
                    $"*Parar* - para não receber mais{Environment.NewLine}" +
                    $"*Nível do rio* - para saber em tempo real quando uma nova medição foi atualizada no site da defesa civil de Blumenau{Environment.NewLine}" +
                    "*Sair* - para sair do menu";

                await sendMessage(e.Message.Chat.Id, texto, keyboard);
            }
            else if (mensagem == "iniciar")
            {
                clientChat.Activated = true;
                clientChat.Status = (int)clientStatus.waitingForTextMessage;
                await sendMessage(e.Message.Chat.Id, "Qual mensagem você deseja receber ao ser lembrado?");
            }
            else if (mensagem == "nivel do rio")
            {
                clientChat.RiverLevel = true;
                await sendMessage(e.Message.Chat.Id, "Feito!\n\nVocê começará a receber as medições do nível do rio a partir de agora.");
            }
            else if (mensagem == "consultar")
            {
                string lembretes = "";

                foreach (var chat in _context.GetClientes())
                {
                    if (chat.ClientId == chatId && chat.TextMessage != "")
                    {
                        if (lembretes != "") { }
                        lembretes += Environment.NewLine;
                        lembretes += $"Lembrete {chat.TextMessage} às {chat.RemindTimeToSend}";
                    }
                }

                if (lembretes == "")
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
                            new KeyboardButton("Sair")
                        }
                    }
                    };
                    lembretes = "Você ainda não tem lembretes cadastrados. Que tal iniciar o cadastro de um novo lembrete?";
                    await sendMessage(e.Message.Chat.Id, lembretes, keyboard);
                }
                else
                {
                    await sendMessage(e.Message.Chat.Id, lembretes);
                }
            }
            else if (mensagem == "parar")
            {
                clientChat.Activated = false;
                clientChat.RiverLevel = false;
                clientChat.TextMessage = "";
                clientChat.Status = (int)clientStatus.newCliente;
                clientChat.LastSend = new DateTime();
                await sendMessage(e.Message.Chat.Id, "Removido da fila de envio.");
            }
            else if (mensagem == "sair")
            {
                await sendMessage(e.Message.Chat.Id, $"Feito :D{Environment.NewLine}{Environment.NewLine}Para voltar a conversar comigo diga Olá");
                _context.Remove(_context.Conversas.FirstOrDefault(c => c.ClientId == chatId));
            }
            else if (clientChat.Status == (int)clientStatus.waitingForTextMessage)
            {
                if (mensagem != null)
                {
                    clientChat.TextMessage = e.Message.Text;
                    clientChat.Status = (int)clientStatus.waitingForTime;
                    await sendMessage(e.Message.Chat.Id, "Qual horário você deseja ser lembrado? Precisa ser no formato HH:MM!");
                }
            }
            else if (clientChat.Status == (int)clientStatus.waitingForTime)
            {
                TimeSpan sendTime = new TimeSpan();
                if (TimeSpan.TryParse(mensagem, out sendTime))
                {
                    clientChat.RemindTimeToSend = sendTime;
                    clientChat.Status = (int)clientStatus.complete;

                    if (DateTime.Now.Date + sendTime < DateTime.Now)
                    {
                        clientChat.LastSend = DateTime.Now; //Se o horário do lembrete é um horário que hoje já passou, registra como já enviado o lembrete. Se isso não for feito, será reenviado no próximo ciclo
                    }

                    await sendMessage(e.Message.Chat.Id, $"Cadastro criado com sucesso!!!{Environment.NewLine}{Environment.NewLine}Você receberá a mensagem: {clientChat.TextMessage}{Environment.NewLine}Todos os dias as {clientChat.RemindTimeToSend.ToString()}");
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
            //clientChat.MessageHistory.Add(new Message { dateTimeMessage = e.Message.Date, MessageText = e.Message.Text, MessageId = e.Message.MessageId });
            _context.Update(clientChat);
            _context.SaveChanges();
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
                var messageSent = await bot.SendTextMessageAsync(chatId, text, Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, 0, replyMarkup);
                string jsonString = JsonSerializer.Serialize(messageSent);
                Functions.LogEvent($"messageSent: {jsonString}");
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
                string siteContent = string.Empty;
                string url = "http://alertablu.cob.sc.gov.br/d/nivel-do-rio";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())  // Go query google
                using (Stream responseStream = response.GetResponseStream())               // Load the response stream
                using (StreamReader streamReader = new StreamReader(responseStream))       // Load the stream reader to read the response
                {
                    siteContent = streamReader.ReadToEnd(); // Read the entire response and store it in the siteContent variable
                }
                var document = new HtmlDocument();
                document.LoadHtml(siteContent);
                var nodes = document.DocumentNode.SelectNodes("//*[@id='river-level-table']/tbody/tr/td");
                var hora = nodes[0].InnerText.Trim();
                var nivelRio = nodes[1].InnerText.Trim();
                bool enviarNivel = false;

                if (String.IsNullOrEmpty(lastRiverLevel) == false && lastRiverLevel != hora + nivelRio)
                {
                    enviarNivel = true;
                }
                lastRiverLevel = hora + nivelRio;

                foreach (var client in _context.GetClientes())
                {
                    //enviar nível do rio
                    if (enviarNivel && client.RiverLevel)
                    {
                        await sendMessage(client.ClientId, $"O nível do rio está {nivelRio} às {hora}");
                    }
                    //enviar lembretes criados
                    if (client.TextMessage != "" && client.Status == (int)clientStatus.complete) //apenas clientes com o cadastro de um lembrete completo
                    {
                        if ((DateTime.Now.Date + client.RemindTimeToSend) <= DateTime.Now && client.LastSend.Date < DateTime.Now.Date) //considerar enviar 
                        {
                            if (client.LastSend == new DateTime())
                            {
                                await sendMessage(client.ClientId, client.TextMessage);
                            }
                            else if (client.LastSend.AddMinutes(-5) < DateTime.Now)
                            {
                                await sendMessage(client.ClientId, client.TextMessage);
                            }
                            client.LastSend = DateTime.Now;
                            _context.Update(client);
                            _context.SaveChanges();
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
            public bool RiverLevel { get; set; }
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
