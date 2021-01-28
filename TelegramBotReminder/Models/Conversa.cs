using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TelegramBotReminder.Models;

namespace TelegramBotReminder.Model
{
    public class Cliente
    {
        [Key]
        public long ClientId { get; set; }
        public string TelegramChatId { get; set; }
        public string TextMessage { get; set; }
        public int Status { get; set; }
        // public List<Message> MessageHistory { get; set; }
        public TimeSpan RemindTimeToSend { get; set; }
        public DateTime LastSend { get; set; }
        public bool Activated { get; set; }
        public bool RiverLevel { get; set; }
        public DateTime StartChat { get; set; }


    }

}
