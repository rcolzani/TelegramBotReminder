using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TelegramBotReminder.Models;

namespace TelegramBotReminder.Models
{
    public class Cliente
    {
        [Key]
        public int ClientId { get; set; }
        public long TelegramChatId { get; set; }
        public string TextMessage { get; set; }
        public int Status { get; set; }
        public IEnumerable<Mensagem> MessageHistory { get; set; }
        public TimeSpan RemindTimeToSend { get; set; }
        public DateTime LastSend { get; set; }
        public bool Activated { get; set; }
        public bool RiverLevel { get; set; }
        public DateTime StartChat { get; set; }


    }

}
