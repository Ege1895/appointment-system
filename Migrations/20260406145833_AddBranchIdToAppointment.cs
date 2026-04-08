using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIdToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BranchId",
                table: "Appointments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BusinessId_BranchId",
                table: "Appointments",
                columns: new[] { "BusinessId", "BranchId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Branches_BranchId",
                table: "Appointments",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Branches_BranchId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BranchId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BusinessId_BranchId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Appointments");
        }
    }
}
