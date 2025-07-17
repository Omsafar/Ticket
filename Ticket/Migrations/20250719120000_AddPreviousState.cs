using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketingApp.Migrations
{
    public partial class AddPreviousState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatoPrecedente",
                table: "Ticket",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatoPrecedente",
                table: "Ticket");
        }
    }
}