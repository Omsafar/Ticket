using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketingApp.Migrations
{
    public partial class AddGraphMessageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GraphMessageId",
                table: "Ticket",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_GraphMessageId",
                table: "Ticket",
                column: "GraphMessageId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ticket_GraphMessageId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "GraphMessageId",
                table: "Ticket");
        }
    }
}