using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TelegramBotReminder.Models
{
    public class Mensagem
    {
        [Key]
        public int MensagemId { get; set; }
        public string TextMessage { get; set; }
        public DateTime MessageDate { get; set; }
        public bool MessageSent { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
    }
}
