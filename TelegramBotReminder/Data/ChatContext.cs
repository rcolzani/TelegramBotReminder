using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotReminder.Models;

namespace TelegramBotReminder.Data
{
    public class BotContext : DbContext
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Mensagem> Mensagens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .HasKey(c => new { c.ClientId });
            modelBuilder.Entity<Cliente>()
                .Property(p => p.ClientId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Mensagem>()
                .HasKey(m => new { m.MensagemId });
            modelBuilder.Entity<Mensagem>()
                .Property(p => p.MensagemId)
                .ValueGeneratedOnAdd();

        }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //options.UseMySql($@"Data Source={Functions.ConfigurationRead("MySqlConnection")}");
            options.UseMySql(Functions.GetConnectionStringFromSettings());
            base.OnConfiguring(options);
        }
        public int GetClienteId(long telegramClientId)
        {
            var cliente = Clientes.FirstOrDefault(cli => cli.TelegramChatId == telegramClientId);
            return cliente.ClientId;
        }
        public bool addCliente(Cliente conversa)
        {
            try
            {
                conversa.StartChat = DateTime.UtcNow;
                using (var db = new BotContext())
                {
                    var cliente = db.Clientes.FirstOrDefault(c => c.TelegramChatId == conversa.TelegramChatId);
                    if (cliente != null)
                    {
                        return false;
                    }
                    db.Clientes.Add(conversa);
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
    }
}
