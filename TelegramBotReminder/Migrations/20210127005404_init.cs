using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TelegramBotReminder.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conversas",
                columns: table => new
                {
                    ClientId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TelegramChatId = table.Column<string>(nullable: true),
                    TextMessage = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    RemindTimeToSend = table.Column<TimeSpan>(nullable: false),
                    LastSend = table.Column<DateTime>(nullable: false),
                    Activated = table.Column<bool>(nullable: false),
                    RiverLevel = table.Column<bool>(nullable: false),
                    StartChat = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversas", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    MensagemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TextMessage = table.Column<string>(nullable: true),
                    MessageDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.MensagemId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conversas");

            migrationBuilder.DropTable(
                name: "Mensagens");
        }
    }
}
