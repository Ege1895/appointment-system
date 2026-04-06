using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KvkkAccepted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KvkkVersion",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyPolicyAccepted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyVersion",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Businesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "KvkkAccepted",
                table: "Businesses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KvkkVersion",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyPolicyAccepted",
                table: "Businesses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyVersion",
                table: "Businesses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BusinessId",
                table: "Branches",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BusinessId_IsActive",
                table: "Branches",
                columns: new[] { "BusinessId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BusinessId_IsMain",
                table: "Branches",
                columns: new[] { "BusinessId", "IsMain" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KvkkAccepted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KvkkVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyAccepted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PrivacyVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "KvkkAccepted",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "KvkkVersion",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PrivacyPolicyAccepted",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PrivacyVersion",
                table: "Businesses");
        }
    }
}
