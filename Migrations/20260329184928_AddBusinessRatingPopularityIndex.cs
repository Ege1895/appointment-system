using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessRatingPopularityIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Businesses_AverageRating_ReviewCount",
                table: "Businesses",
                columns: new[] { "AverageRating", "ReviewCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Businesses_AverageRating_ReviewCount",
                table: "Businesses");
        }
    }
}
