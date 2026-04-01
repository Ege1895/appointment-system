using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessWorkingDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int[]>(
                name: "WorkingDays",
                table: "Businesses",
                type: "integer[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkingDays",
                table: "Businesses");
        }
    }
}
