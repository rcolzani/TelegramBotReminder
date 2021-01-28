using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotReminder.Model;

namespace TelegramBotReminder.Models
{
    public class Mensagem
    {
        public int MensagemId { get; set; }
        public string TextMessage { get; set; }
        public DateTime MessageDate { get; set; }

        //public int ConversaId { get; set; }
        //public Conversa Conversa { get; set; }
    }
}
