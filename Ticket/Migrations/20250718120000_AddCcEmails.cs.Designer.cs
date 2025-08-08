using Microsoft.EntityFrameworkCore.Migrations;

namespace TicketingApp.Migrations
{
    public partial class AddCcEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CcEmails",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CcEmails",
                table: "Ticket");
        }
    }
}