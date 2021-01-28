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
            options.UseMySql(Functions.GetConnectionStringFromSettings());
            base.OnConfiguring(options);
        }
        public Cliente getClient(int clientId)
        {
            using (var db = new BotContext())
            {
                var clienteExist = db.Conversas.AsNoTracking().FirstOrDefault(cli => cli.ClientId == clientId);
                return clienteExist;
            }
        }
        public bool updateClient(Cliente cliente)
        {
            using (var db = new BotContext())
            {
                var clienteExist = db.Conversas.FirstOrDefault(cli => cli.ClientId == cliente.ClientId);
                if (clienteExist == null)
                {
                    return false;
                }
                db.Update(cliente);
                db.SaveChanges();
            }
            return true;
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
    }
}
