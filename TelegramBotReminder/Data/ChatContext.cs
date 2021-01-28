using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotReminder.Model;
using TelegramBotReminder.Models;

namespace TelegramBotReminder.Data
{
    public class BotContext : DbContext
    {
        public DbSet<Cliente> Conversas { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .HasKey(c => new { c.ClientId });
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseMySql($@"Data Source={Functions.ConfigurationRead("MySqlConnection")}");
            options.UseMySql(@"server=192.168.99.100;port=3306;database=TelegramBot;user=root;password=123456");
            base.OnConfiguring(options);
        }

        public bool addCliente(Cliente conversa)
        {
            try
            {
                conversa.StartChat = DateTime.UtcNow;
                using (var db = new BotContext())
                {
                    var cliente = db.Conversas.FirstOrDefault(c => c.TelegramChatId == conversa.TelegramChatId);
                    if (cliente != null)
                    {
                        return false;
                    }
                    db.Conversas.Add(conversa);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                Functions.LogException(e);
                return false;
            }
        }
        public List<Cliente> GetClientes()
        {
            using (var db = new BotContext())
            {
                return db.Conversas.AsNoTracking().ToList();
            }
        }

    }
}
