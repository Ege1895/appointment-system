using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIsolationAndUniqueFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeWorkingHours_EmployeeId_DayOfWeek",
                table: "EmployeeWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Email",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Subdomain",
                table: "Businesses");

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId",
                table: "EmployeeWorkingHours",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_BusinessId",
                table: "EmployeeWorkingHours",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_EmployeeId_DayOfWeek",
                table: "EmployeeWorkingHours",
                columns: new[] { "EmployeeId", "DayOfWeek" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Email",
                table: "Businesses",
                column: "Email",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Subdomain",
                table: "Businesses",
                column: "Subdomain",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeWorkingHours_Businesses_BusinessId",
                table: "EmployeeWorkingHours",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeWorkingHours_Businesses_BusinessId",
                table: "EmployeeWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeWorkingHours_BusinessId",
                table: "EmployeeWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeWorkingHours_EmployeeId_DayOfWeek",
                table: "EmployeeWorkingHours");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Email",
                table: "Businesses");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_Subdomain",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "EmployeeWorkingHours");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkingHours_EmployeeId_DayOfWeek",
                table: "EmployeeWorkingHours",
                columns: new[] { "EmployeeId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Email",
                table: "Businesses",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Subdomain",
                table: "Businesses",
                column: "Subdomain",
                unique: true);
        }
    }
}
