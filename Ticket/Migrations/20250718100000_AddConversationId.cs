using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketingApp.Migrations
{
    public partial class AddConversationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConversationId",
                table: "Ticket",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ConversationId",
                table: "Ticket",
                column: "ConversationId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ticket_ConversationId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "ConversationId",
                table: "Ticket");
        }
    }
}