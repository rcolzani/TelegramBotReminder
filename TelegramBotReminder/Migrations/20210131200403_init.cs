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
                name: "Clientes",
                columns: table => new
                {
                    ClientId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TelegramChatId = table.Column<long>(nullable: false),
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
                    table.PrimaryKey("PK_Clientes", x => x.ClientId);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    MensagemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClienteId = table.Column<int>(nullable: false),
                    TextMessage = table.Column<string>(nullable: true),
                    MessageDate = table.Column<DateTime>(nullable: false),
                    MessageSent = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => new { x.MensagemId, x.ClienteId });
                    table.ForeignKey(
                        name: "FK_Mensagens_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ClienteId",
                table: "Mensagens",
                column: "ClienteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
